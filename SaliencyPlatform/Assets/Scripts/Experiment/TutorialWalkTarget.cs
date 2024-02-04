using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialWalkTarget : MonoBehaviour
{
    public float m_rotateSpeed; 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.localRotation = Quaternion.Euler(0.0f, this.m_rotateSpeed * Time.deltaTime, 0.0f) * this.transform.localRotation;
    }
}
