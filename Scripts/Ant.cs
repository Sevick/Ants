using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;
using UnityEngine;

public class Ant : Agent, IIntercom {


    IntercomStateMachine intercomStateMachine = new IntercomStateMachine();

    enum CommStatusEnum { NONE, COLLIDED, HANDSHAKE, CONNECTED, WAITING_FOR_RESPONSE };

    static int agentIdCount = 0;
    public int agentID;

    CommStatusEnum commStatus = CommStatusEnum.NONE;

    [HideInInspector]
    public Dictionary<LifeResourceType, Tank> resourceMap = new Dictionary<LifeResourceType, Tank>();

    private GameObject area;
    private Rigidbody agentRb;
    private TextMesh header;
    private Renderer bodyRenderer;

    [Tooltip("Lesson to set if curriculum learning is not configured")]
    public int defaultSceneOption = 2;
    public bool useGroup = true;

    [Header("Food Tank configuration")]
    [Tooltip("Volume of internal Tank for Food")]
    public int FoodTankerVolume = 150;
    [Tooltip("About of Food used each tick")]
    public int FoodTankerLeak = 1;
    [Tooltip("Amount of Food can be transfered at once")]
    public int eatAmountLimit = 30;

    [Header("Water Tank configuration")]
    [Tooltip("Volume of internal Tank for Water")]
    public int WaterTankerVolume = 250;
    [Tooltip("About of Water used each tick")]
    public int WaterTankerLeak = 1;
    [Tooltip("Amount of Water can be transfered at once")]
    public int drinkAmountLimit = 50;

    [Header("Movement configuration")]
    public float moveSpeed = 2;
    public float turnSpeed = 300;
    [Tooltip("Number of ticks agent will be locked for communications")]
    public int communicationLockTime = 20;

    [Header("Rewards configuration")]
    public float DEATH_REWARD = -1.0f;
    public float RESOURCE_CONSUMPTION_REWARD = 0.5f;
    public float EXCHANGE_REWARD = 0.1f;
    [Tooltip("0 to disable hunger reward")]
    public float RESOURCE_SHORTAGE_REWARD = -0.0f;
    [Tooltip("0 to disable satiety reward")]    
    public float RESOURCE_AMPLE_REWARD = 0.0f;

    [Header("Debug")]
    public bool debugComm = false;


    //branch 0
    private int ACTION_SETCOLOR_DONOTHING_IDX = 0;

    //branch 1
    private int ACTION_HANDSHAKE_DONOTHING_IDX = 0;
    private int ACTION_HANDSHAKE_IDX = 1;
    private int ACTION_HANDSHAKE_ACCEPT_IDX = 2;
    private int ACTION_HANDSHAKE_REJECT_IDX = 3;

    //branch 2
    private int ACTION_COMM_DONOTHING_IDX = 0;
    private int ACTION_COMM_ACCEPT_IDX = 1;
    private int ACTION_COMM_REJECT_IDX = 2;

    //branch 3 PROPOSE_EXCHANGE water->food
    // >0 - % of resource for exchange (1step - 10%)

    //branch 4 PROPOSE_EXCHANGE food->water
    // >0 - % of resource for exchange (1step - 10%)

    private HashSet<int> handshakeBranchActions = new HashSet<int>();
    private HashSet<int> commBranchActions = new HashSet<int>();

    private HashSet<int> handshakeBranchEnabledActions = new HashSet<int>();
    private HashSet<int> commBranchEnabledActions = new HashSet<int>();

    //private int InternalState[];

    private long tickNum = 0;
    private Ant connectedAgent = null;
    private int communicationLockTimeLeft;

    private bool resourceExchangeEnabled = false;
    private int waterProposedForExchange = 0;
    private int foodProposedForExchange = 0;

    private AntEnvController antEnvController;

    [HideInInspector]
    public int test = 0;

