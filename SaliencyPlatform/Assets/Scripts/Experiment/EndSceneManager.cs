using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EndSceneManager : MonoBehaviour
{
    public MeshRenderer[] m_leftCaptures, m_rightCaptures, m_frontCaptures; 

    // Start is called before the first frame update
    void Start()
    {
        int numScenes = 4;
        for (int sIdx = 0; sIdx < numScenes; sIdx++)
        {
            Texture2D left = GlobalExperimentState.getCaptureTextureLeft(sIdx);
            Texture2D right = GlobalExperimentState.getCaptureTextureRight(sIdx);
            Texture2D front = GlobalExperimentState.getCaptureTextureFront(sIdx);
            this.m_leftCaptures[sIdx].material.mainTexture = left;
            this.m_rightCaptures[sIdx].material.mainTexture = right;
            this.m_frontCaptures[sIdx].material.mainTexture = front;

            byte[] leftBytes = left.EncodeToPNG();
            File.WriteAllBytes(Application.persistentDataPath + "/scene" + sIdx.ToString() + "_left.png", leftBytes);
            byte[] rightBytes = right.EncodeToPNG();
            File.WriteAllBytes(Application.persistentDataPath + "/scene" + sIdx.ToString() + "_right.png", rightBytes);
            byte[] frontBytes = front.EncodeToPNG();
            File.WriteAllBytes(Application.persistentDataPath + "/scene" + sIdx.ToString() + "_front.png", frontBytes);

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
