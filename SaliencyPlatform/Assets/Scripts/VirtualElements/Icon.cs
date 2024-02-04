using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Icon : MonoBehaviour
{
    public Texture m_imageTexture;
    public MeshRenderer m_iconMeshRenderer;

    // Start is called before the first frame update
    void Start()
    {
        this.m_iconMeshRenderer.material.SetTexture("_MainTex", this.m_imageTexture);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
