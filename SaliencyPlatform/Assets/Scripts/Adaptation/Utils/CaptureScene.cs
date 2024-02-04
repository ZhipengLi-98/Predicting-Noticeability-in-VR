using System.IO;
using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class CameraPlacement
{
    [SerializeField] public Vector3 position;
    [SerializeField] public Vector3 eulerAngles;
}

public class CaptureScene : MonoBehaviour
{
    public GameObject m_camera;
    private Transform m_cameraTransform;
    private Camera m_cameraSettings;
    public CameraPlacement[] m_views;
    public string m_imgLabel;

    public void capture(string output)
    {
        RenderTexture captureRT = new RenderTexture(m_cameraSettings.pixelWidth, m_cameraSettings.pixelHeight, 32);
        m_cameraSettings.targetTexture = captureRT;
        RenderTexture.active = captureRT;

        m_cameraSettings.Render();
        int width = m_cameraSettings.targetTexture.width;
        int height = m_cameraSettings.targetTexture.height;
        Texture2D tex = new Texture2D(width, height);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();
        byte[] texBytes = tex.EncodeToPNG();
        File.WriteAllBytes("ExperimentResults/" + output + ".png", texBytes);

        RenderTexture.active = null;
        m_cameraSettings.targetTexture = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_cameraTransform = m_camera.transform;
        m_cameraSettings = m_camera.GetComponent<Camera>();
        int imgNum = m_views.Length;
        for (int i = 0; i < imgNum; i++)
        {
            m_cameraTransform.SetPositionAndRotation(m_views[i].position, Quaternion.Euler(m_views[i].eulerAngles));
            capture(m_imgLabel + "_" + m_views[i].position.ToString() + m_views[i].eulerAngles.ToString());
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
