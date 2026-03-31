using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHand
{
    public List<Domino> tiles = new List<Domino>();
    // Start is called before the first frame update

    public void AddTile(Domino tile)
    {
        tiles.Add(tile);
    }

    public void RemoveTile(Domino tile)
    {
        tiles.Remove(tile);
    }

    public int Count => tiles.Count;
}
