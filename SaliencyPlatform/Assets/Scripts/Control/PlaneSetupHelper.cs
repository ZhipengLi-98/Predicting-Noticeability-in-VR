using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneSetupHelper {
    public static List<Transform> m_points = new List<Transform>();
    public static Transform m_currPoint;
    public static int m_numConfirmedPoints;
    public static GameObject m_plane;
    public static Transform m_planeBounds;

    private static void clear()
    {
        foreach (Transform point in m_points)
        {
            point.SetParent(null);
            GameObject.Destroy(point.gameObject);
        }
        m_points.Clear();
        m_plane = null;
        m_planeBounds = null;
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

    public static void addPlane(GameObject plane)
    {
        m_plane = plane;
        m_planeBounds = m_plane.transform.Find("bounds");
    }

    public static void updatePlane()
    {
        Vector3 pointA = m_points[0].position;
        Vector3 pointB = m_points[1].position;
        Vector3 ab = pointB - pointA;
        if (m_points.Count < 3)
        {
            m_plane.transform.position = 0.5f * (pointA + pointB);
            m_plane.transform.rotation = Quaternion.FromToRotation(Vector3.right, ab.normalized);
            m_planeBounds.localScale = new Vector3(ab.magnitude, 0.01f, 0.01f);
        }
        else
        {
            Vector3 pointC = m_points[2].position;
            Vector3 ac = pointC - pointA;
            Vector3 normal = Vector3.Cross(ab.normalized, ac.normalized).normalized;
            Vector3 right = Vector3.Cross(normal, Vector3.up).normalized;
            Vector3 up = Vector3.Cross(right, normal).normalized;
            float upMax = Mathf.Max(0, Vector3.Dot(ab, up), Vector3.Dot(ac, up));
            float upMin = Mathf.Min(0, Vector3.Dot(ab, up), Vector3.Dot(ac, up));
            float rightMax = Mathf.Max(0, Vector3.Dot(ab, right), Vector3.Dot(ac, right));
            float rightMin = Mathf.Min(0, Vector3.Dot(ab, right), Vector3.Dot(ac, right));
            m_plane.transform.position = pointA + (0.5f * (upMax + upMin) * up) + (0.5f * (rightMax + rightMin) * right);
            m_plane.transform.rotation = Quaternion.FromToRotation(Vector3.forward, normal);
            m_planeBounds.localScale = new Vector3(rightMax - rightMin, upMax - upMin, 0.01f);
        }
    }

    public static void confirmPlane()
    {
        clear();
    }
}
