using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandRenderer : MonoBehaviour
{
    public RectTransform anchor;
    public GameObject tilePrefab;
    public float tileSpacing = 120f;

    public void RenderHand(List<int[]> hand)
    {
        // Clear old tiles
        foreach (Transform child in anchor)
        {
            Destroy(child.gameObject);
        }

        // Center the hand a bit by offsetting start
        float totalWidth = (hand.Count - 1) * tileSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < hand.Count; i++)
        {
            GameObject tile = Instantiate(tilePrefab, anchor);
            RectTransform rt = tile.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(startX + i * tileSpacing, 0);
        }
    }
}
