using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCosmeticsManager : MonoBehaviour
{
    public static PlayerCosmeticsManager Instance;

    private Dictionary<string, DominoSpriteDatabase> playerSkins =
        new Dictionary<string, DominoSpriteDatabase>();

    private void Awake()
    {
        Instance = this;
    }

    public void SetPlayerSkin(string playerId, DominoSpriteDatabase skin)
    {
        playerSkins[playerId] = skin;
    }

    public DominoSpriteDatabase GetPlayerSkin(string playerId)
    {
        if (playerSkins.TryGetValue(playerId, out var skin))
            return skin;

        Debug.LogWarning($"No skin found for player {playerId}");
        return null;
    }
}

