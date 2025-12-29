using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DominoTileUI : MonoBehaviour
{
    [SerializeField] private Image tileImage;
    public TMP_Text debugLabel;        // optional: show "2|3" for debugging

    public int _left;
    public int _right;

    public void Setup(int left, int right, DominoSpriteDatabase skin)
    {
        _left = left;
        _right = right;

        if (skin != null)
        {
            tileImage.sprite = skin.GetTileSprite(left, right);
        }

        if (debugLabel != null)
            debugLabel.text = $"{left}|{right}";
    }
    private void Awake()
    {
        if (tileImage == null)
            tileImage = GetComponent<Image>();
    }
}
