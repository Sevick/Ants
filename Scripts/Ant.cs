using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;
using UnityEngine;


//throw new System.NotImplementedException();
public class Ant : Agent {

    static int agentIdCount = 0;
    public int agentID;

    [HideInInspector]
    public Dictionary<LifeResourceType, ITank> resourceMap = new Dictionary<LifeResourceType, ITank>();

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
    public float turnSpeed = 100;
    [Tooltip("Number of ticks agent will be locked for communications")]
    public int communicationLockTime = 20;

    [Header("Rewards configuration")]
    public float DEATH_REWARD = -1.0f;
    public float RESOURCE_CONSUMPTION_REWARD = 0.02f;
    public float EXCHANGE_REWARD = 0.01f;
    [Tooltip("0 to disable hunger reward")]
    public float RESOURCE_SHORTAGE_REWARD = -0.01f;
    [Tooltip("0 to disable satiety reward")]    
    public float RESOURCE_AMPLE_REWARD = 0.0f;

    [Header("Debug")]
    public bool debugComm = false;


    //branch 0
    private int ACTION_SETCOLOR_DONOTHING_IDX = 0;


    private long tickNum = 0;
    private Ant connectedAgent = null;

    public IntercomStateMachine stateMachine;
    private int communicationLockTimeLeft = 0;

    private AntEnvController antEnvController;

    [HideInInspector]
    public int test = 0;

    GameObject foodTanker;
    GameObject waterTanker;


