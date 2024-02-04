using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScatterAxis : MonoBehaviour
{
    // Line Renderer 
    private LineRenderer m_lr;
    private Vector3[] m_lrPos;
    public GameObject m_lrStart;

    // Calculate LineRenderer positions 
    public void calcLR()
    {
        this.m_lrPos[0] = this.m_lrStart.transform.position;
        this.m_lrPos[1] = this.transform.position;
        this.m_lr.SetPositions(this.m_lrPos);
    }

    // Start is called before the first frame update
    void Start()
    {
        this.m_lr = this.GetComponent<LineRenderer>();
        this.m_lr.startWidth = 0.01f;
        this.m_lr.endWidth = 0.01f;
        this.m_lrPos = new Vector3[2];
        this.calcLR();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
