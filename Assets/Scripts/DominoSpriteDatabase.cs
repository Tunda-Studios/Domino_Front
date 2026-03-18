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
            lookup[(t.right, t.left)] = t.sprite; 
        }
    }

    public Sprite GetTileSprite(int left, int right)
    {
        // Try direct match first
        foreach (var entry in tiles)
        {
            if (entry.left == left && entry.right == right)
                return entry.sprite;
        }

        // Try flipped match — domino sprites are symmetric
        foreach (var entry in tiles)
        {
            if (entry.left == right && entry.right == left)
                return entry.sprite;
        }

        Debug.LogWarning($"No sprite found for [{left}|{right}]");
        return null;
    }

}
