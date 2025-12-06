using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DominoSpriteDatabaseLoader : MonoBehaviour
{
    public DominoSpriteDatabase database;
    public static DominoSpriteDatabase Instance;
    // Start is called before the first frame update

    private void Awake()
    {
        Instance = database;
    }
}
