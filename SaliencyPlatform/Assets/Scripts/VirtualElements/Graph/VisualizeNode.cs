using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualizeNode : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Camera.main != null)
        {
            Vector3 dir = transform.position - Camera.main.transform.position;
            transform.rotation = Quaternion.LookRotation(dir);
        }
    }
}
