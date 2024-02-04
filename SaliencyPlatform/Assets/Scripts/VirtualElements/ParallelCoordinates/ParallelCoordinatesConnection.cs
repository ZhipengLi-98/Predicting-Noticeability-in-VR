using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallelCoordinatesConnection : MonoBehaviour
{
    // Line Renderer 
    private LineRenderer m_lr;
    private Vector3[] m_lrPos;
    public GameObject[] m_points; 
    private bool m_instantiated = false; 

    // Calculate LineRenderer positions 
    public void calcLR()
    {
        if (m_instantiated) return; 

        this.m_lr = this.GetComponent<LineRenderer>();
        this.m_lr.startWidth = 0.01f;
        this.m_lr.endWidth = 0.01f;
        this.m_lrPos = new Vector3[this.m_points.Length];
        this.m_lr.positionCount = this.m_points.Length;
        this.m_lrPos[0] = m_points[0].transform.position;
        this.m_lrPos[1] = m_points[1].transform.position;
        this.m_lrPos[2] = m_points[2].transform.position;
        this.m_lr.SetPositions(this.m_lrPos);
        this.transform.gameObject.layer = LayerMask.NameToLayer("Default");
    }

    // Start is called before the first frame update
    void Start()
    {
        calcLR();
    }

    // Update is called once per frame
    void Update()
    {
    }
}
