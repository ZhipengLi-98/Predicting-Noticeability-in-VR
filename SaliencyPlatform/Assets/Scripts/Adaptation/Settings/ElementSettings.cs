using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ElementObjectRelationship
{
    [SerializeField] public string environmentObject;

    [Range(1, 7)]
    [SerializeField] public int association = 1;
}

public class ElementSettings : MonoBehaviour
{
    public Constants.Dimensionality type;

    [Range(Constants.MIN_RATING, Constants.MAX_RATING)]
    public int utility = Constants.MIN_RATING;

    [Range(Constants.MIN_RATING, Constants.MAX_RATING)]
    public int visibilityRequirement = Constants.MIN_RATING;

    [Range(Constants.MIN_RATING, Constants.MAX_RATING)]
    public int touchRequirement = Constants.MIN_RATING;

    [Range(Constants.MIN_RATING, Constants.MAX_RATING)]
    public int backgroundTolerance = Constants.MIN_RATING;

    public double minimumScale = Constants.MIN_ELEMENT_SCALE;
    public double maximumScale = Constants.MAX_ELEMENT_SCALE;

    public ElementObjectRelationship[] anchorObjects;
    public ElementObjectRelationship[] avoidObjects;


}
