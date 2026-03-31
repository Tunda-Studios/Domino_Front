using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathState
{
    public Direction currentDirection;
    public Direction lastHorizontalDirection;
    public int horizontalStepsTaken;
    public int verticalStepsTaken;
    public Direction previousDirection;

    public bool justSwitchedDirection;
    public int stepsSinceSwitch;
}
