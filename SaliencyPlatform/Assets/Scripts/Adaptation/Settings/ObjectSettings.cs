using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSettings : MonoBehaviour
{
    [Range(Constants.MIN_RATING, Constants.MAX_RATING)]
    public int utility = Constants.MIN_RATING;
}
