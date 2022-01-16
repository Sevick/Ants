using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats : MonoBehaviour, IStats{

    private Dictionary<string, long> statsMap = new Dictionary<string, long>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void incValue(string statName) {
        try {
            statsMap[statName]++;
        }
        catch (KeyNotFoundException) {
            statsMap.Add(statName, 0);
        }
    }

    public void setValue(string statName, long value) {
        try {
            statsMap[statName] = value;
        }
        catch (KeyNotFoundException) {
            statsMap.Add(statName, value);
        }
    }

    public void resetValue(string statName) {
        setValue(statName, 0);
    }

    public long getValue(string statName) {
        return statsMap[statName];
    }

    public List<string> getStatNames() {
        return new List<string>(statsMap.Keys);
    }
}
