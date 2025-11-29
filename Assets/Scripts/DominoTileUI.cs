using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DominoTileUI : MonoBehaviour
{
    public SpriteRenderer faceRenderer;
    public TMP_Text debugLabel;        // optional: show "2|3" for debugging

    public int _left;
    public int _right;

    public void Setup(int left, int right, Sprite sprite)
    {
        _left = left;
        _right = right;

        if (faceRenderer != null && sprite != null)
            faceRenderer.sprite = sprite;

        if (debugLabel != null)
            debugLabel.text = $"{left}|{right}";
    }
}
