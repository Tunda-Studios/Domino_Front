using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class DominoTileUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image tileImage;
    public TMP_Text debugLabel;        // optional: show "2|3" for debugging
    [SerializeField] private Image highlightBorder;

    public int _left;
    public int _right;

    private bool selected = false;
    public DominoTableView table;

    public void Setup(int left, int right, DominoSpriteDatabase skin)
    {
        _left = left;
        _right = right;

        if (skin != null)
        {
            tileImage.sprite = skin.GetTileSprite(left, right);
            Debug.Log("reached " + tileImage.sprite);
        }
        else
        {

            Debug.Log("Reached");
        }

        if (debugLabel != null)
            debugLabel.text = $"{left}|{right}";
    }
    private void Awake()
    {
        Debug.Log("reached " );
        if (tileImage == null)
            tileImage = GetComponent<Image>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {

        /* //toggle selection
         selected = !selected;

         if (selected)
         {
             DominoGameController.Instance.SetSelectedTile(this);
         }
         Domino tile = new Domino(_left, _right);

         DominoGameController.Instance.TryPlayTile(tile);
        */

        //localhost test
        if (table != null)
            table.OnTileClicked(_left, _right);
    }

    public void setSelected(bool value)
    {
        selected = value;

        if (highlightBorder != null)
        {
            highlightBorder.enabled = value;
        }
    }

 
}