    private void Awake() {
        //Debug.Log("Awake");
        Init();
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

        foreach (KeyValuePair<LifeResourceType, ITank> entry in resourceMap) {
            entry.Value.tick();
            if (RESOURCE_SHORTAGE_REWARD!=0 && (float) entry.Value.currentLevel() / entry.Value.tankCapacity() < 0.1)
                addReward(RESOURCE_SHORTAGE_REWARD);
            
            if (RESOURCE_AMPLE_REWARD!=0 && (float)entry.Value.currentLevel() / entry.Value.tankCapacity() > 0.1)
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

    void Die(ITank sender) {
        //Debug.Log("Die called with message: " + message);
        bodyRenderer.material.SetColor("_Color", new Color32(255, 0, 0, 255));

        addReward(DEATH_REWARD);
        antEnvController.deactivateAgent(this);

        /*
        Ant newAgent = Instantiate(this, new Vector3(Random.Range(-50, 50), 5f, Random.Range(-50, 50)) + area.transform.position, Quaternion.Euler(new Vector3(0f, Random.Range(0, 360))));
        Destroy(gameObject);

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
                    Ant connectedAgent = collision.gameObject.GetComponent<Ant>();
                    var localStateMachine = new IntercomStateMachine();
                    var remoteStateMachine = new IntercomStateMachine(new Context(localStateMachine, connectedAgent.resourceMap, connectedAgent.addReward));
                    localStateMachine.init(new Context(remoteStateMachine, resourceMap, this.addReward));
                    stateMachine = localStateMachine;
                    connectedAgent.stateMachine = remoteStateMachine;
                    communicationLockTimeLeft = communicationLockTime;
                }
                else {
                    // do nothing - already busy with some other communication
                }
                break;
            case "resource":
                Dispenser resourceDispenser = collision.gameObject.GetComponent<Dispenser>();
                if (resourceDispenser != null) {
                    try {
                        ITank tank = resourceMap[resourceDispenser.tankResourceType()];
                        int refilledAmount = tank.pumpFrom(resourceDispenser.tank, tank.tankConsumeLimit());
                        //Debug.Log("Consumed " + refilledAmount + " of " + resourceDispenser.tankResourceType() + " currentLevel=" + tank.currentLevel());
                        if (refilledAmount > 0) {
                            if (Academy.Instance.EnvironmentParameters.GetWithDefault("curriculum_option", defaultSceneOption) > 0) {
                                addReward(RESOURCE_CONSUMPTION_REWARD * refilledAmount / (tank.currentLevel() - refilledAmount));
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

        int setColorCommand = discreteActions[0];
        if (setColorCommand != ACTION_SETCOLOR_DONOTHING_IDX)
            setColor(setColorCommand - 1);

        if (discreteActions.Length > 1) {
            if (stateMachine != null) {
                if (stateMachine.onCommand(new MLCommand(discreteActions.Array)) == null) {
                    connectedAgent = null;
                    stateMachine = null;                    
                }
            }
        }
    }


    public override void OnActionReceived(ActionBuffers actionBuffers) {
        tickNum++;
        if (communicationLockTimeLeft == 0)
            MoveAgent(actionBuffers, false);
            //MoveAgent(actionBuffers.DiscreteActions);
        else {
            MoveAgent(actionBuffers, true);
            //MoveAgent(actionBuffers.DiscreteActions);            
            communicationLockTimeLeft--;
            if (communicationLockTimeLeft == 0)
                if (stateMachine != null) {
                    Debug.Log("Timeout disconnect");
                    stateMachine.onDisconnect();
                    stateMachine = null;
                    connectedAgent = null;
                }
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
        // Example:
        // actionMask.SetActionEnabled(branch, actionIndex, isEnabled);
        // actionMask.SetActionEnabled(branch, new int[4] { 0, 1, 2, 3 }, isEnabled);

        // branch0 = changeColor (0,1,2)
        // branch1 = handshakeCommands(0,1,2,3)  0 - DoNothing, 1 - handshake, 2 - acept, 3 - reject
        // branch2 = commCommands(0,1,2,3) 0 - DoNothing, 1 - askResource, 2 - proposeExchange, 3 - giveResource, 4 - exchange, 5 - reject
        
        for (var i = 1; i <= (int) IIntercomState.IntercomCommands.ACTION_DONOTHING_LAST; i++) {
            actionMask.SetActionEnabled(IntercomState.INTERCOM_ACTIONS_BRANCH, i, false);
        }
        for (var i = 1; i <= (int)IIntercomState.IntercomCommandResponse.ACTION_DONOTHING_LAST; i++) {
            actionMask.SetActionEnabled(IntercomState.INTERCOM_RESPONSES_BRANCH, i, false);
        }

        if (stateMachine != null) {
            stateMachine.WriteDiscreteActionMask(actionMask);
        }

        /*
        for (int i = 1; i <= 10; i = i + 1) {
            actionMask.SetActionEnabled(3, i, resourceExchangeEnabled);
        }

        for (int i = 1; i <= 10; i = i + 1) {
            actionMask.SetActionEnabled(4, i, resourceExchangeEnabled);
        }
        */
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

        /*
        sensor.AddObservation(resourceMap[LifeResourceType.Food].currentLevel() / resourceMap[LifeResourceType.Food].tankCapacity());
        if (Academy.Instance.EnvironmentParameters.GetWithDefault("curriculum_option", defaultSceneOption) >= 1)
            sensor.AddObservation(resourceMap[LifeResourceType.Water].currentLevel() / resourceMap[LifeResourceType.Water].tankCapacity());
        */
        sensor.AddObservation(resourceMap[LifeResourceType.Food].currentLevel());
        sensor.AddObservation(resourceMap[LifeResourceType.Water].currentLevel());

        sensor.AddObservation(communicationLockTimeLeft > 0);

        if (stateMachine!=null)
            sensor.AddOneHotObservation((int) stateMachine.type(), (int) IIntercomState.IntercomStateTypes.STATE_LAST);
        else
            sensor.AddOneHotObservation((int)IIntercomState.IntercomStateTypes.STATE_NULL, (int)IIntercomState.IntercomStateTypes.STATE_LAST);

        //AddObservation(stateMachine.type());
        //sensor.AddOneHotObservation((int)commStatus, Enum.GetValues(typeof(CommStatusEnum)).Length);
    }


    private void Init() {
        agentID = agentIdCount++;

        antEnvController = this.transform.parent.GetComponent<AntEnvController>();
        agentRb = this.GetComponent<Rigidbody>();
        header = this.transform.parent.Find("Header").gameObject.GetComponent<TextMesh>();
        bodyRenderer = this.transform.Find("Body").gameObject.GetComponent<Renderer>();
        area = this.transform.parent.gameObject;

        foodTanker = this.transform.Find("FoodTanker").gameObject;
        waterTanker = this.transform.Find("WaterTanker").gameObject;

        resourceMap = new Dictionary<LifeResourceType, ITank>();
        resourceMap.Add(LifeResourceType.Food, new TankVis(
                new TankLeak(LifeResourceType.Food, FoodTankerVolume, FoodTankerLeak, FoodTankerVolume, eatAmountLimit, eatAmountLimit, Die)
                , foodTanker));

        if (Academy.Instance.EnvironmentParameters.GetWithDefault("curriculum_option", defaultSceneOption) >= 1)
            resourceMap.Add(LifeResourceType.Water, new TankVis(
                new TankLeak(LifeResourceType.Water, WaterTankerVolume, WaterTankerLeak, WaterTankerVolume, drinkAmountLimit, drinkAmountLimit, Die)
                , waterTanker));
    }


    private void SetResetParams() {
        tickNum = 0;
        communicationLockTimeLeft = 0;
        connectedAgent = null;

        //area.GetComponent<GameArea>().reset();
        agentRb.velocity = Vector3.zero;
        setColor(0);
        bodyRenderer.material.SetColor("_Color", new Color32(0, 255, 0, 255));
        //transform.position = new Vector3(Random.Range(-50, 50), 5f, Random.Range(-50, 50)) + area.transform.position;
        //transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360)));

        /*
        this.gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        this.gameObject.transform.Rotate(new Vector3(0, 1, 0), Random.Range(-10f, 10f));
        this.gameObject.transform.Rotate(new Vector3(0, 0, 1), Random.Range(-10f, 10f));
        */

        foreach (KeyValuePair<LifeResourceType, ITank> entry in resourceMap) {
            entry.Value.reset();
        }
    }


    public override void OnEpisodeBegin() {
        //Debug.Log("OnEpisodeBegin");
        SetResetParams();
    }

    public Color32 ToColor(int hexVal) {
        var r = (byte)((hexVal >> 16) & 0xFF);
        var g = (byte)((hexVal >> 8) & 0xFF);
        var b = (byte)(hexVal & 0xFF);
        return new Color32(r, g, b, 255);
    }

    public void OnDrawGizmos() {
        

        if (connectedAgent != null) {
            Debug.Log("DrawConnection");
            Vector3 a = this.gameObject.transform.position;
            Vector3 b = connectedAgent.gameObject.transform.position;
            float h = 5;

            //Draw the parabola by sample a few times
            Gizmos.color = Color.green;
            Gizmos.DrawLine(a, b);
            float count = 20;
            Vector3 lastP = a;
            for (float i = 0; i < count + 1; i++) {
                Vector3 p = SampleParabola(a, b, h, i / count, this.gameObject.transform.up);
                Gizmos.color = i % 2 == 0 ? Color.blue : Color.green;
                Gizmos.DrawLine(lastP, p);
                lastP = p;
            }
        }
    }

    Vector3 SampleParabola(Vector3 start, Vector3 end, float height, float t, Vector3 outDirection) {
        float parabolicT = t * 2 - 1;
        //start and end are not level, gets more complicated
        Vector3 travelDirection = end - start;
        Vector3 levelDirection = end - new Vector3(start.x, end.y, start.z);
        Vector3 right = Vector3.Cross(travelDirection, levelDirection);
        Vector3 up = outDirection;
        Vector3 result = start + t * travelDirection;
        result += ((-parabolicT * parabolicT + 1) * height) * up.normalized;
        return result;
    }
}