using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCosmeticsManager : MonoBehaviour
{
    public static PlayerCosmeticsManager Instance;

    public DominoSpriteDatabase activeDominoSet;

    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    public void SetTileSkin(DominoSpriteDatabase newSet)
    {
        activeDominoSet = newSet;
    }

    public DominoSpriteDatabase GetTileSkin()
    {
        return activeDominoSet;
    }
}
