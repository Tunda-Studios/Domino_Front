using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DominoMove : MonoBehaviour
{
    public string userId;
    public int[] tile;      // e.g. [6,4]
    public string end;      // "left" or "right"
    public string createdAt;
}
