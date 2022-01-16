using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStats{
    void incValue(string statName);
    void setValue(string statName, long value);
    void resetValue(string statName);
    long getValue(string statName);
    List<string> getStatNames();
}
