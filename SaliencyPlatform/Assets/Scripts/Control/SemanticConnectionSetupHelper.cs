using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SemanticConnectionSetupHelper {
    public static Transform m_element;
    public static Transform m_object;
    public static Transform m_connection;

    public static void clear()
    {
        m_element = null;
        m_object = null;
        m_connection = null;
    }

    public static void init()
    {
        clear(); 
    }
    

    public static bool addElement(Transform element)
    {
        m_element = element;
        if (m_object != null) return true;
        return false; 
    }

    public static bool addObject(Transform obj)
    {
        m_object = obj;
        if (m_element != null) return true;
        return false;
    }
}
