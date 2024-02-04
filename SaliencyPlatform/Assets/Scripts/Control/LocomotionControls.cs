using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using TMPro;

public class LocomotionControls : MonoBehaviour
{
    // Character 
    public Transform m_character;
    public Transform m_camera; 

    // Device 
    public InputDeviceCharacteristics m_deviceChar;
    private InputDevice m_device;

    // Joystick Output 
    private Vector2 m_joystick;
    private Vector3 m_move;

    // Locomotion
    public float m_moveSpeed;
    public float m_moveThreshold;

    // Start is called before the first frame update
    void Start()
    {
        // Get Controller 
        List<InputDevice> inputDevices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(this.m_deviceChar, inputDevices);
        this.m_device = inputDevices[0];
        this.m_move = Vector3.zero; 
    }

    // Update is called once per frame
    void Update()
    {
        if (this.m_device.TryGetFeatureValue(CommonUsages.primary2DAxis, out this.m_joystick)) {

            if (this.m_joystick.magnitude > this.m_moveThreshold)
            {
                Vector3 forward = this.m_camera.forward;
                forward.y = 0.0f;
                forward.Normalize();
                Vector3 right = this.m_camera.right;
                right.y = 0.0f;
                right.Normalize();
                this.m_move = (this.m_joystick.y * forward) + (this.m_joystick.x * right);
                this.m_move *= this.m_moveSpeed * Time.deltaTime;
                this.m_character.position += this.m_move;
            }
        }
    }
}
