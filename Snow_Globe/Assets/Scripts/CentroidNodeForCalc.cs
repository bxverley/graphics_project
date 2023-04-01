using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CentroidNodeForCalc : CentroidNode
{
    // -------------------------- //
    // --- Parent's Variables --- //

    // public CentroidNode leftChild;
    // public CentroidNode rightChild;

    // public Vector3 position;
    // public float radius = 0;

    // -------------------------- //
    // -------------------------- //


    // Reference of being able to access parent variables from child class: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/tutorials/inheritance
    public List<TriangleNode> nearestVertexIndices = new List<TriangleNode>();

}
