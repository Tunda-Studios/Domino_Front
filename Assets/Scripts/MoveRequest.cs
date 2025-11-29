using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveRequest 
{
    public int[] tile; // null in JSON => pass
    public string end; // "left" or "right"
}
