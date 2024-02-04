using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR; 

public class EvaluationController : MonoBehaviour
{
    // Laser 
    public LineRenderer m_lr;
    private Vector3 m_lrDir;
    private Vector3[] m_lrPos = new Vector3[2];
    public Transform m_lrStart;
    public Transform m_lrEnd;
    public float m_lrNear;
    public float m_lrFar;

    // Raycasting 
    public float m_selectionRange;
    private int m_layerMaskAll, m_layerMaskUI, m_layerMaskElements;
    private GameObject m_laserPointer;

    // Controls 
    public InputDeviceCharacteristics m_deviceCharacteristics;
    private InputDevice m_device;
    private bool m_prevTrigger;
    private bool m_trigger;
    private bool m_prevGrip;
    private bool m_grip;
    private Vector2 m_joystick;
    public float m_joystickThreshold;

    // Selection
    private string m_colorField = "_OutlineColor";
    public Color m_deselectedColor;
    public Color m_selectedColor;
    private static GameObject m_selectedElement;
    private static float m_selectDistance;
    public float m_rangeAdjustmentSpeed;
    public bool m_adjustRange;

    // Supporting variables for transformations 
    // Rotation helper variables 
    private static Quaternion m_startObjRot;
    private static Quaternion m_startControllerRotInv;

    // Optimization
    public EvaluationExperimentManager m_experimentManager; 
    public OptimizationV2 m_optimizer;

    // Controls 
    private void initControllers()
    {
        List<InputDevice> inputDevices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(m_deviceCharacteristics, inputDevices);
        m_device = inputDevices[0];
        m_prevTrigger = false;
        m_prevGrip = false;
    }

    // Trigger
    private bool getTriggerValue()
    {
        return this.m_device.TryGetFeatureValue(CommonUsages.triggerButton, out m_trigger);
    }
    private bool getTriggerPress()
    {
        return (m_prevTrigger != m_trigger) && m_trigger;
    }
    private bool getTriggerRelease()
    {
        return (m_prevTrigger != m_trigger) && !m_trigger;
    }
    private bool getTriggerDown()
    {
        return m_trigger;
    }

    // Grip
    private bool getGripValue()
    {
        return this.m_device.TryGetFeatureValue(CommonUsages.gripButton, out m_grip);
    }
    private bool getGripPress()
    {
        return (m_prevGrip != m_grip) && m_grip;
    }
    private bool getGripRelease()
    {
        return (m_prevGrip != m_grip) && !m_grip;
    }
    private bool getGripDown()
    {
        return m_grip;
    }

    // Joystick
    private bool getJoystickValue()
    {
        return this.m_device.TryGetFeatureValue(CommonUsages.primary2DAxis, out m_joystick);
    }

    private bool getInputDeviceStates()
    {
        bool valid = getTriggerValue();
        valid &= getGripValue();
        valid &= getJoystickValue();
        return valid;
    }
    private void updatePreviousInputDeviceStates()
    {
        m_prevTrigger = m_trigger;
        m_prevGrip = m_grip;
    }

    // Laser Controls  
    private void initLR()
    {
        m_lr.startWidth = 0.01f;
        m_lr.endWidth = 0.005f;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.5f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        m_lr.colorGradient = gradient;
    }

    private void calcLR()
    {
        m_lrDir = (m_lrEnd.position - m_lrStart.position).normalized;
        m_lrPos[0] = m_lrStart.position + m_lrNear * m_lrDir;
        m_lrPos[1] = m_lrStart.position + m_lrFar * m_lrDir;
    }

    private void updateLR()
    {
        m_lr.SetPositions(m_lrPos);
    }

    // Raycasting 
    private void initRaycast()
    {
        m_layerMaskUI = 1 << 8;
        m_layerMaskElements = 1 << 9;
        m_layerMaskAll = m_layerMaskUI | m_layerMaskElements;
        m_laserPointer = transform.Find("laser_pointer").gameObject;
    }

    private Transform raycast(int layer)
    {
        RaycastHit hit;
        if (Physics.Raycast(m_lrPos[0], m_lrDir, out hit, m_selectionRange, layer))
        {
            // visual feedback
            m_laserPointer.SetActive(true);
            m_laserPointer.transform.position = hit.point;

            // return
            return hit.transform;
        }
        else
        {
            // visual feedback
            m_laserPointer.SetActive(false);

            // return 
            return null;
        }

    }

    // Updating the outline color
    private void updateOutlineColor(GameObject obj, Color c)
    {
        Renderer r = obj.GetComponent<Renderer>();
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        r.GetPropertyBlock(mpb);
        mpb.SetColor(this.m_colorField, c);
        r.SetPropertyBlock(mpb);
    }

    // Start is called before the first frame update
    void Start()
    {
        initControllers();
        initLR();
        initRaycast();
    }

    // Update is called once per frame
    void Update()
    {
        calcLR();
        updateLR();
        raycast(m_layerMaskAll);

        if (getInputDeviceStates())
        {
            if (getTriggerPress())
            {
                if (m_selectedElement != null)
                {
                    updateOutlineColor(m_selectedElement, m_deselectedColor);
                    m_selectedElement = null;
                }
                Transform selection = raycast(m_layerMaskUI);
                if (selection != null)
                {
                    switch (selection.name)
                    {
                        case "Next":
                            m_experimentManager.nextEnvironment();
                            break;
                        case "Back":
                            m_experimentManager.previousEnvironment();
                            break;
                        case "Reset":
                            m_experimentManager.resetEnvironment();
                            break;
                        case "ShowConnections":
                            m_optimizer.visualizeConnections();
                            break; 
                    }
                }
                selection = raycast(m_layerMaskElements);
                if (selection != null)
                {
                    m_optimizer.clearConnectionTransforms();
                    m_selectedElement = selection.gameObject;
                    updateOutlineColor(m_selectedElement, m_selectedColor);
                    m_selectDistance = Vector3.Distance(m_selectedElement.transform.position, transform.position);
                }
            }

            if (m_selectedElement != null)
            {
                // Translation
                if (getTriggerDown())
                {
                    m_selectedElement.transform.position = m_lrStart.position + (m_selectDistance * m_lrDir);
                }
                if (m_adjustRange && (Mathf.Abs(m_joystick.y) > m_joystickThreshold))
                {
                    m_selectDistance += m_rangeAdjustmentSpeed * m_joystick.y * Time.deltaTime;
                }

                // Rotation 
                if (getGripPress())
                {
                    m_startObjRot = m_selectedElement.transform.rotation;
                    m_startControllerRotInv = Quaternion.Inverse(transform.rotation);
                }
                if (getGripDown())
                {
                    m_selectedElement.transform.rotation = transform.rotation * m_startControllerRotInv * m_startObjRot;
                }
            }

            updatePreviousInputDeviceStates();
        }
    }
}
