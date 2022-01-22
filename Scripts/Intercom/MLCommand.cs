using UnityEngine;


public class MLCommand {

    public int[] dicreteActions = null;

    public MLCommand(int[] discreteActions) {
        this.dicreteActions = discreteActions;
    }

    public IIntercomState.IntercomCommands getBranch(int branchIdx) {
        if (dicreteActions == null)
            return 0;
        else {
            IIntercomState.IntercomCommands command = (IIntercomState.IntercomCommands) dicreteActions[branchIdx];
            return command;
        }
    }
}