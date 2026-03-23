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

        Debug.Log($"[SETUP] Called with left={left}, right={right}");

        if (skin != null)
        {
            int fetchLeft = Mathf.Min(left, right);
            int fetchRight = Mathf.Max(left, right);

            Debug.Log($"[SETUP] Fetching sprite for [{fetchLeft}|{fetchRight}]");

            Sprite sprite = skin.GetTileSprite(fetchLeft, fetchRight);

            if (sprite == null)
                Debug.LogError($"[SETUP] Sprite is NULL for [{fetchLeft}|{fetchRight}]!");
            else
                Debug.Log($"[SETUP] Sprite found: {sprite.name}");

            tileImage.sprite = sprite;
            tileImage.transform.localScale = Vector3.one;

            Debug.Log($"[SETUP] tileImage.sprite set to: {(tileImage.sprite != null ? tileImage.sprite.name : "NULL")}");
            Debug.Log($"[SETUP] tileImage.transform.localScale = {tileImage.transform.localScale}");
        }
        else
        {
            Debug.LogError($"[SETUP] Skin is NULL for tile [{left}|{right}]!");
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

        if (table != null)
        {
            Domino tile = new Domino(_left, _right);
            table.HandleTileDragging(eventData.position, tile);
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        Domino tile = new Domino(_left, _right);

        DominoGameController.Instance.TryPlayTile(tile, eventData.position, this);

        if (table != null)
            table.HideDropHints();
    }

    public void ReturnToHand()
    {
        rect.anchoredPosition = startPosition;
    }


}
