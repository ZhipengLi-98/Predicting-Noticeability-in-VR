using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementRelationshipModel
{
    private double[,] _similarity;
    private double[,] _relevance;

    public ElementRelationshipModel(double[,] similarity, double[,] relevance)
    {
        _similarity = similarity;
        _relevance = relevance;

    }
}
