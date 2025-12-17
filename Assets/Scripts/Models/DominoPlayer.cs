using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DominoPlayer 
{
    public string userId;
    public string displayName;

    public int wins;
    public int points;
    public int score;

    public bool isActive;
    public List<int[]> hand;

    public DominoSpriteDatabase selectedSkin;
}