    private void Awake() {
        //Debug.Log("Awake");
        Init();
        //area.GetComponent<GameArea>().awake();
        SetResetParams();
    }

    void Start() {
        //Debug.Log("Ant: Start");
        Init();
    }



    // Update is called once per frame
    void Update() {
        //RaycastHit mouseHit;
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //if (Physics.Raycast(ray, out mouseHit, 100) && mouseHit.collider.gameObject.name == this.gameObject.name) {
        //    Debug.Log("Mouse is pointing to me! Im a choosen one!");
        //}

        if (connectedAgent == null)
            setColor(0);
        else
            setColor(1);

        foreach (KeyValuePair<LifeResourceType, Tank> entry in resourceMap) {
            entry.Value.tick();
            if (RESOURCE_SHORTAGE_REWARD!=0 && (float) entry.Value.currentVolume / entry.Value.capacity < 0.1)
                addReward(RESOURCE_SHORTAGE_REWARD);
            
            if (RESOURCE_AMPLE_REWARD!=0 && (float)entry.Value.currentVolume / entry.Value.capacity > 0.1)
                addReward(RESOURCE_AMPLE_REWARD);
        }

        /*
        if (Academy.Instance.EnvironmentParameters.GetWithDefault("curriculum_option", defaultSceneOption) >= 1)
            header.text = "Food: " + resourceMap[LifeResourceType.Food].currentVolume + "\nWater:" + resourceMap[LifeResourceType.Water].currentVolume;
        else
            header.text = "Food: " + resourceMap[LifeResourceType.Food].currentVolume;
        */
    }

    // Called automatically by Unity once every Physics step.
    void FixedUpdate() {
    }

    void addReward(float score) {
        if (useGroup) {
            //agentGroup.AddGroupReward(score);
            antEnvController.Score(score);
        }
        AddReward(score);
    }

    void Die(string message) {
        //Debug.Log("Die called with message: " + message);
        addReward(DEATH_REWARD);

        //Instantiate(this, new Vector3(Random.Range(-50, 50), 5f, Random.Range(-50, 50)) + area.transform.position, Quaternion.Euler(new Vector3(0f, Random.Range(0, 360))));


        antEnvController.deactivateAgent(this);
        /*
        this.gameObject.SetActive(false);
        this.gameObject.SetActive(true);
        if (useGroup) {
            antEnvController.agentGroup.RegisterAgent(this);
            //antEnvController.agentGroup.EndGroupEpisode();
            //Destroy(gameObject);
        }
        else {
            EndEpisode();
        }
        */
        //bodyRenderer.material.SetColor("_Color", new Color32(255, 0, 0, 255));
    }



    void OnCollisionExit(Collision collision) {
        /*
        switch (collision.gameObject.tag) {
            case "agent":
                disconnectAgent();
                break;
        }
        */
    }


    void OnCollisionEnter(Collision collision) {

        switch (collision.gameObject.tag) {
            case "wall":
                addReward(-0.1f);
                break;
            case "agent":
                if (connectedAgent == null) {
                    connectedAgent = collision.gameObject.GetComponent<Ant>();
                    communicationLockTimeLeft = communicationLockTime;
                    handshakeBranchEnabledActions.Clear();
                    handshakeBranchEnabledActions.Add(ACTION_HANDSHAKE_IDX);
                }
                else {
                    // do nothing - already busy with some other communication
                }
                break;
            case "resource":
                ResourceProvider resourceProvider = collision.gameObject.GetComponent<ResourceProvider>();
                if (resourceProvider != null) {
                    try {
                        Tank tank = resourceMap[resourceProvider.lifeResource];
                        int refilledAmount = tank.refill(resourceProvider);
                        //Debug.Log("Consumed " + refilledAmount + " of " + resourceProvider.lifeResource + " currentLevel=" + tank.currentVolume);
                        if (refilledAmount > 0) {
                            if (Academy.Instance.EnvironmentParameters.GetWithDefault("curriculum_option", defaultSceneOption) > 0) {
                                addReward(RESOURCE_CONSUMPTION_REWARD * refilledAmount / (tank.currentVolume - refilledAmount));
                            }
                            else {
                                addReward(1.0f);
                                EndEpisode();
                            }

                        }
                    }
                    catch (KeyNotFoundException) {
                        // There is no tank for such type of resource
                    }
                }
                break;
        }
    }


