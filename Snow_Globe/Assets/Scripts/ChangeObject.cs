using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class ChangeObject { 
    
    static string[] objectsNames = new string[] {"original", "tree", "lamp", "tower", "sphere"}; 
    static int objectNameIndex = 0;

    
    public static void ChangeNextGlobeObject()
    {
        if (objectNameIndex < objectsNames.Length - 1)
        {
            objectNameIndex++;
        }
        else
        {
            objectNameIndex = 0;
        }
    }
    public static void ChangePrevGlobeObject()
    {
        if (objectNameIndex > 0)
        {
            objectNameIndex--;
        }
        else
        {
            objectNameIndex = objectsNames.Length - 1;
        }
    }

    public static string GetGlobeObjName()
    {
        return objectsNames[objectNameIndex];
    }
}
