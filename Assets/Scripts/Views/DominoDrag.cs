using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DominoDrag : MonoBehaviour, IBeginDragHandler, IDragHandler,IEndDragHandler
{
    public Domino tile;

    private RectTransform rect;
    private Canvas canvas;

    private Vector2 startPosition;

    public void OnBeginDrag(PointerEventData eventData)
    {
        //remember starting position 
        startPosition = rect.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //move tile with mouse or touch
        rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //if not dropped on a valid zone return to hand
      
    }

    public void ReturnToHand()
    {
        rect.anchoredPosition = startPosition;
    }


    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponent<Canvas>();
    }

    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