    public void MoveAgent(ActionBuffers actionBuffers, bool isLocked) {
        var continuousActions = actionBuffers.ContinuousActions;
        var discreteActions = actionBuffers.DiscreteActions;

        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;
        var forward = Mathf.Clamp(continuousActions[0], -1f, 1f);
        var right = Mathf.Clamp(continuousActions[1], -1f, 1f);
        var rotate = Mathf.Clamp(continuousActions[2], -1f, 1f);
        dirToGo = transform.forward * forward;
        dirToGo += transform.right * right;
        rotateDir = -transform.up * rotate;
        if (!isLocked) {
            agentRb.AddForce(dirToGo * moveSpeed, ForceMode.VelocityChange);
            transform.Rotate(rotateDir, Time.fixedDeltaTime * turnSpeed);
            // slow it down
            if (agentRb.velocity.sqrMagnitude > 25f) {
                agentRb.velocity *= 0.95f;
            }
        }

        if (continuousActions[2] != 0) {
            //AddReward(-0.01f); // rotate penalty
        };

        var setColorCommand = discreteActions[0];
        var handshakeCommand = discreteActions[1];
        var commCommand = discreteActions[2];
        var commExchWaterFood = discreteActions[3];
        var commExchFoodWater = discreteActions[4];
        //var askResourceCommand = discreteActions[2];
        //var proposeExchangeCommand = discreteActions[3];

        //if (setColorCommand != ACTION_SETCOLOR_DONOTHING_IDX)
        //    setColor(setColorCommand - 1);

        if (handshakeCommand == ACTION_HANDSHAKE_IDX)
            handshakeAction();
        if (handshakeCommand == ACTION_HANDSHAKE_ACCEPT_IDX)
            handshakeAcceptAction();
        if (handshakeCommand == ACTION_HANDSHAKE_REJECT_IDX)
            handshakeRejectAction();

        if (commCommand == ACTION_COMM_ACCEPT_IDX)
            proposeExchangeAccept();
        if (commCommand == ACTION_COMM_REJECT_IDX)
            proposeExchangeReject();

        if (commExchWaterFood > 0)
            proposeExchangeWaterFood(commExchWaterFood);
        if (commExchFoodWater > 0)
            proposeExchangeFoodWater(commExchFoodWater);

        //AddReward(0.0001f);
    }


    public override void OnActionReceived(ActionBuffers actionBuffers) {
        tickNum++;
        if (communicationLockTimeLeft == 0)
            MoveAgent(actionBuffers, false);
        //MoveAgent(actionBuffers.DiscreteActions);
        else {
            MoveAgent(actionBuffers, true);
            communicationLockTimeLeft--;
            if (communicationLockTimeLeft == 0)
                disconnectAgent();
        }
    }


    public void MoveAgent(ActionSegment<int> act) {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action = act[0];

        switch (action) {
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
            case 3:
                rotateDir = transform.up * 1f;
                //AddReward(-0.1f);
                break;
            case 4:
                rotateDir = transform.up * -1f;
                //AddReward(-0.1f);
                break;
            case 5:
                // do nothing
                break;
            case 7:
                setColor(0);
                break;
            case 8:
                setColor(1);
                break;
            case 9:
                setColor(2);
                break;
        }
        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);
        agentRb.AddForce(dirToGo * moveSpeed,
            ForceMode.VelocityChange);

