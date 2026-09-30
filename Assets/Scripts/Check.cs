using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Check : MonoBehaviour
{
    // Start is called before the first frame update

    public RenderTexture rt;

    void Start()
    {
        Debug.Log($"aa={rt.antiAliasing} fmt={rt.graphicsFormat} depth={rt.depth} srgb={rt.sRGB}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
