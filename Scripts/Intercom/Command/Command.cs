using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Actuators;
using Unity.MLAgents;

public abstract class Command {
    public string id;
    protected int actionCommand;

    protected int currentParameter = 0;
    protected int parametersCount;
    public List<object> parameters;

    public Command(int actionCommand, int parametersCount) {
        this.id = Guid.NewGuid().ToString();
        this.actionCommand = actionCommand;
        this.parametersCount = parametersCount;        
    }

    public bool needMoreParameters() {
        return currentParameter < parametersCount;
    }

    abstract public void WriteDiscreteActionMask(IDiscreteActionMask actionMask);

}