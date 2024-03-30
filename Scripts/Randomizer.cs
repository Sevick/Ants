using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Randomizer : MonoBehaviour
{
    private long tickCount;
    private long moveLockLeft;
    private AntEnvController envController; // use to get random positions

    [Tooltip("Object will move if random generator hits zero. This option defined random high limit.")]
    public int randomHighBound = 10000;
    [Tooltip("Minimal number of ticks between repositions")]
    public int moveLockTime = 1000;

    [Header("Debug")]
    public bool debugLog = false;

    private void DebugLog(string log) {
        if (debugLog) {
            Debug.Log(name + " " + log);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        tickCount = 0;
        envController = this.transform.parent.GetComponent<AntEnvController>();
    }

    // Update is called once per frame
    void Update()
    {
        tickCount++;
        if (moveLockLeft == 0) {
            // could be replaced with generation of next move tick# for performace (but worse for simulation)
            if (Random.Range(0, randomHighBound) == 0) {
                DebugLog("moved");
                this.transform.transform.SetPositionAndRotation(envController.GetRandomSpawnPos(), envController.GetRandomRot());
                moveLockLeft = moveLockTime; 
            }
        }
        else {
            moveLockLeft--;
        }
    }
}
