using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class DominoClick : MonoBehaviour, IPointerClickHandler
{
    public Domino tile;
    public void OnPointerClick(PointerEventData eventData)
    {

        //final code
      //  DominoGameController.Instance.TryPlayTile(tile);
    }
}
