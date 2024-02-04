using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScatterPlot : MonoBehaviour
{
    public List<ScatterAxis> m_axes;
    private Vector3 m_prevPosition, m_prevScale, m_prevEuler;
    public Color m_data1Color, m_data2Color;
    public List<Vector3> m_data1, m_data2;
    private List<Vector3> m_data1Normalized, m_data2Normalized;
    public float m_scale = 10;
    public Transform m_dataPointContainer; 

    // Update axis 
    private void updateAxis()
    {
        foreach(ScatterAxis axis in this.m_axes)
        {
            axis.calcLR();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        float xMax = 0, yMax = 0, zMax = 0;
        foreach (Vector3 dataPoint in this.m_data1)
        {
            if (dataPoint.x > xMax) xMax = dataPoint.x;
            if (dataPoint.y > yMax) yMax = dataPoint.y;
            if (dataPoint.z > zMax) zMax = dataPoint.z;
        }
        foreach (Vector3 dataPoint in this.m_data2)
        {
            if (dataPoint.x > xMax) xMax = dataPoint.x;
            if (dataPoint.y > yMax) yMax = dataPoint.y;
            if (dataPoint.z > zMax) zMax = dataPoint.z;
        }
        this.m_data1Normalized = new List<Vector3>();
        this.m_data2Normalized = new List<Vector3>();
        foreach (Vector3 dataPoint in this.m_data1)
        {
            this.m_data1Normalized.Add(new Vector3(
                dataPoint.x / xMax,
                dataPoint.y / yMax,
                dataPoint.z / zMax));
        }
        foreach (Vector3 dataPoint in this.m_data2)
        {
            this.m_data2Normalized.Add(new Vector3(
                dataPoint.x / xMax,
                dataPoint.y / yMax,
                dataPoint.z / zMax));
        }
        foreach (Vector3 dataPoint in this.m_data1Normalized)
        {
            GameObject pointObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pointObj.name = "datapoint";
            pointObj.transform.SetParent(this.m_dataPointContainer);
            pointObj.transform.localScale = 0.5f * Vector3.one;
            pointObj.transform.localPosition = this.m_scale * dataPoint;
            pointObj.GetComponent<MeshRenderer>().material.SetColor("_Color", this.m_data1Color);
        }
        foreach (Vector3 dataPoint in this.m_data2Normalized)
        {
            GameObject pointObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pointObj.name = "datapoint";
            pointObj.transform.SetParent(this.m_dataPointContainer);
            pointObj.transform.localScale = 0.5f * Vector3.one;
            pointObj.transform.localPosition = this.m_scale * dataPoint;
            pointObj.GetComponent<MeshRenderer>().material.SetColor("_Color", this.m_data2Color);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (this.m_prevPosition != this.transform.position || this.m_prevScale != this.transform.lossyScale || this.m_prevEuler != this.transform.eulerAngles)
        {
            this.updateAxis();
            this.m_prevPosition = this.transform.position;
            this.m_prevScale = this.transform.lossyScale;
            this.m_prevEuler = this.transform.eulerAngles;
        }
    }
}
