using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DominoSpriteDatabase", menuName = "Dominoes/Sprite Database")]
public class DominoSpriteDatabase : ScriptableObject
{
    [System.Serializable]
    public struct DominoEntry
    {
        public int left;
        public int right;
        public Sprite sprite;
    }

    public List<DominoEntry> tiles = new List<DominoEntry>();

    private Dictionary<(int, int), Sprite> lookup;

    // Build dictionary for fast lookup
    private void OnEnable()
    {
        lookup = new Dictionary<(int, int), Sprite>();

        foreach (var t in tiles)
        {
            lookup[(t.left, t.right)] = t.sprite;
            lookup[(t.right, t.left)] = t.sprite; // dominoes are reversible
        }
    }

    public Sprite GetTileSprite(int left, int right)
    {
        if (lookup.TryGetValue((left, right), out var sprite))
            return sprite;

        Debug.LogWarning($"Sprite not found for domino {left}|{right}");
        return null;
    }
    
}
