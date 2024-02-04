using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualizeEdge : MonoBehaviour
{
    // Line Renderer 
    private LineRenderer m_lr;
    private Vector3[] m_lrPos;
    public GameObject m_lrStart;
    public GameObject m_lrEnd;
    private bool m_lrRendered = false; 


    // Calculate LineRenderer positions 
    public void calcLR()
    {
        if (m_lrRendered) return;
        this.m_lr = this.GetComponent<LineRenderer>();
        this.m_lr.startWidth = 0.01f;
        this.m_lr.endWidth = 0.01f;
        this.m_lrPos = new Vector3[2];
        if (m_lrStart != null && m_lrEnd != null)
        {
            this.m_lrPos[0] = this.m_lrStart.transform.position;
            this.m_lrPos[1] = this.m_lrEnd.transform.position;
            this.m_lr.SetPositions(this.m_lrPos);
        }
        m_lrRendered = true; 
    }

    // Start is called before the first frame update
    void Start()
    {
        this.calcLR();
    }

    // Update is called once per frame
    void Update()
    {   
    }
}
