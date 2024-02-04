using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class ContainerSetupControlsV2 : MonoBehaviour
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
    private int m_layerMaskAll, m_layerMaskUI, m_layerMaskElements, m_layerMaskObjects, m_layerMaskConnection;
    private GameObject m_laserPointer;

    // Controls 
    public InputDeviceCharacteristics m_deviceCharacteristics;
    private InputDevice m_device;
    public bool m_handleContainerSetup; 
    private enum DeviceState { IDLE, DEFINING_CONNECTION };
    private DeviceState m_state;
    private bool m_prevTrigger;
    private bool m_trigger;
    public float m_joystickThreshold;
    public float m_rangeAdjustmentSpeed;
    private Vector2 m_joystick; 
    
    private float m_selectDistance;

    // Semantic connections 
    public GameObject m_semanticConnectionPrefab;

    // Optimization
    public OptimizationV2 m_optimizer;

    // Other controller reference 
    public GameObject m_other;
    private ContainerSetupControlsV2 m_otherOptimizationControls;
    private ManualAdjustmentControlsV2 m_thisManualAdjustmentControls, m_otherManualAdjustmentControls; 

    // Menus 
    public GameObject m_menuHMD;
    public GameObject m_menuOptimization;
    public GameObject m_menuManualAdjustment;

    // Evaluation 
    public EvaluationExperimentManager m_experimentManager; 

    private void initControllers()
    {
        List<InputDevice> inputDevices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(m_deviceCharacteristics, inputDevices);
        m_device = inputDevices[0];
        m_prevTrigger = false;
        m_otherManualAdjustmentControls = m_other.GetComponent<ManualAdjustmentControlsV2>();
        m_thisManualAdjustmentControls = gameObject.GetComponent<ManualAdjustmentControlsV2>();
        m_otherOptimizationControls = m_other.GetComponent<ContainerSetupControlsV2>();
    }

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

    private bool getJoystickValue()
    {
        return this.m_device.TryGetFeatureValue(CommonUsages.primary2DAxis, out m_joystick);
    }

    private bool getInputDeviceStates()
    {
        bool valid = getTriggerValue();
        valid &= getJoystickValue();
        return valid; 
    }
    private void updatePreviousInputDeviceStates()
    {
        m_prevTrigger = m_trigger;
    }

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

    private void initRaycast()
    {
        m_layerMaskUI = 1 << 8;
        m_layerMaskElements = 1 << 9;
        m_layerMaskObjects = 1 << 10;
        m_layerMaskConnection = 1 << 11; 
        m_layerMaskAll = m_layerMaskUI | m_layerMaskElements | m_layerMaskObjects | m_layerMaskConnection;
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
        } else
        {
            // visual feedback
            m_laserPointer.SetActive(false);

            // return 
            return null;
        }

    }

    private void initDefineConnection()
    {
        m_state = DeviceState.DEFINING_CONNECTION;
        SemanticConnectionSetupHelper.init(); 
    }

    private void toggleMenu()
    {
        m_menuHMD.SetActive(!m_menuHMD.activeInHierarchy);
    }

    private void toggleManualControl()
    {
        m_menuManualAdjustment.SetActive(true);
        m_menuOptimization.SetActive(false);
        m_otherManualAdjustmentControls.enabled = true;
        m_thisManualAdjustmentControls.enabled = true;
        m_otherOptimizationControls.enabled = false; 
        this.enabled = false;
    }

    private void triggerPressOnMenu(string menuItem)
    {
        switch (menuItem)
        {
            case "Optimize":
                m_optimizer.optimize();
                break;
            case "AddConnection":
                if (m_optimizer.optimized) initDefineConnection();
                break;
            case "ClearConnections":
                m_optimizer.clearConnections();
                break;
            case "ManualAdjustment":
                m_optimizer.clearConnectionTransforms();
                toggleManualControl();
                break;
            case "Menu":
                toggleMenu();
                break;
            case "Reset":
                m_optimizer.resetOptimization();
                break;
            case "EnvOffice":
                m_optimizer.changeEnvironments(Constants.Environments.Office);
                break;
            case "EnvBedroom":
                m_optimizer.changeEnvironments(Constants.Environments.Bedroom);
                break;
            // Evaluation 
            case "Next":
                m_experimentManager.nextEnvironment();
                break;
        }
    } 

    // Start is called before the first frame update
    void Start()
    {
        initControllers();
        initLR();
        initRaycast();
        m_state = DeviceState.IDLE;
    }

    // Update is called once per frame
    void Update()
    {
        calcLR();
        updateLR();
        raycast(m_layerMaskAll);

        if (getInputDeviceStates() && m_handleContainerSetup)
        {
            switch (m_state)
            {
                case DeviceState.IDLE:
                    if (getTriggerPress())
                    {
                        // UI Selection
                        Transform selection = raycast(m_layerMaskUI);
                        if (selection != null)
                        {
                            triggerPressOnMenu(selection.name);
                        } else
                        {
                            // Breaking Connections 
                            selection = raycast(m_layerMaskConnection);
                            if (selection != null)
                            {
                                m_optimizer.removeConnection(selection);
                            }
                        }
                    }
                    break;
                case DeviceState.DEFINING_CONNECTION:
                    if (getTriggerPress())
                    {
                        int raycastLayer = m_layerMaskObjects | m_layerMaskElements; 
                        if (SemanticConnectionSetupHelper.m_object == null && SemanticConnectionSetupHelper.m_element != null)
                        {
                            raycastLayer = m_layerMaskObjects;
                        } else if (SemanticConnectionSetupHelper.m_object != null && SemanticConnectionSetupHelper.m_element == null)
                        {
                            raycastLayer = m_layerMaskElements;
                        }
                        Transform selection = raycast(raycastLayer);
                        if (selection != null)
                        {
                            int selectionLayer = 1 << selection.gameObject.layer;
                            bool initConnection = false;
                            if (selectionLayer == m_layerMaskElements)
                            {
                                initConnection = SemanticConnectionSetupHelper.addElement(selection);
                            }
                            else if (selectionLayer == m_layerMaskObjects)
                            {
                                initConnection = SemanticConnectionSetupHelper.addObject(selection);
                            }
                            if (initConnection)
                            {
                                m_optimizer.addConnection(SemanticConnectionSetupHelper.m_element, SemanticConnectionSetupHelper.m_object, 1.0, true);
                                m_state = DeviceState.IDLE;
                            }
                        } 
                    }
                    break;
            }

            updatePreviousInputDeviceStates();
        }
    }
}
