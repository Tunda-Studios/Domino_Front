using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardRenderer : MonoBehaviour
{
    public RectTransform boardArea;
    public GameObject tilePrefab;
    public float spacing = 130f;

    public void RenderBoard(List<int[]> board)
    {
        foreach (Transform child in boardArea)
        {
            Destroy(child.gameObject);
        }

        float x = 0f;

        foreach (var d in board)
        {
            GameObject tile = Instantiate(tilePrefab, boardArea);
            RectTransform rt = tile.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(x, 0);
            x += spacing;
        }
    }

}
