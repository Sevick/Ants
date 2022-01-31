using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class AntEnvController : MonoBehaviour {
    [System.Serializable]
    public class PlayerInfo {
        public Ant Agent;
        [HideInInspector]
        public Vector3 StartingPos;
        [HideInInspector]
        public Quaternion StartingRot;
        [HideInInspector]
        public Rigidbody Rb;

        public PlayerInfo(Ant Agent) {
            this.Agent = Agent;
            this.StartingPos = Agent.gameObject.transform.position;
            this.StartingRot = Agent.gameObject.transform.rotation;
            this.Rb = Agent.gameObject.GetComponent<Rigidbody>();
        }
    }

    [System.Serializable]
    public class ResourceProviderInfo {
        public Transform transform;
        [HideInInspector]
        public Vector3 StartingPos;
        [HideInInspector]
        public Quaternion StartingRot;
        [HideInInspector]
        public Rigidbody Rb;

        /*
        public ResourceProviderInfo(ResourceProvider resourceProvider) {
            this.transform = resourceProvider.gameObject.transform;
            this.StartingPos = resourceProvider.gameObject.transform.position;
            this.StartingRot = resourceProvider.gameObject.transform.rotation;
            this.Rb = resourceProvider.gameObject.GetComponent<Rigidbody>();
        }
        */
    }

    [Header("Max Environment Steps")]
    [Tooltip("Max Academy steps before this platform resets")]
    public int MaxEnvironmentSteps = 25000;

    [Tooltip("The spawn area margin multiplier. ex: .9 means 90% of spawn area will be used." +
        ".1 margin will be left (so players don't spawn off of the edge). The higher this value, the longer training time required.")]
    public float spawnAreaMarginMultiplier;

    [Tooltip("When a goal is scored the ground will switch to this material for a few seconds.")]
    public Material goalScoredMaterial;

    [Tooltip("When an agent fails, the ground will turn this material for a few seconds.")]
    public Material failMaterial;

    /// The area bounds.
    [HideInInspector]
    public Bounds areaBounds;

    private GameObject ground;
    private GameObject area;

    Material m_GroundMaterial; //cached on Awake()

    /// We will be changing the ground material based on success/failue
    Renderer m_GroundRenderer;

    //List of Agents On Platform
    public List<PlayerInfo> AgentsList = new List<PlayerInfo>();
    //List of ResourceProviders in the area
    public List<ResourceProviderInfo> resourceProvidersList = new List<ResourceProviderInfo>();

    public bool UseRandomAgentRotation = true;
    public bool UseRandomAgentPosition = true;
    public bool UseRandomBlockRotation = true;
    public bool UseRandomBlockPosition = true;

    [HideInInspector]
    public SimpleMultiAgentGroup agentGroup;

    private int m_ResetTimer;
    private int agentsAlive;
    private int agentsCount = 0;

    void Start() {
        Debug.Log("AntEnvController: Start");

        // Initialize TeamManager
        agentGroup = new SimpleMultiAgentGroup();

        area = this.gameObject;
        ground = this.transform.Find("Ground").gameObject;
        // Get the ground's bounds
        areaBounds = ground.GetComponent<Collider>().bounds;
        // Get the ground renderer so we can change the material when a goal is scored
        m_GroundRenderer = ground.GetComponent<Renderer>();
        // Starting material
        m_GroundMaterial = m_GroundRenderer.material;


        Ant[] players = GetComponentsInChildren<Ant>();
        foreach (var player in players) {
            AgentsList.Add(new PlayerInfo(player));
            agentsCount++;
        }
        agentsAlive = agentsCount;

        /*
        ResourceProvider[] resourceProviders = GetComponentsInChildren<ResourceProvider>();
        foreach (var resourceProvider in resourceProviders) {
            resourceProvidersList.Add(new ResourceProviderInfo(resourceProvider));
        }
        */

        foreach (var item in resourceProvidersList) {
            item.StartingPos = item.transform.transform.position;
            item.StartingRot = item.transform.transform.rotation;
            item.Rb = item.transform.GetComponent<Rigidbody>();
        }

        foreach (var item in AgentsList) {
            item.StartingPos = item.Agent.transform.position;
            item.StartingRot = item.Agent.transform.rotation;
            item.Rb = item.Agent.GetComponent<Rigidbody>();
            agentGroup.RegisterAgent(item.Agent);
        }
        ResetScene();
    }

    void FixedUpdate() {
        m_ResetTimer += 1;
        if (m_ResetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0) {
            agentGroup.GroupEpisodeInterrupted();
            ResetScene();
        }

        //Hurry Up Penalty
        //agentGroup.AddGroupReward(-0.5f / MaxEnvironmentSteps);
    }

    /// Use the ground's bounds to pick a random spawn position.
    public Vector3 GetRandomSpawnPos() {
        var foundNewSpawnLocation = false;
        var randomSpawnPos = Vector3.zero;
        while (foundNewSpawnLocation == false) {
            var randomPosX = Random.Range(-areaBounds.extents.x * spawnAreaMarginMultiplier,
                areaBounds.extents.x * spawnAreaMarginMultiplier);

            var randomPosZ = Random.Range(-areaBounds.extents.z * spawnAreaMarginMultiplier,
                areaBounds.extents.z * spawnAreaMarginMultiplier);
            randomSpawnPos = ground.transform.position + new Vector3(randomPosX, 0.1f, randomPosZ);
            if (Physics.CheckBox(randomSpawnPos, new Vector3(1.5f, 0.01f, 1.5f)) == false) {
                foundNewSpawnLocation = true;
            }
        }
        return randomSpawnPos;
    }

    /// Resets the block position and velocities.
    void ResetResourceProvider(ResourceProviderInfo resourceProvider) {
        // Get a random position for the block.
        resourceProvider.transform.position = GetRandomSpawnPos();

        // Reset block velocity back to zero.
        resourceProvider.Rb.velocity = Vector3.zero;

        // Reset block angularVelocity back to zero.
        resourceProvider.Rb.angularVelocity = Vector3.zero;
    }

    /// Swap ground material, wait time seconds, then swap back to the regular material.
    IEnumerator GoalScoredSwapGroundMaterial(Material mat, float time) {
        m_GroundRenderer.material = mat;
        yield return new WaitForSeconds(time); // Wait for 2 sec
        m_GroundRenderer.material = m_GroundMaterial;
    }



    public void Score(float score) {
        agentGroup.AddGroupReward(score);
    }


    /// Called when the agent moves the block into the goal.
    public void ScoredAGoal(Collider col, float score) {
        Debug.Log($"Scored {score} on {gameObject.name}");

        //Disable the block
        col.gameObject.SetActive(false);

        //Give Agent Rewards
        agentGroup.AddGroupReward(score);

        // Swap ground material for a bit to indicate we scored.
        StartCoroutine(GoalScoredSwapGroundMaterial(goalScoredMaterial, 0.5f));

        var done = false;
        if (done) {
            //Reset assets
            agentGroup.EndGroupEpisode();
            ResetScene();
        }
    }

    Quaternion GetRandomRot() {
        return Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0);
    }

    public void ResetScene() {
        agentGroup = new SimpleMultiAgentGroup();
        agentsAlive = agentsCount;
        m_ResetTimer = 0;

        //Random platform rotation
        var rotation = Random.Range(0, 4);
        var rotationAngle = rotation * 90f;
        area.transform.Rotate(new Vector3(0f, rotationAngle, 0f));

        //Reset Agents
        foreach (var item in AgentsList) {
            var pos = UseRandomAgentPosition ? GetRandomSpawnPos() : item.StartingPos;
            var rot = UseRandomAgentRotation ? GetRandomRot() : item.StartingRot;

            if (item.Agent.gameObject.activeSelf)
                item.Agent.gameObject.SetActive(false);

            item.Agent.gameObject.SetActive(true);
            agentGroup.RegisterAgent(item.Agent);

            item.Agent.transform.SetPositionAndRotation(pos, rot);
            item.Rb.velocity = Vector3.zero;
            item.Rb.angularVelocity = Vector3.zero;
        }

        //Reset Blocks
        foreach (var item in resourceProvidersList) {
            var pos = UseRandomBlockPosition ? GetRandomSpawnPos() : item.StartingPos;
            var rot = UseRandomBlockRotation ? GetRandomRot() : item.StartingRot;

            item.transform.transform.SetPositionAndRotation(pos, rot);
            item.Rb.velocity = Vector3.zero;
            item.Rb.angularVelocity = Vector3.zero;
            item.transform.gameObject.SetActive(true);
        }

    }

    public void deactivateAgent(Ant agent) {
        agentGroup.GroupEpisodeInterrupted();
        
        /*
        StartCoroutine(GoalScoredSwapGroundMaterial(goalScoredMaterial, 0.5f));
        agent.gameObject.SetActive(false);
        agentsAlive--;
        if (agentsAlive == 0) {
            //agentGroup.EndGroupEpisode();
            agentGroup.GroupEpisodeInterrupted();
            ResetScene();
        }
        */
    }
}