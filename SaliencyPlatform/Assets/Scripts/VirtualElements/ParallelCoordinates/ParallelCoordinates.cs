using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallelCoordinates : MonoBehaviour
{
    public List<ParallelCoordinatesAxis> m_axes;
    public List<Transform> m_axesDataPointContainer; 
    private Vector3 m_prevPosition, m_prevScale, m_prevEuler;
    public List<Vector3> m_data1, m_data2;
    private List<Vector3> m_data1Normalized, m_data2Normalized;
    public float m_scale = 10;
    public GameObject m_dataConnectionPrefab;
    private List<ParallelCoordinatesConnection> m_dataConnections;
    public Transform m_connectionContainer;
    public Color m_pointColour;

    // Datapoint Objs 
    private List<GameObject[]> m_dataObjs; 

    // Update axis 
    private void updateAxis()
    {
        foreach (ParallelCoordinatesAxis axis in this.m_axes)
        {
            axis.calcLR();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach(Transform dataPointContainer in this.m_axesDataPointContainer)
        {
            foreach(Transform dataPoint in dataPointContainer)
            {
                GameObject.Destroy(dataPoint.gameObject);
            }
        }
        foreach(Transform connection in this.m_connectionContainer)
        {
            GameObject.Destroy(connection.gameObject);
        }

        this.m_data1Normalized = new List<Vector3>();
        foreach (Vector3 dataPoint in this.m_data1)
        {
            this.m_data1Normalized.Add(dataPoint / 8.0f);
        }
        this.m_data2Normalized = new List<Vector3>();
        foreach (Vector3 dataPoint in this.m_data2)
        {
            this.m_data2Normalized.Add(dataPoint / 8.0f);
        }
        int numDataPoints = this.m_data1Normalized.Count;
        int numFeatures = this.m_axesDataPointContainer.Count;

        this.m_dataConnections = new List<ParallelCoordinatesConnection>();
        for (int dIdx = 0; dIdx < numDataPoints; dIdx++)
        {
            GameObject[] dataObjs = new GameObject[3];
            for (int fIdx = 0; fIdx < numFeatures; fIdx++)
            {
                dataObjs[fIdx] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                dataObjs[fIdx].GetComponent<MeshRenderer>().material.SetColor("_Color", this.m_pointColour);
                dataObjs[fIdx].name = "datapoint";
                dataObjs[fIdx].transform.SetParent(this.m_axesDataPointContainer[fIdx]);
                dataObjs[fIdx].transform.localScale = 0.5f * Vector3.one;
                dataObjs[fIdx].transform.localPosition = this.m_scale * new Vector3(0, this.m_data1Normalized[dIdx][fIdx], this.m_data2Normalized[dIdx][fIdx]);
            }
            GameObject connection = GameObject.Instantiate(this.m_dataConnectionPrefab, this.m_connectionContainer);
            connection.name = "connection";
            ParallelCoordinatesConnection connectionInfo = connection.GetComponent<ParallelCoordinatesConnection>();
            connectionInfo.m_points = new GameObject[3];
            for (int fIdx = 0; fIdx < numFeatures; fIdx++)
            {
                connectionInfo.m_points[fIdx] = dataObjs[fIdx];
            }
            this.m_dataConnections.Add(connectionInfo);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (this.m_prevPosition != this.transform.position || this.m_prevScale != this.transform.lossyScale || this.m_prevEuler != this.transform.eulerAngles)
        {
            this.updateAxis();
            foreach(ParallelCoordinatesConnection connection in this.m_dataConnections)
            {
                connection.calcLR();
            }
            this.m_prevPosition = this.transform.position;
            this.m_prevScale = this.transform.lossyScale;
            this.m_prevEuler = this.transform.eulerAngles;
        }
    }
}
