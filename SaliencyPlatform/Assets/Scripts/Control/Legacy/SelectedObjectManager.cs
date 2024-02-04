using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SelectedObjectManager : MonoBehaviour
{
    // Field in shader
    private string m_colorField = "_OutlineColor";
    // Color for deselected objects
    public Color m_deselectedColor;
    // Color for selected objects 
    public Color m_selectedColor; 

    // Selected object 
    private GameObject m_selectedObject;

    // Translation 
    // Selected controller
    private GameObject m_selectionController;
    // Select distance 
    private float m_selectedDistance;
    // Whether selected object is moving 
    private bool m_selectedObjectMoving;

    // Rotation 
    //private GameObject m_rotationAnchor;
    //private Vector3 m_startRotControllerDir;
    private Quaternion m_startRotControllerRotationInv; 
    private Quaternion m_startRotObjRotation;
    private Vector3 m_rotPosition;
    private GameObject m_rotationClone;
    public bool m_isRotating;
    //private Vector3 m_currRotControllerDir;

    // Scaling
    private float m_startScaleControllerDist;
    private Vector3 m_startScaleObjectScale; 

    private void updateOutlineColor(GameObject obj, Color c)
    {
        Renderer r = obj.GetComponent<Renderer>();
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        r.GetPropertyBlock(mpb);
        mpb.SetColor(this.m_colorField, c);
        r.SetPropertyBlock(mpb);
    }

    public void updateSelectedObj(GameObject newSelectedObj, GameObject selectionController)
    {
        if (newSelectedObj != null)
        {
            if (this.m_selectedObject != null)
            {
                if (newSelectedObj != this.m_selectedObject)
                {
                    this.updateOutlineColor(this.m_selectedObject, this.m_deselectedColor);
                    this.m_selectedObject = null;
                }
            }
            this.m_selectedObject = newSelectedObj;
            this.m_selectionController = selectionController;
            this.updateOutlineColor(this.m_selectedObject, this.m_selectedColor);
        } else
        {
            this.updateOutlineColor(this.m_selectedObject, this.m_deselectedColor);
            this.m_selectedObject = null;
        }
    }

    public void setSelectedDistance(Vector3 controllerPos)
    {
        this.m_selectedDistance = Vector3.Distance(this.m_selectedObject.transform.position, controllerPos);
    }

    public void updateSelectedDistance(float update)
    {
        this.m_selectedDistance += update;
    }

    public float getSelectedDistance()
    {
        return this.m_selectedDistance;
    }

    public void setSelectedObjectPosition(Vector3 objectPosition)
    {
        this.m_selectedObject.transform.position = objectPosition;
    }

    public GameObject getSelectedObject()
    {
        return this.m_selectedObject;
    }

    public bool isObjectSelected()
    {
        return this.m_selectedObject != null;
    }

    public void setObjectMoving(bool moving)
    {
        this.m_selectedObjectMoving = moving;
    }

    public bool isObjectMoving()
    {
        return this.m_selectedObjectMoving;
    }

    public GameObject getSelectionController()
    {
        return this.m_selectionController;
    }

    public void setRotationStartOrientation(GameObject rotationController)
    {
        this.m_isRotating = true; 
        this.m_startRotObjRotation = this.m_selectedObject.transform.rotation;
        this.m_rotationClone = GameObject.Instantiate(this.m_selectedObject, this.m_selectedObject.transform.parent);
        this.m_rotationClone.layer = 0;
        this.updateOutlineColor(this.m_rotationClone, this.m_selectedColor);
        this.m_rotationClone.transform.position = rotationController.transform.position;
        this.m_rotationClone.transform.rotation = this.m_startRotObjRotation;
        this.m_startRotControllerRotationInv = Quaternion.Inverse(rotationController.transform.rotation);
    }

    public void rotateObject(GameObject rotationController)
    {
        Quaternion updateRotation = (rotationController.transform.rotation * this.m_startRotControllerRotationInv) * this.m_startRotObjRotation;
        // Rotation Clone
        this.m_rotationClone.transform.position = rotationController.transform.position;
        this.m_rotationClone.transform.rotation = updateRotation;
        // Actual Object
        this.m_selectedObject.transform.rotation = updateRotation;
    }

    public void endRotation()
    {
        this.m_isRotating = false; 
        GameObject.DestroyImmediate(this.m_rotationClone);
    }

    /*
    public void setRotationAnchor(GameObject rotationAnchorController)
    {
        this.m_rotationAnchor = rotationAnchorController;
    }

    public void setRotationStartOrientation(GameObject rotationController)
    {
        this.m_startRotControllerDir = (rotationController.transform.position - this.m_rotationAnchor.transform.position).normalized;
        this.m_startRotObjRotation = this.m_selectedObject.transform.rotation;
    }

    public void rotateObject(GameObject rotationController)
    {
        this.m_currRotControllerDir = (rotationController.transform.position - this.m_rotationAnchor.transform.position).normalized;
        this.m_selectedObject.transform.rotation = Quaternion.FromToRotation(this.m_startRotControllerDir, this.m_currRotControllerDir) * this.m_startRotObjRotation;
    }

    public GameObject getRotationAnchor()
    {
        return this.m_rotationAnchor;
    }
    */

    public void startScale(Vector3 a, Vector3 b)
    {
        this.m_startScaleControllerDist = Vector3.Distance(a, b);
        this.m_startScaleObjectScale = this.m_selectedObject.transform.localScale;
    }

    public void scaleObject(Vector3 a, Vector3 b)
    {
        this.m_selectedObject.transform.localScale = (Vector3.Distance(a, b) / this.m_startScaleControllerDist) * this.m_startScaleObjectScale;
    }

    // Start is called before the first frame update
    void Start()
    {
        this.m_selectedObject = null;
    }
}
