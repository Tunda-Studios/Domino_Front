using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedViewCamera : MonoBehaviour
{
    // Start is called before the first frame update
    public float referenceWidth = 1920f;
    public float referenceHeight = 1080f;

    private Camera cam;
    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        UpdateSize();
    }


    // Update is called once per frame
    void Update()
    {
        UpdateSize();
    }

    void UpdateSize()
    { 
        float targetAspect = referenceWidth / referenceHeight;
        float windowAspect = (float)Screen.width / Screen.height;

        float scale = targetAspect / windowAspect;

        cam.orthographicSize = referenceHeight / 200f * Mathf.Max(1f, scale);
    }
}
