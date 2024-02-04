using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class VisualizeLayout : MonoBehaviour
{
    public int m_participant = -1;
    public string m_layoutFilePath;
    public Transform m_virtualElements;
    public Color m_color;
    private bool m_loaded = false;

    private void updateOutlineColor(GameObject obj, Color c)
    {
        Renderer r = obj.GetComponent<Renderer>();
        if (r != null)
        {
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            r.GetPropertyBlock(mpb);
            mpb.SetColor("_OutlineColor", c);
            r.SetPropertyBlock(mpb);
        }
    }

    public void loadLayout(string[] restrictElements)
    {
        loadLayout("ExperimentResults/Participant" + m_participant.ToString("D2") + "/" + this.m_layoutFilePath + ".txt", restrictElements);

    }

    public void loadLayout()
    {
        Debug.Assert((m_participant > 0) && (m_layoutFilePath.Length > 0), "Ensure participant and file path arguments are defined correctly.");
        this.loadLayout("ExperimentResults/Participant" + m_participant.ToString("D2") + "/" + this.m_layoutFilePath + ".txt");
    }

    private void loadLayout(string path)
    {
        if (m_loaded) return; 
        string str = File.ReadAllText(path);
        string[] lines = str.Split(
            new[] { "\r\n", "\r", "\n" },
            StringSplitOptions.RemoveEmptyEntries
        );
        bool res = int.TryParse(lines[0], out int numElements);
        Debug.Assert(res);

        int lnum = 1;
        for (int eIdx = 0; eIdx < numElements; eIdx++)
        {
            string[] elementInfo = lines[lnum++].Split(' ');
            Debug.Assert(elementInfo.Length == 11);

            // Retrieve element 
            string elementName = elementInfo[1];
            Transform element = m_virtualElements.Find(elementName);
            Debug.Assert(element);
            GameObject elementObj = GameObject.Instantiate(element.gameObject, this.transform);
            elementObj.name = elementName;
            elementObj.SetActive(true);

            this.updateOutlineColor(elementObj, this.m_color);

            // Position 
            res = float.TryParse(elementInfo[2], out float posX);
            Debug.Assert(res);
            res = float.TryParse(elementInfo[3], out float posY);
            Debug.Assert(res);
            res = float.TryParse(elementInfo[4], out float posZ);
            Debug.Assert(res);
            // Rotation
            res = float.TryParse(elementInfo[5], out float rotX);
            Debug.Assert(res);
            res = float.TryParse(elementInfo[6], out float rotY);
            Debug.Assert(res);
            res = float.TryParse(elementInfo[7], out float rotZ);
            Debug.Assert(res);
            // Scale
            res = float.TryParse(elementInfo[8], out float scaleX);
            Debug.Assert(res);
            res = float.TryParse(elementInfo[9], out float scaleY);
            Debug.Assert(res);
            res = float.TryParse(elementInfo[10], out float scaleZ);
            Debug.Assert(res);

            elementObj.transform.position = new Vector3(posX, posY, posZ);
            elementObj.transform.eulerAngles = new Vector3(rotX, rotY, rotZ);
            elementObj.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
        }
        m_loaded = true;    
    }


    private void loadLayout(string path, string[] restrictElements)
    {
        if (m_loaded) return; 
        string str = File.ReadAllText(path);
        string[] lines = str.Split(
            new[] { "\r\n", "\r", "\n" },
            StringSplitOptions.RemoveEmptyEntries
        );
        bool res = int.TryParse(lines[0], out int numElements);
        Debug.Assert(res);

        int lnum = 1;
        for (int eIdx = 0; eIdx < numElements; eIdx++)
        {
            string[] elementInfo = lines[lnum++].Split(' ');
            Debug.Assert(elementInfo.Length == 11);

            // Retrieve element 
            string elementName = elementInfo[1];
            if (Array.IndexOf(restrictElements, elementName) < 0) continue; 
            Transform element = m_virtualElements.Find(elementName);
            Debug.Assert(element);
            GameObject elementObj = GameObject.Instantiate(element.gameObject, this.transform);
            elementObj.name = elementName;
            elementObj.SetActive(true);

            this.updateOutlineColor(elementObj, this.m_color);

            // Position 
            res = float.TryParse(elementInfo[2], out float posX);
            Debug.Assert(res);
            res = float.TryParse(elementInfo[3], out float posY);
            Debug.Assert(res);
            res = float.TryParse(elementInfo[4], out float posZ);
            Debug.Assert(res);
            // Rotation
            res = float.TryParse(elementInfo[5], out float rotX);
            Debug.Assert(res);
            res = float.TryParse(elementInfo[6], out float rotY);
            Debug.Assert(res);
            res = float.TryParse(elementInfo[7], out float rotZ);
            Debug.Assert(res);
            // Scale
            res = float.TryParse(elementInfo[8], out float scaleX);
            Debug.Assert(res);
            res = float.TryParse(elementInfo[9], out float scaleY);
            Debug.Assert(res);
            res = float.TryParse(elementInfo[10], out float scaleZ);
            Debug.Assert(res);

            elementObj.transform.position = new Vector3(posX, posY, posZ);
            elementObj.transform.eulerAngles = new Vector3(rotX, rotY, rotZ);
            elementObj.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
        }
        m_loaded = true;
    }
}
