using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualizeEdges : MonoBehaviour
{
    public GameObject m_edgePrefab;
    public List<GameObject> m_nodes; 
    private List<VisualizeEdge> m_edges; 

    private VisualizeEdge newEdge(GameObject start, GameObject end)
    {
        GameObject edge = GameObject.Instantiate(this.m_edgePrefab, this.transform);
        edge.name = "Edge";
        VisualizeEdge edgeInfo = edge.GetComponent<VisualizeEdge>();
        edgeInfo.m_lrStart = start;
        edgeInfo.m_lrEnd = end;
        return edgeInfo;
    }

    private void buildEdges()
    {
        this.m_edges.Add(this.newEdge(this.m_nodes[12], this.m_nodes[0]));
        this.m_edges.Add(this.newEdge(this.m_nodes[11], this.m_nodes[4]));
        this.m_edges.Add(this.newEdge(this.m_nodes[10], this.m_nodes[7]));
        this.m_edges.Add(this.newEdge(this.m_nodes[9], this.m_nodes[8]));
        this.m_edges.Add(this.newEdge(this.m_nodes[8], this.m_nodes[4]));
        this.m_edges.Add(this.newEdge(this.m_nodes[7], this.m_nodes[5]));
        this.m_edges.Add(this.newEdge(this.m_nodes[7], this.m_nodes[4]));
        this.m_edges.Add(this.newEdge(this.m_nodes[6], this.m_nodes[5]));
        this.m_edges.Add(this.newEdge(this.m_nodes[5], this.m_nodes[4]));
        this.m_edges.Add(this.newEdge(this.m_nodes[4], this.m_nodes[0]));
        this.m_edges.Add(this.newEdge(this.m_nodes[4], this.m_nodes[1]));
        this.m_edges.Add(this.newEdge(this.m_nodes[3], this.m_nodes[0]));
        this.m_edges.Add(this.newEdge(this.m_nodes[3], this.m_nodes[1]));
        this.m_edges.Add(this.newEdge(this.m_nodes[2], this.m_nodes[1]));
        this.m_edges.Add(this.newEdge(this.m_nodes[1], this.m_nodes[0]));
    }

    public void visualizeEdges()
    {
        foreach(VisualizeEdge edge in this.m_edges)
        {
            edge.calcLR();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach(Transform edge in this.transform)
        {
            GameObject.Destroy(edge.gameObject);
        }
        this.m_edges = new List<VisualizeEdge>();
        this.buildEdges();
        this.visualizeEdges();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
