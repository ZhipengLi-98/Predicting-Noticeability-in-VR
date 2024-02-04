using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualObjectManager : MonoBehaviour
{
    public List<GameObject> m_virtualObjects;
    // Define init grid 
    public int m_numRows, m_numColumns;
    public float m_rowSpacing, m_columnSpacing; 

    // Start is called before the first frame update
    void Start()
    {
        Vector3[] initialPos = new Vector3[this.m_numRows * this.m_numColumns];
        List<int> initialPosIdx = new List<int>();
        int pIdx = 0; 
        // Randomize placement of virtual objects 
        for (int y = 0; y < m_numRows; y++)
        {
            for (int x = 0; x < m_numColumns; x++)
            {
                initialPos[pIdx] = new Vector3(x * this.m_columnSpacing, y * this.m_rowSpacing, 0);
                initialPosIdx.Add(pIdx++);
            }
        }
        int numVirtualObjects = this.m_virtualObjects.Count;
        //int updateIdx;
        Vector3 updatePos;
        int updateIdx, i; 
        string objName;
        for (int oIdx = 0; oIdx < numVirtualObjects; oIdx++)
        {
            updateIdx = Random.Range(0, initialPosIdx.Count);
            i = initialPosIdx[updateIdx];
            updatePos = initialPos[i];
            objName = this.m_virtualObjects[oIdx].name;
            if (objName == "ParallelCoordinates") // Offset for certain objects
            {
                updatePos.x -= 0.25f;
                updatePos.y -= 0.125f; 
            }
            if (objName == "HMDModel")
            {
                updatePos.y -= 0.125f;
            }
            if (objName == "ShoppingApplication")
            {
                updatePos.y -= 0.125f;
            }
            if (objName == "Time")
            {
                updatePos.x -= 0.3f;
            }

            this.m_virtualObjects[oIdx].transform.localPosition = updatePos;
            initialPosIdx.RemoveAt(updateIdx);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
