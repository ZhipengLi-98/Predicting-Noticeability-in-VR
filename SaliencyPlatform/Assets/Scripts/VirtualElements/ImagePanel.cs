using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImagePanel : MonoBehaviour
{
    public Texture m_imageTexture;
    public MeshRenderer m_imagePanelMeshRenderer;

    // Start is called before the first frame update
    void Start()
    {
        this.m_imagePanelMeshRenderer.material.SetTexture("_MainTex", this.m_imageTexture);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
