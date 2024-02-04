using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class ManualAdjustmentControls : MonoBehaviour
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
    private bool m_primary;
    private bool m_prevPrimary; 
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
    // Scale helper variables
    private static Vector3 m_startObjScale;
    private static float m_startControllerScale;
    public bool m_performRotation;

    // Other controller reference 
    public GameObject m_other;
    private ManualAdjustmentControls m_otherManualAdjustmentControls;
    private ContainerSetupControls m_thisOptimizationControls, m_otherOptimizationControls;

    // Menus
    public GameObject m_menuOptimization;
    public GameObject m_menuManualAdjustment;

    // Controls 
    private void initControllers()
    {
        List<InputDevice> inputDevices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(m_deviceCharacteristics, inputDevices);
        m_device = inputDevices[0];
        m_prevTrigger = false;
        m_prevGrip = false;
        m_prevPrimary = false;
        m_otherManualAdjustmentControls = m_other.GetComponent<ManualAdjustmentControls>();
        m_thisOptimizationControls = gameObject.GetComponent<ContainerSetupControls>();
        m_otherOptimizationControls = m_other.GetComponent<ContainerSetupControls>();
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

    // Primary button
    private bool getPrimaryValue()
    {
        return this.m_device.TryGetFeatureValue(CommonUsages.primaryButton, out m_primary);
    }
    public bool getPrimaryPress()
    {
        return (m_prevPrimary != m_primary) && m_primary;
    }
    private bool getPrimaryRelease()
    {
        return (m_prevPrimary != m_primary) && !m_primary;
    }
    public bool getPrimaryDown()
    {
        return m_primary;
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
        valid &= getPrimaryValue();
        valid &= getJoystickValue();
        return valid;
    }
    private void updatePreviousInputDeviceStates()
    {
        m_prevTrigger = m_trigger;
        m_prevGrip = m_grip;
        m_prevPrimary = m_primary;
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

    private void toggleManualControl()
    {
        m_menuManualAdjustment.SetActive(false);
        m_menuOptimization.SetActive(true);
        m_otherOptimizationControls.enabled = true;
        m_thisOptimizationControls.enabled = true;
        m_otherManualAdjustmentControls.enabled = false;
        this.enabled = false;
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
                    if (selection.name == "OptimizationSettings")
                    {
                        toggleManualControl();
                    }
                }
                selection = raycast(m_layerMaskElements);
                if (selection != null)
                {
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

                // Scale 
                if (getPrimaryPress() && m_otherManualAdjustmentControls.getPrimaryDown())
                {
                    m_startObjScale = m_selectedElement.transform.localScale;
                    m_startControllerScale = Vector3.Distance(transform.position, m_other.transform.position);
                }
                if (m_performRotation && getPrimaryDown() && m_otherManualAdjustmentControls.getPrimaryDown())
                {
                    m_selectedElement.transform.localScale = (Vector3.Distance(transform.position, m_other.transform.position) / m_startControllerScale) * m_startObjScale;
                }
            }
            
            updatePreviousInputDeviceStates();
        } 
    }
}
