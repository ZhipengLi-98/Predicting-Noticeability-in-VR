using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeSetupHelper
{
    public static List<Transform> m_points = new List<Transform>();
    public static Transform m_currPoint;
    public static int m_numConfirmedPoints;
    public static GameObject m_volume;
    public static Transform m_volumeBounds; 

    private static void clear()
    {
        foreach (Transform point in m_points)
        {
            point.SetParent(null);
            GameObject.Destroy(point.gameObject);
        }
        m_points.Clear();
        m_volume = null;
        m_volumeBounds = null;
        m_numConfirmedPoints = 0;
    }

    public static void init(Transform currPoint)
    {
        clear();
        m_currPoint = currPoint;
        m_points.Add(m_currPoint);
    }

    public static void addControlPoint(Transform point)
    {
        m_currPoint = point;
        m_points.Add(m_currPoint);
    }

    public static void addVolume(GameObject volume)
    {
        m_volume = volume;
        m_volumeBounds = m_volume.transform.Find("bounds");
    }

    public static void updateVolume()
    {
        Vector3 pointA = m_points[0].position;
        Vector3 pointB = m_points[1].position;
        m_volume.transform.position = 0.5f * (pointA + pointB);
        m_volumeBounds.localScale = Vector3.Max(pointA, pointB) - Vector3.Min(pointA, pointB);
    }

    public static void confirmVolume()
    {
        clear();
    }
}
