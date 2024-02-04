using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SceneManagement;
using System.IO;

public class TutorialManager : MonoBehaviour
{
    // Right Touch Controller
    private InputDevice m_rightController;

    // Text Display 
    public TextMeshProUGUI m_text;

    // Track Secondary Button
    private bool m_prevSecondaryButton;
    private bool m_secondaryButton;

    // Tutorial Elements 
    public GameObject m_walkTarget; 
    public GameObject m_virtualObject;
    public GameObject m_virtualObjectTarget;
    public GameObject m_rotationTarget;
    public GameObject[] m_rotationCheck;

    public bool m_skipTutorial = false; 

    // Touch Controller Input Handling
    private void getSecondaryButtonValue()
    {
        this.m_rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out this.m_secondaryButton);
    }
    private void getInputDeviceStates()
    {
        this.getSecondaryButtonValue();
    }
    private void updatePreviousInputDeviceStates()
    {
        this.m_prevSecondaryButton = this.m_secondaryButton;
    }
    
    private bool getSecondaryPress()
    {
        return this.m_prevSecondaryButton != this.m_secondaryButton && this.m_secondaryButton;
    }

    IEnumerator Instruction(string instruction)
    {
        this.m_text.SetText(instruction);
        yield return new WaitForSeconds(0.3f);
        while (true)
        {
            this.getInputDeviceStates();
            if (this.getSecondaryPress())
            {
                break;
            }
            this.updatePreviousInputDeviceStates();
            yield return null; 
        }
    }

    IEnumerator WalkToTarget()
    {
        this.m_text.SetText("Walk to the yellow target to continue.");
        Vector3 pos = Camera.main.transform.position;
        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0;
        forward.Normalize();
        pos += 2.0f * forward;
        pos.y = 1;
        this.m_walkTarget.transform.position = pos;
        this.m_walkTarget.SetActive(true);
        while (Vector3.Distance(Camera.main.transform.position, this.m_walkTarget.transform.position) > 1.0f)
        {
            yield return null; 
        }
        this.m_walkTarget.SetActive(false);
    }

    private void revealVirtualObject()
    {
        Vector3 pos = Camera.main.transform.position;
        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0;
        forward.Normalize();
        pos += 2.0f * forward;
        pos.y = 1;
        this.m_virtualObject.transform.position = pos;
        this.m_virtualObject.SetActive(true);
    }

    IEnumerator MoveToTarget()
    {
        this.m_text.SetText("Move virtual object to the target to continue.");
        yield return new WaitForSeconds(0.5f);
        Vector3 pos = Camera.main.transform.position;
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;
        forward.y = 0;
        forward.Normalize();
        right.y = 0;
        right.Normalize();
        pos += (2.0f * forward) + (2.0f * right);
        pos.y = 1;
        this.m_virtualObjectTarget.transform.position = pos;
        this.m_virtualObjectTarget.SetActive(true);
        while (Vector3.Distance(this.m_virtualObjectTarget.transform.position, this.m_virtualObject.transform.position) > 0.1f)
        {
            yield return null;
        }
        this.m_virtualObjectTarget.SetActive(false);
    }

    IEnumerator ScaleToTarget()
    {
        this.m_text.SetText("Scale the virtual object to the target to continue.");
        yield return new WaitForSeconds(0.5f);
        Vector3 lockedPosition = this.m_virtualObject.transform.position;
        this.m_virtualObjectTarget.transform.position = lockedPosition;
        this.m_virtualObjectTarget.transform.localScale = new Vector3(0.25f, 0.5f, 0.25f);

        this.m_virtualObjectTarget.SetActive(true);
        while (Vector3.Distance(this.m_virtualObjectTarget.transform.localScale, this.m_virtualObject.transform.localScale) > 0.1f)
        {
            this.m_virtualObject.transform.position = lockedPosition;
            yield return null;
        }
        this.m_virtualObjectTarget.SetActive(false);
    }

    IEnumerator RotateToTarget()
    {
        this.m_text.SetText("Rotate the virtual object to the target to continue.");
        yield return new WaitForSeconds(0.5f);
        Vector3 lockedPosition = this.m_virtualObject.transform.position;
        this.m_virtualObjectTarget.transform.position = lockedPosition;
        this.m_virtualObjectTarget.transform.localScale = this.m_virtualObject.transform.localScale;
        this.m_virtualObjectTarget.transform.eulerAngles = new Vector3(0, 0, 90);
        this.m_virtualObjectTarget.SetActive(true);
        bool isRotated = false; 
        while (true)
        {
            this.m_virtualObject.transform.position = lockedPosition;
            isRotated = Mathf.Min(
                Vector3.Distance(this.m_rotationCheck[0].transform.position, this.m_rotationTarget.transform.position),
                Vector3.Distance(this.m_rotationCheck[1].transform.position, this.m_rotationTarget.transform.position)
            ) < 0.1f;
            if (isRotated) break; 
            yield return null; 
        }
        this.m_virtualObjectTarget.SetActive(false);
    }

    IEnumerator StartExperiment()
    {
        this.m_text.SetText("Please notify the researcher now.");
        yield return new WaitForSeconds(0.3f);
        while (true)
        {
            this.getInputDeviceStates();
            if (this.getSecondaryPress())
            {
                break;
            }
            this.updatePreviousInputDeviceStates();
            yield return null;
        }
        GlobalExperimentState.setSceneOrder();
        
        // Log Scene Order 
        string fileName = System.DateTime.Now.Month.ToString("D2") + "-" +
            System.DateTime.Now.Day.ToString("D2") + "-" +
            System.DateTime.Now.Hour.ToString("D2") + "-" +
            System.DateTime.Now.Minute.ToString("D2") + "-" +
            System.DateTime.Now.Second.ToString("D2");
        StreamWriter sw = new StreamWriter(Application.persistentDataPath + "/sceneOrder_" + fileName + ".txt");
        foreach(string scene in GlobalExperimentState.sceneOrder)
        {
            sw.WriteLine(scene);
        }
        if (sw.BaseStream != null) sw.Close();

        // Delay
        yield return new WaitForSeconds(0.3f);

        // Proceed to next scene
        SceneManager.LoadScene(GlobalExperimentState.getCurrScene());
    }

    IEnumerator TutorialSequence()
    {
        Coroutine c = StartCoroutine(Instruction("Welcome! Thank you for participating in our pilot study. Going forward, press B to continue unless instructed otherwise."));
        yield return c;
        if (!this.m_skipTutorial) { 
            c = StartCoroutine(Instruction("In this experiment, your task will be to design and adapt MR content layouts for different environments."));
            yield return c;
            c = StartCoroutine(Instruction("This tutorial will help you familiarize yourself with the experiment controls."));
            yield return c;

            c = StartCoroutine(Instruction("Use the left joystick to walk around the scene."));
            yield return c;
            c = StartCoroutine(WalkToTarget());
            yield return c;

            revealVirtualObject();
            c = StartCoroutine(Instruction("Virtual objects are indicated with a semi-transparent black outline."));
            yield return c;

            c = StartCoroutine(Instruction("Use the trigger on either controller to grab and move virtual objects."));
            yield return c;
            c = StartCoroutine(Instruction("Use the right joystick to bring the grabbed object closer or further away."));
            yield return c;
            c = StartCoroutine(MoveToTarget());
            yield return c;

            c = StartCoroutine(Instruction("Hold both A and X to scale objects."));
            yield return c;
            c = StartCoroutine(ScaleToTarget());
            yield return c;

            c = StartCoroutine(Instruction("Use the right grip to rotate objects."));
            yield return c;
            c = StartCoroutine(RotateToTarget());
            yield return c;

            c = StartCoroutine(Instruction("You are ready to begin."));
            yield return c;
            c = StartCoroutine(Instruction("Feel free to stay in the tutorial until you feel comfortable with the controls."));
            yield return c;
        }
        c = StartCoroutine(StartExperiment());
        yield return c; 

    }

    // Start is called before the first frame update
    void Start()
    {
        // Initialize right controller
        List<InputDevice> inputDevices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right, inputDevices);
        this.m_rightController = inputDevices[0];

        StartCoroutine(TutorialSequence());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
