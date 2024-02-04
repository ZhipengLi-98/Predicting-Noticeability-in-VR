using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualizeGraph : MonoBehaviour
{
    public VisualizeEdges m_edges;
    private Vector3 m_prevPosition;
    private Vector3 m_prevScale;
    private Vector3 m_prevEuler; 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (this.m_prevPosition != this.transform.position || this.m_prevScale != this.transform.lossyScale || this.m_prevEuler != this.transform.eulerAngles)
        {
            this.m_edges.visualizeEdges();
            this.m_prevPosition = this.transform.position;
            this.m_prevScale = this.transform.lossyScale;
            this.m_prevEuler = this.transform.eulerAngles; 
        }
    }
}
