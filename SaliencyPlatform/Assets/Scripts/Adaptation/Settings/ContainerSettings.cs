using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ContainerObjectRelationship
{
    [SerializeField] public string environmentObject;

    [Range(1, 7)]
    [SerializeField] public int association = 1;
}

public class ContainerSettings : MonoBehaviour
{
    public Constants.Dimensionality type;

    public bool overrideUtility = false;
    [Range(Constants.MIN_RATING, Constants.MAX_RATING)]
    public int utility = Constants.MIN_RATING;

    public ContainerObjectRelationship[] associations;

}
