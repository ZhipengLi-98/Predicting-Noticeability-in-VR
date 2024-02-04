using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ElementRelationship
{
    [SerializeField] public GameObject element1, element2;

    [Range(1, 7)]
    [SerializeField] public int connection = 1;

}

public class BetweenElementSettings : MonoBehaviour
{
    public ElementRelationship[] elementRelationships;
}