        // slow it down
        if (agentRb.velocity.sqrMagnitude > 25f) {
            agentRb.velocity *= 0.95f;
        }
    }


    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask) {

        // branch0 = changeColor (0,1,2)
        // branch1 = handshakeCommands(0,1,2,3)  0 - DoNothing, 1 - handshake, 2 - acept, 3 - reject
        // branch2 = commCommands(0,1,2,3) 0 - DoNothing, 1 - askResource, 2 - proposeExchange, 3 - giveResource, 4 - exchange, 5 - reject

        // actionMask.SetActionEnabled(branch, actionIndex, isEnabled);
        // actionMask.SetActionEnabled(branch, new int[4] { 0, 1, 2, 3 }, isEnabled);

        actionMask.SetActionEnabled(1, ACTION_HANDSHAKE_DONOTHING_IDX, true);   // cannot disable whole branch
        foreach (int i in handshakeBranchActions) {
            actionMask.SetActionEnabled(1, i, handshakeBranchEnabledActions.Contains(i));
        }

        actionMask.SetActionEnabled(1, ACTION_COMM_DONOTHING_IDX, true);    // cannot disable whole branch
        foreach (int i in commBranchActions) {
            actionMask.SetActionEnabled(2, i, commBranchEnabledActions.Contains(i));
        }


        for (int i = 1; i <= 10; i = i + 1) {
            actionMask.SetActionEnabled(3, i, resourceExchangeEnabled);
        }

        for (int i = 1; i <= 10; i = i + 1) {
            actionMask.SetActionEnabled(4, i, resourceExchangeEnabled);
        }
    }



    private void setColor(int colorIdx) {
        switch (colorIdx) {
            case 0:
                bodyRenderer.material.SetColor("_Color", new Color32(255, 0, 0, 255));
                break;
            case 1:
                bodyRenderer.material.SetColor("_Color", new Color32(255, 255, 0, 255));
                break;
            case 2:
                bodyRenderer.material.SetColor("_Color", new Color32(255, 0, 255, 255));
                break;
        }
    }


    public void handshakeAction() {
        if (debugComm)
            Debug.Log("AgentID = " + agentID + " handshakeAction");
        communicationLockTimeLeft = communicationLockTime;
        commStatus = CommStatusEnum.HANDSHAKE;
        if (connectedAgent != null)
            connectedAgent.handshakeReceiver(this);
        handshakeBranchEnabledActions.Clear();
    }

    public void handshakeAcceptAction() {
        if (debugComm)
            Debug.Log("AgentID = " + agentID + " handshakeAceptAction");
        handshakeBranchEnabledActions.Clear();
        communicationLockTimeLeft = communicationLockTime;
        commStatus = CommStatusEnum.CONNECTED;
        if (connectedAgent != null)
            connectedAgent.handshakeResponseReceiver(IIntercom.ResponseCode.ACCEPT);
    }

    public void handshakeRejectAction() {
        if (debugComm)
            Debug.Log("AgentID = " + agentID + " handshakeRejectAction");
        handshakeBranchEnabledActions.Clear();
        if (connectedAgent != null)
            connectedAgent.handshakeResponseReceiver(IIntercom.ResponseCode.REJECT);
        communicationLockTimeLeft = 0;
    }


    // ask other agent for resource
    public void askResourceAction() {
        if (debugComm)
            Debug.Log("AgentID = " + agentID + " askResourceAction");
    }

    // response to askResource from other agent
    private void giveResourceAction() {
        if (debugComm)
            Debug.Log("AgentID = " + agentID + " giveResourceAction");
    }


    //propose exchange water for food to another agent
    private void proposeExchangeWaterFood(int amountToPropose) {
        if (debugComm)
            Debug.Log("AgentID = " + agentID + " proposeExchangeWaterFood");
        if (connectedAgent != null)
            connectedAgent.exchangeWaterFoodReceiver((int) (1.0f*amountToPropose/10 * resourceMap[LifeResourceType.Water].currentVolume));
    }

    //propose exchange food for water to another agent
    private void proposeExchangeFoodWater(int amountToPropose) {
        if (debugComm)
            Debug.Log("AgentID = " + agentID + " proposeExchangeFoodWater");
        if (connectedAgent != null)
            connectedAgent.exchangeFoodWaterReceiver((int) (1.0f * amountToPropose / 10 * resourceMap[LifeResourceType.Food].currentVolume));
    }

    //accept exchange proposed by other agent
    private void proposeExchangeAccept() {
        if (connectedAgent == null)
            return;
        Debug.Log("AgentID = " + agentID + " proposeExchangeAccept, partner - " + connectedAgent.agentID);
        connectedAgent.exchangeResponseReceiver(IIntercom.ResponseCode.ACCEPT);

        if (foodProposedForExchange > 0) {
            if (connectedAgent != null) {
                var foodAmount = connectedAgent.resourceMap[LifeResourceType.Food].currentVolume >= foodProposedForExchange ?
                    foodProposedForExchange : connectedAgent.resourceMap[LifeResourceType.Food].currentVolume;

                if (resourceMap[LifeResourceType.Water].currentVolume + foodAmount > resourceMap[LifeResourceType.Water].capacity)
                    foodAmount = resourceMap[LifeResourceType.Water].spaceLeft();

                connectedAgent.resourceMap[LifeResourceType.Food].currentVolume -= foodAmount;
                resourceMap[LifeResourceType.Water].currentVolume += foodAmount;
            }
            else {
                // exception
            }
        }

        if (waterProposedForExchange > 0) {
            if (connectedAgent != null) {
                var waterAmount = connectedAgent.resourceMap[LifeResourceType.Water].currentVolume >= waterProposedForExchange ?
                    waterProposedForExchange : connectedAgent.resourceMap[LifeResourceType.Water].currentVolume;
                
                connectedAgent.resourceMap[LifeResourceType.Water].currentVolume -= waterAmount;

                resourceMap[LifeResourceType.Food].currentVolume = resourceMap[LifeResourceType.Food].currentVolume + waterAmount > resourceMap[LifeResourceType.Food].capacity ?
                                                                        resourceMap[LifeResourceType.Food].capacity : resourceMap[LifeResourceType.Food].currentVolume + waterAmount;
            }
            else {
                // exception
            }
        }

        disconnectAgent();
    }

    //accept exchange proposed by other agent
    private void proposeExchangeReject() {
        if (debugComm)
            Debug.Log("AgentID = " + agentID + " proposeExchangeReject");
        if (connectedAgent != null) {
            connectedAgent.exchangeResponseReceiver(IIntercom.ResponseCode.REJECT);
            disconnectAgent();
        }
    }


    public override void Heuristic(in ActionBuffers actionsOut) {
        heuristicContinuous(actionsOut);
        //heuristicDiscrete(actionsOut);
    }


    private void heuristicDiscrete(in ActionBuffers actionsOut) {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.D)) {
            discreteActionsOut[0] = 3;
        }
        if (Input.GetKey(KeyCode.W)) {
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.A)) {
            discreteActionsOut[0] = 4;
        }
        if (Input.GetKey(KeyCode.S)) {
            discreteActionsOut[0] = 2;
        }
        //discreteActionsOut[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }


    private void heuristicContinuous(in ActionBuffers actionsOut) {
        var continuousActionsOut = actionsOut.ContinuousActions;
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.D)) {
            continuousActionsOut[2] = 1;
        }
        if (Input.GetKey(KeyCode.W)) {
            continuousActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.A)) {
            continuousActionsOut[2] = -1;
        }
        if (Input.GetKey(KeyCode.S)) {
            continuousActionsOut[0] = -1;
        }
        if (Input.GetKey(KeyCode.Alpha1)) {
            discreteActionsOut[0] = -1;
        }
        if (Input.GetKey(KeyCode.Alpha2)) {
            discreteActionsOut[0] = 0;
        }
        if (Input.GetKey(KeyCode.Alpha3)) {
            discreteActionsOut[0] = 1;
        }
    }


    public override void CollectObservations(VectorSensor sensor) {
        var localVelocity = transform.InverseTransformDirection(agentRb.velocity);
        sensor.AddObservation(localVelocity.x);
        sensor.AddObservation(localVelocity.z);
        sensor.AddObservation(transform.rotation.y);
        //sensor.AddObservation(transform.rotation);
        //sensor.AddObservation(transform.forward);
        sensor.AddObservation(transform.position.x - area.transform.position.x);
        sensor.AddObservation(transform.position.z - area.transform.position.z);
        //sensor.AddObservation(transform.position - area.transform.position);

        if (communicationLockTimeLeft > 0)
            sensor.AddObservation(true);
        else
            sensor.AddObservation(false);

        sensor.AddObservation(waterProposedForExchange / 10);
        sensor.AddObservation(foodProposedForExchange / 10);

        sensor.AddObservation(resourceMap[LifeResourceType.Food].currentVolume / resourceMap[LifeResourceType.Food].capacity);
        if (Academy.Instance.EnvironmentParameters.GetWithDefault("curriculum_option", defaultSceneOption) >= 1)
            sensor.AddObservation(resourceMap[LifeResourceType.Water].currentVolume / resourceMap[LifeResourceType.Water].capacity);

        sensor.AddOneHotObservation((int)commStatus, Enum.GetValues(typeof(CommStatusEnum)).Length);
    }


    private void Init() {
        agentID = agentIdCount++;

        handshakeBranchActions.Add(ACTION_HANDSHAKE_IDX);
        handshakeBranchActions.Add(ACTION_HANDSHAKE_ACCEPT_IDX);
        handshakeBranchActions.Add(ACTION_HANDSHAKE_REJECT_IDX);

        commBranchActions.Add(ACTION_COMM_ACCEPT_IDX);
        commBranchActions.Add(ACTION_COMM_REJECT_IDX);

        antEnvController = this.transform.parent.GetComponent<AntEnvController>();

        agentRb = this.GetComponent<Rigidbody>();
        header = this.transform.parent.Find("Header").gameObject.GetComponent<TextMesh>();
        bodyRenderer = this.GetComponent<Renderer>();
        area = this.transform.parent.gameObject;

        resourceMap = new Dictionary<LifeResourceType, Tank>();
        resourceMap.Add(LifeResourceType.Food, new Tank(LifeResourceType.Food, FoodTankerVolume, eatAmountLimit, FoodTankerLeak, Die));
        if (Academy.Instance.EnvironmentParameters.GetWithDefault("curriculum_option", defaultSceneOption) >= 1)
            resourceMap.Add(LifeResourceType.Water, new Tank(LifeResourceType.Water, FoodTankerVolume, drinkAmountLimit, WaterTankerLeak, Die));
    }


    private void SetResetParams() {
        commStatus = CommStatusEnum.NONE;
        tickNum = 0;
        communicationLockTimeLeft = 0;
        connectedAgent = null;
        waterProposedForExchange = 0;
        foodProposedForExchange = 0;
        resourceExchangeEnabled = false;
        handshakeBranchEnabledActions.Clear();
        commBranchEnabledActions.Clear();
        //area.GetComponent<GameArea>().reset();
        agentRb.velocity = Vector3.zero;
        setColor(0);
        //bodyRenderer.material.SetColor("_Color", new Color32(0, 255, 0, 255));
        //transform.position = new Vector3(Random.Range(-50, 50), 5f, Random.Range(-50, 50)) + area.transform.position;
        //transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));

        /*
        this.gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        this.gameObject.transform.Rotate(new Vector3(0, 1, 0), Random.Range(-10f, 10f));
        this.gameObject.transform.Rotate(new Vector3(0, 0, 1), Random.Range(-10f, 10f));
        */

        foreach (KeyValuePair<LifeResourceType, Tank> entry in resourceMap) {
            entry.Value.reset();
        }
    }


    public override void OnEpisodeBegin() {
        //Debug.Log("OnEpisodeBegin");
        SetResetParams();
    }


    public void handshakeReceiver(Ant agent1) {
        if (debugComm)
            Debug.Log("AgentID = " + agentID + " handshakeReceiver +   request from "+agent1.agentID);
        if (connectedAgent == null) {
            connectedAgent = agent1;
            communicationLockTimeLeft = communicationLockTime;
            handshakeBranchEnabledActions.Clear();
            handshakeBranchEnabledActions.Add(ACTION_HANDSHAKE_ACCEPT_IDX);
            handshakeBranchEnabledActions.Add(ACTION_HANDSHAKE_REJECT_IDX);
            commStatus = CommStatusEnum.WAITING_FOR_RESPONSE;
        }
    }


    public void handshakeResponseReceiver(IIntercom.ResponseCode responseCode) {
        if (debugComm)
            Debug.Log("AgentID = " + agentID + " handshakeResponseReceiver");
        if (responseCode == IIntercom.ResponseCode.ACCEPT)
            resourceExchangeEnabled = true;
        else {
            disconnectAgent();
        }
    }


    private void disconnectAgent(bool sendDisconnectSignal = true) {
        resourceExchangeEnabled = false;
        handshakeBranchEnabledActions.Clear();
        commBranchEnabledActions.Clear();
        if (connectedAgent != null && sendDisconnectSignal) {
            connectedAgent.diconnectReceiver();
        }
        connectedAgent = null;
        communicationLockTimeLeft = 0;
        commStatus = CommStatusEnum.NONE;
    }

    public void diconnectReceiver() {
        if (connectedAgent != null)
            disconnectAgent(false);
    }

    public void askForResourceReceiver() {
        if (debugComm)
            Debug.Log("AgentID = " + agentID + " askForResourceReceiver");
    }

    public void exchangeWaterFoodReceiver(int amount) {
        if (debugComm)
            Debug.Log("AgentID = " + agentID + " exchangeWaterFoodReceiver");
        waterProposedForExchange = amount;
        commBranchEnabledActions.Clear();
        commBranchEnabledActions.Add(ACTION_COMM_ACCEPT_IDX);
        commBranchEnabledActions.Add(ACTION_COMM_REJECT_IDX);
        setColor(2);
        commStatus = CommStatusEnum.WAITING_FOR_RESPONSE;
    }

    public void exchangeFoodWaterReceiver(int amount) {
        if (debugComm)
            Debug.Log("AgentID = " + agentID + " exchangeFoodWaterReceiver");
        foodProposedForExchange = amount;
        commBranchEnabledActions.Clear();
        commBranchEnabledActions.Add(ACTION_COMM_ACCEPT_IDX);
        commBranchEnabledActions.Add(ACTION_COMM_REJECT_IDX);
        setColor(2);
        commStatus = CommStatusEnum.WAITING_FOR_RESPONSE;
    }


    public void exchangeResponseReceiver(IIntercom.ResponseCode responseCode) {
        if (debugComm)
            Debug.Log("AgentID = " + agentID + " exchangeResponseReceiver");
        if (responseCode == IIntercom.ResponseCode.ACCEPT) {
            Debug.Log("AgentID = " + agentID + " exchange accepted, partner - " + connectedAgent.agentID);
            addReward(EXCHANGE_REWARD);
        }
        else {
            //
        }
        disconnectAgent();
    }


    public Color32 ToColor(int hexVal) {
        var r = (byte)((hexVal >> 16) & 0xFF);
        var g = (byte)((hexVal >> 8) & 0xFF);
        var b = (byte)(hexVal & 0xFF);
        return new Color32(r, g, b, 255);
    }
}
