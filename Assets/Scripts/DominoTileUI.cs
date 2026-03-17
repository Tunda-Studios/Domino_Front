using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class DominoTileUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image tileImage;
    public TMP_Text debugLabel;        // optional: show "2|3" for debugging
    [SerializeField] private Image highlightBorder;

    public int _left;
    public int _right;

    private RectTransform rect;
    private Canvas canvas;

    private Vector2 startPosition;

    private bool selected = false;
    public DominoTableView table;

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
        rect = GetComponent<RectTransform>();

        if (rect == null)
            Debug.LogError("DominoTileUI missing RectTransform!");

        canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
            Debug.LogError("DominoTileUI could not find Canvas!");

        if (tileImage == null)
            tileImage = GetComponent<Image>();
    }

    public void setSelected(bool value)
    {
        selected = value;

        if (highlightBorder != null)
        {
            highlightBorder.enabled = value;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = rect.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Domino tile = new Domino
        {
            left = _left,
            right = _right
        };

        DominoGameController.Instance.TryPlayTile(tile, rect.position, this);
    }

    public void ReturnToHand()
    {
        rect.anchoredPosition = startPosition;
    }


}
