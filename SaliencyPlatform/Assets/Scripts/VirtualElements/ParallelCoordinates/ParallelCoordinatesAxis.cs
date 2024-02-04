using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallelCoordinatesAxis : MonoBehaviour
{
    // Line Renderers
    private LineRenderer m_lr;
    private Vector3[] m_lrPos;
    public GameObject m_z, m_y, m_yz;

    public void calcLR()
    {
        this.m_lrPos[0] = this.transform.position;
        this.m_lrPos[1] = this.m_y.transform.position;
        this.m_lrPos[2] = this.m_yz.transform.position;
        this.m_lrPos[3] = this.m_z.transform.position;
        this.m_lrPos[4] = this.transform.position;
        this.m_lr.SetPositions(this.m_lrPos);
    }

    // Start is called before the first frame update
    void Start()
    {
        this.m_lr = this.GetComponent<LineRenderer>();
        this.m_lr.startWidth = 0.02f;
        this.m_lr.endWidth = 0.02f;
        this.m_lr.positionCount = 5;
        this.m_lrPos = new Vector3[5];
        this.calcLR();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
