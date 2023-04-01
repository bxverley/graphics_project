using System;
using System.Collections.Generic;
using UnityEngine;

public class KMeansFunctions : MonoBehaviour
{
    private KDTree kDTreeUnoptimised = new();
    private List<CentroidNodeForCalc> allCentroidNodeForCalc = new List<CentroidNodeForCalc>();
    private List<bool> centroidChanged = new();

    private float maximumAcceptableDist = 0.001f;
    private float maximumAcceptableCosAngle = -0.01f;                                                           // Allowed angle to be more than 90 degrees. So vectors compared are more likely to be pointing in the oppsite direction.

    private List<Vector3> verticesOfTriangles;
    private List<TriangleNode> vertices;
    public KDTree triangleCenterVerticesTree;
    private List<int> triangles;

    private CentroidNodeForCalc[,] arrayToDistributeEqually;                                                    // For storing centroids at intervals. To check distribution of centroid locations.
    private int[] arrayToDistributeEqually_XYZIndex = new int[3];                                               // For storing result of calculation of indices to access slots in arrayToDistributeEqually

    private float[] minMaxIntervalRange_xyz;
    private float[] min_xyz;
    private float[] max_xyz;

    private bool firstRoundCalc = true;
    private bool continueCalc;

    private int counter = 0;

    private const int spreadIterationCount = 3;
    private const int kMeansCalcIterationCount = 30;

    private List<GameObject> allSpheres = new List<GameObject>();


    // To REUSE for any Vector3 variable, CentroidNodeForCalc variable, Trianglenode variable, and CentroidNode variable, as and when needed .
    private Vector3 currentVector3 = new Vector3(0, 0, 0);
    private Vector3 currentVector3_temp = new Vector3(0, 0, 0);
    private CentroidNodeForCalc currentCentroidNodeForCalc;
    private TriangleNode currentTriangleNode;
    private CentroidNode nearestNode;
    private CentroidNode nearestNodeInTempTree;
    private List<CentroidNodeForCalc> tempCentroidNodeForCalcList;
    private KDTree tempKDTree;
    private List<TriangleNode> tempListOfTriNodesTooFar;
    private List<bool> tempCentroidChanged;

   





    public void Start()
    {

        vertices = new List<TriangleNode>();
        triangleCenterVerticesTree = new KDTree();
        int triangleRowIndex;

        verticesOfTriangles = gameObject.GetComponent<SnowGlobe>().vertices;
        triangles = gameObject.GetComponent<SnowGlobe>().triangles;
        

        for (int i = 0; i < triangles.Count / 3; i++)
        {
            triangleRowIndex = i * 3;

            // Get midpoint of triangle.
            currentVector3 = verticesOfTriangles[triangles[triangleRowIndex]] + verticesOfTriangles[triangles[triangleRowIndex + 1]] + verticesOfTriangles[triangles[triangleRowIndex + 2]];
            currentVector3 /= 3;

            currentTriangleNode = new()
            {
                position = currentVector3
            };

            // Normalise the cross product
            // Cross product of vertex 1 - vertex 0 and vertex 2 - vertex 0
            // Get normal facing away from you by right hand rule.
            currentVector3 = Vector3.Normalize(Vector3.Cross((verticesOfTriangles[triangles[triangleRowIndex + 1]] - verticesOfTriangles[triangles[triangleRowIndex]]),
                                                                (verticesOfTriangles[triangles[triangleRowIndex + 2]] - verticesOfTriangles[triangles[triangleRowIndex]])));

            currentTriangleNode.triangleNormal = currentVector3;
            currentTriangleNode.triangleIndex = i;

            vertices.Add(currentTriangleNode);

            triangleCenterVerticesTree.Insert(currentTriangleNode);
        }
    }

    public void BeginKMeansOperations(int numberOfCentroids)
    {
        CentroidGeneration(numberOfCentroids, vertices, kDTreeUnoptimised, allCentroidNodeForCalc, centroidChanged, true);
        KMeans(vertices, kDTreeUnoptimised, allCentroidNodeForCalc, centroidChanged, false);
        CalcMinDistFromCentroids(allCentroidNodeForCalc);


        counter = 0;

        
        tempKDTree = new KDTree();
        tempCentroidNodeForCalcList = new List<CentroidNodeForCalc>();
        tempListOfTriNodesTooFar = new List<TriangleNode>();
        tempCentroidChanged = new List<bool>();

        for (int i = 0; i < kMeansCalcIterationCount; i++)
        {
            ResetAllReuseVariables();

            firstRoundCalc = true;

            // Collect all triangle midpoints that are too far from the bounding spheres
            for (int j = 0; j < vertices.Count; j++)
            {
                SearchNearestCentroidOrAdd(vertices[j], tempListOfTriNodesTooFar);
            }

            CentroidGeneration(numberOfCentroids, tempListOfTriNodesTooFar, tempKDTree, tempCentroidNodeForCalcList, tempCentroidChanged, false);

            KMeans(tempListOfTriNodesTooFar, tempKDTree, tempCentroidNodeForCalcList, tempCentroidChanged, true);

            CalcMinDistFromCentroids(tempCentroidNodeForCalcList);

            ResetEachCentroidNodeForCalc(tempCentroidNodeForCalcList);

            InsertToMainKDTree(tempCentroidNodeForCalcList);
            
        }

        ResetAllReuseVariables();

        int startIndex = 0;

        for(int i = 0; i < spreadIterationCount; i++)
        {
            Spread(allCentroidNodeForCalc, tempCentroidNodeForCalcList, startIndex);

            startIndex = allCentroidNodeForCalc.Count;

            InsertToMainKDTree(tempCentroidNodeForCalcList);

            ResetAllReuseVariables();

        }

        EnsureWithinMinMax();

        EnsureAllBoundsInsideMesh();
    }











    public void CentroidGeneration(int numberOfCentroids, List<TriangleNode> triangleMidVerticesInvolved, KDTree kdTreeInvolved, List<CentroidNodeForCalc> centroidNodeForCalcList, List<bool> centroidChangedBoolList, bool uniformDistribution)
    {
        
        int randomIndex;


        for (int i = 0; i < numberOfCentroids; i++)
        {
            currentCentroidNodeForCalc = new CentroidNodeForCalc();
            randomIndex = UnityEngine.Random.Range(0, triangleMidVerticesInvolved.Count - 1);

            currentCentroidNodeForCalc.position = triangleMidVerticesInvolved[randomIndex].position + (-0.01f * triangleMidVerticesInvolved[randomIndex].triangleNormal);                                     // (-0.01f * vertices[randomIndex].triangleNormal) is to shift centroid slightly inwards into the mesh, that is, slightly away from the mesh face.;

            if (uniformDistribution)
            {
                // Only first round of centroid generation have high chance of getting uniform distribution without errors. The next few rounds might have errors as the list of nodes used
                //  consist of triangle midpoint nodes that are not distributed across uniform intervals from min_xyz to max_xyz of the whole mesh. Some of the uniform intervals may not even have
                //  nodes that are too far from the bounding spheres.

                minMaxIntervalRange_xyz = new float[3];
                arrayToDistributeEqually = new CentroidNodeForCalc[3, numberOfCentroids];

                min_xyz = gameObject.GetComponent<SnowGlobe>().min_xyz;
                max_xyz = gameObject.GetComponent<SnowGlobe>().max_xyz;

                for (int j = 0; j < 3; j++)
                {
                    minMaxIntervalRange_xyz[j] = (gameObject.GetComponent<SnowGlobe>().max_xyz[j] - min_xyz[j]) / numberOfCentroids;
                }


                arrayToDistributeEqually_XYZIndex[0] = (int)((currentCentroidNodeForCalc.position[0] - min_xyz[0]) / minMaxIntervalRange_xyz[0]);
                arrayToDistributeEqually_XYZIndex[1] = (int)((currentCentroidNodeForCalc.position[1] - min_xyz[1]) / minMaxIntervalRange_xyz[1]);
                arrayToDistributeEqually_XYZIndex[2] = (int)((currentCentroidNodeForCalc.position[2] - min_xyz[2]) / minMaxIntervalRange_xyz[2]);



                // Keeps finding a new vertex whose x, y, z would fall in the intervals of sizes (in the x, y and z direction) as specified in minMaxIntervalRange_xyz
                // - Prevents using vertices that are opposite of each other on an axis, or are too close to each other.
                // Subtracting by min_xyz is so that the division does not become a negative number (without subtracting, if the min on the x, y and z ranges are negative, the division would become negative, causing exception). 
                while (arrayToDistributeEqually[0, arrayToDistributeEqually_XYZIndex[0]] != null &&
                        arrayToDistributeEqually[1, arrayToDistributeEqually_XYZIndex[1]] != null &&
                        arrayToDistributeEqually[2, arrayToDistributeEqually_XYZIndex[2]] != null)
                {

                    randomIndex = UnityEngine.Random.Range(0, triangleMidVerticesInvolved.Count - 1);
                    currentCentroidNodeForCalc.position = triangleMidVerticesInvolved[randomIndex].position + (-0.01f * triangleMidVerticesInvolved[randomIndex].triangleNormal);                                  // (-0.01f * vertices[randomIndex].triangleNormal) is to shift centroid slightly inwards into the mesh, that is, slightly away from the mesh face.

                    arrayToDistributeEqually_XYZIndex[0] = (int)((currentCentroidNodeForCalc.position[0] - min_xyz[0]) / minMaxIntervalRange_xyz[0]);
                    arrayToDistributeEqually_XYZIndex[1] = (int)((currentCentroidNodeForCalc.position[1] - min_xyz[1]) / minMaxIntervalRange_xyz[1]);
                    arrayToDistributeEqually_XYZIndex[2] = (int)((currentCentroidNodeForCalc.position[2] - min_xyz[2]) / minMaxIntervalRange_xyz[2]);

                }

                // To ensure that in a certain interval of x, y, and z, only one centroid exist. Basically to try and evenly spread out the centroids, in case 2 are side by side.
                // Would reach here when finally found an interval that does not have centroids.
                arrayToDistributeEqually[0, arrayToDistributeEqually_XYZIndex[0]] = currentCentroidNodeForCalc;
                arrayToDistributeEqually[1, arrayToDistributeEqually_XYZIndex[1]] = currentCentroidNodeForCalc;
                arrayToDistributeEqually[2, arrayToDistributeEqually_XYZIndex[2]] = currentCentroidNodeForCalc;
            }

            InsertToKDTree(currentCentroidNodeForCalc, kdTreeInvolved, centroidNodeForCalcList, centroidChangedBoolList);
        }

        
    }
    
    public void InsertToKDTree(CentroidNodeForCalc node, KDTree kdTreeInvolved, List<CentroidNodeForCalc> centroidNodeForCalcList, List<bool> centroidChangedBoolList)
    {
        kdTreeInvolved.Insert(node);

        if (firstRoundCalc)
        {
            centroidNodeForCalcList.Add(node);
            centroidChangedBoolList.Add(false);
        }
        
    }

    public void InsertToMainKDTree(List<CentroidNodeForCalc> centroidNodeForCalcList)
    {
        for(int i = 0; i < centroidNodeForCalcList.Count; i++)
        {
            kDTreeUnoptimised.Insert(centroidNodeForCalcList[i]);
            allCentroidNodeForCalc.Add(centroidNodeForCalcList[i]);
            centroidChanged.Add(false);
        }
        
    }


   











    public void KMeans(List<TriangleNode> triangleMidVerticesInvolved, KDTree kdTreeInvolved, List<CentroidNodeForCalc> centroidNodeForCalcList, List<bool> centroidChangedBoolList, bool includeNearestCentroid)
    {
        counter = 0;

        continueCalc = true;

        // OPERATION.
        while (continueCalc && counter < 30)
        {
            counter++;

            for (int i = 0; i < triangleMidVerticesInvolved.Count; i++)
            {
                SearchNearestCentroid(triangleMidVerticesInvolved[i], kdTreeInvolved);
            }

            
            if (includeNearestCentroid)
            {
                for (int i = 0; i < centroidNodeForCalcList.Count; i++)
                {
                    nearestNode = kDTreeUnoptimised.StartSearch(centroidNodeForCalcList[i].position);
                    centroidNodeForCalcList[i].nearestVertexIndices.Add(new TriangleNode() { position = nearestNode.position });
                }
                
            }

            CalcNewCentroids(centroidNodeForCalcList, centroidChangedBoolList);

            if (includeNearestCentroid) {
                continueCalc = false;
            }

            // continueCalc is a global bool variable which would be changed in CalcNewCentroids().
            if (continueCalc)
            {
                ResetCentroidChangedList(centroidChangedBoolList);                                      // Resetting the bool list which was for determining if there is a centroid that is changed.
                ResetEachCentroidNodeForCalc(centroidNodeForCalcList);                                  // 0 is the index for the very first element. So reset children and internal variables of the CentroidNodeForCalc class objects.
                kdTreeInvolved.SetRootNode(null);

                // Reconstruct KDTree since new positions calculated.
                for (int i = 0; i < centroidNodeForCalcList.Count; i++)
                {
                    InsertToKDTree(centroidNodeForCalcList[i], kdTreeInvolved, centroidNodeForCalcList, centroidChangedBoolList);
                }
            }

        }
    }

    public void SearchNearestCentroid(TriangleNode vertex, KDTree kdTreeInvolved)
    {
        // For memmory optimisation, DO NOT CREATE NEW VECTOR3 REFERENCE VARIABLE!! REUSE VARIABLES!
        currentVector3 = vertex.position;                                                             // Now setting current vertex


        nearestNode = kdTreeInvolved.StartSearch(currentVector3);

        
        if (Vector3.Dot( (nearestNode.position - vertex.position), vertex.triangleNormal) < maximumAcceptableCosAngle)
        {
            // Entering this if-block means that the vector from the vertex to the centroid does not go out of the mesh,
            // since that vector is in the opposite direction as the normal of the vertex.

            // No implication on memory even if casting? Reference: https://stackoverflow.com/questions/40298290/explicit-cast-explanation-in-terms-of-memory-for-reference-type-in-c-sharp
            ((CentroidNodeForCalc)nearestNode).nearestVertexIndices.Add(vertex);
        }

    }


    public void SearchNearestCentroidOrAdd(TriangleNode vertex, List<TriangleNode> listOfTriNodesTooFar)
    {
        // For memmory optimisation, DO NOT CREATE NEW VECTOR3!! REUSE VARIABLES!
        currentVector3 = vertex.position;                                                   // Now setting current vertex
        nearestNode = kDTreeUnoptimised.StartSearch(currentVector3);
        currentVector3 = nearestNode.position - vertex.position;


        // Reference to check if angle between two vectors are more than 90 degrees: https://math.stackexchange.com/questions/701656/check-angle-between-two-lines-greater-than-90#:~:text=So%2C%20the%20number%20d%20is,is%20greater%20than%2090%20degrees.
        if ((Vector3.Dot((currentVector3), vertex.triangleNormal) < maximumAcceptableCosAngle)
            && Vector3.Magnitude(currentVector3) < nearestNode.radius + maximumAcceptableDist)
        {
            // - Entering this if-block means that the vector from the vertex to the centroid does not go out of the mesh,
            //    since that vector is in the opposite direction as the normal of the vertex.
            // - The vertex is also within maximum acceptable range (must be set) away from the bounding sphere of the centroid if it has radius > 0.
            // - Else if the centroid bounding sphere has radius = 0, just add the vertexIndex.

            // No implication on memory even if casting? Reference: https://stackoverflow.com/questions/40298290/explicit-cast-explanation-in-terms-of-memory-for-reference-type-in-c-sharp
            ((CentroidNodeForCalc)nearestNode).nearestVertexIndices.Add(vertex);

        }

        else
        {
            listOfTriNodesTooFar.Add(vertex);
        }

        return;
    }


    public void CalcNewCentroids(List<CentroidNodeForCalc> centroidNodeForCalcList, List<bool> centroidChangedBoolList)
    {
        firstRoundCalc = false;

        for (int i = 0; i < centroidNodeForCalcList.Count; i++) 
        {
            currentCentroidNodeForCalc = centroidNodeForCalcList[i];

            // Sets global variable currentVector3 to center vertex (that is, the vertex that has position which is average of all the vertices used in the calculation). Uses this.currentCentroidNodeForCalc global variable.
            CalcCenterVertex(currentCentroidNodeForCalc);

            // Setting the bool to determine if the operation should continue or not.
            if (Vector3.Magnitude(currentCentroidNodeForCalc.position - currentVector3) > 0.0001)
            {
                // Change centroidPosition only if differnce is very huge.
                currentCentroidNodeForCalc.position = currentVector3;
                centroidChangedBoolList[i] = true;
            }
        }

        int j = 0;
        continueCalc = false;

        while (j < centroidChangedBoolList.Count && !continueCalc)
        {
            if (centroidChangedBoolList[j])
            {
                continueCalc = true;
                return;
            }

            j++;
        }

        return;
    }

    void CalcCenterVertex(CentroidNodeForCalc currentCentroidNodeForCalc)
    {
        // Initialise to 0. Using to sum up all the x, y and z.
        currentVector3[0] = 0;
        currentVector3[1] = 0;
        currentVector3[2] = 0;


        // Summing the x, y and z.
        for (int j = 0; j < currentCentroidNodeForCalc.nearestVertexIndices.Count; j++)
        {
            currentVector3 += currentCentroidNodeForCalc.nearestVertexIndices[j].position;
        }

        // Here we have the new centroid position
        currentVector3[0] /= currentCentroidNodeForCalc.nearestVertexIndices.Count;
        currentVector3[1] /= currentCentroidNodeForCalc.nearestVertexIndices.Count;
        currentVector3[2] /= currentCentroidNodeForCalc.nearestVertexIndices.Count;

        // Don't set the node's position here as we need to keep the old and compare with the new position.
    }


    public void CalcMinDistFromCentroids(List<CentroidNodeForCalc> centroidNodeForCalcList)
    {
        float currentSize;
        float minRadius;
        int indexOfClosestTriangle;

        for(int i = 0; i < centroidNodeForCalcList.Count; i++)
        {
            // Initialise to be a big number.
            minRadius = 1000000;

            // Nearest triangle mid point to the centroid
            currentTriangleNode = (TriangleNode) triangleCenterVerticesTree.StartSearch(centroidNodeForCalcList[i].position);
            indexOfClosestTriangle = currentTriangleNode.triangleIndex;
            minRadius = Vector3.Magnitude(centroidNodeForCalcList[i].position - currentTriangleNode.position);

            // indexOfClosestTriangle would now be set as the index of the triangle closest to the centroid.
            // This for-loop is to check if any of the vertices of the triangle is closer than the triangle center.
            // If so, then set the minRadius to distance between centroid and closest vertex of the triangle.
            for (int j = 0; j < 3; j++)
            {
                currentSize = Vector3.Magnitude(centroidNodeForCalcList[i].position - verticesOfTriangles[ triangles[indexOfClosestTriangle * 3 + j] ]);
                if (currentSize < minRadius)
                {
                    minRadius = currentSize;
                }
            }

            centroidNodeForCalcList[i].radius = minRadius;
        }
    }








    public void Spread(List<CentroidNodeForCalc> centroidNodeForCalcList, List<CentroidNodeForCalc> newCentroidNodeForCalcList, int startIndex)
    {
        for(int i = startIndex; i < centroidNodeForCalcList.Count; i++)
        {
            currentTriangleNode = (TriangleNode) triangleCenterVerticesTree.StartSearch(centroidNodeForCalcList[i].position);
            currentVector3 = Vector3.Normalize(verticesOfTriangles[triangles[currentTriangleNode.triangleIndex * 3]] - currentTriangleNode.position);               // Gets the normalised vector between first vertex of the triangle and the midpoint
            currentVector3_temp = Vector3.Normalize(Vector3.Cross(currentVector3, currentTriangleNode.triangleNormal));                                             // To get the vector perpendicular to the normal as well as the vector between the midpoint and the first vertex of the triangle.

            // Add new centroid in direction towards first vertex of the nearest triangle
            currentCentroidNodeForCalc = new CentroidNodeForCalc();
            currentCentroidNodeForCalc.position = centroidNodeForCalcList[i].position + currentVector3 * centroidNodeForCalcList[i].radius;
            Spread_CalcRadius(currentCentroidNodeForCalc, newCentroidNodeForCalcList);

            // Add new centroid in the direction opposite to that of the vector that is in the direction of the first vertex of the nearest triangle.
            currentCentroidNodeForCalc = new CentroidNodeForCalc();
            currentCentroidNodeForCalc.position = centroidNodeForCalcList[i].position + -1 * centroidNodeForCalcList[i].radius * currentVector3;
            Spread_CalcRadius(currentCentroidNodeForCalc, newCentroidNodeForCalcList);


            // Add new centroid in the direction of the cross product of the vector between the midpoint of the triangle and the first vertex with the triangle normal
            currentCentroidNodeForCalc = new CentroidNodeForCalc();
            currentCentroidNodeForCalc.position = centroidNodeForCalcList[i].position + currentVector3_temp * centroidNodeForCalcList[i].radius;
            Spread_CalcRadius(currentCentroidNodeForCalc, newCentroidNodeForCalcList);


            // Add new centroid in the opposite direction of the cross product of the vector between the midpoint of the triangle and the first vertex with the triangle normal
            currentCentroidNodeForCalc = new CentroidNodeForCalc();
            currentCentroidNodeForCalc.position = centroidNodeForCalcList[i].position + -1 * centroidNodeForCalcList[i].radius * currentVector3_temp;
            Spread_CalcRadius(currentCentroidNodeForCalc, newCentroidNodeForCalcList);

        }
    }

    private void Spread_CalcRadius(CentroidNodeForCalc centroidNodeForCalc, List<CentroidNodeForCalc> newCentroidNodeForCalcList)
    {
        currentTriangleNode = (TriangleNode)triangleCenterVerticesTree.StartSearch(currentCentroidNodeForCalc.position);
        currentCentroidNodeForCalc.radius = Vector3.Magnitude(currentCentroidNodeForCalc.position - currentTriangleNode.position);
        newCentroidNodeForCalcList.Add(currentCentroidNodeForCalc);
    }





    public void EnsureWithinMinMax()
    {
        for(int i = 0; i < allCentroidNodeForCalc.Count; i++)
        {
            for(int j = 0; j < 3; j++)
            {
                if (allCentroidNodeForCalc[i].position[j] - allCentroidNodeForCalc[i].radius < min_xyz[j])
                {
                    // If bounding sphere crosses min_xyz values, but the centroid itself does not cross the min_xyz values, then shift the centroid by the amount of distance which its bounding sphere crosses the min_xyz values.
                    // If bounding sphere and its centroid crosses min_xyz values, shift the centroid by the amount of distance between the centroid and the min_xyz values, plus the radius, so that the whole bounding sphere does not cross the min_xyz values.
                    // += is to bring the values above the min_xyz values
                    allCentroidNodeForCalc[i].position[j] += (allCentroidNodeForCalc[i].radius - (allCentroidNodeForCalc[i].position[j] - min_xyz[j]));
                }

                if (allCentroidNodeForCalc[i].position[j] + allCentroidNodeForCalc[i].radius > max_xyz[j])
                {
                    // If bounding sphere crosses max_xyz values, but the centroid itself does not cross the max_xyz values, then shift the centroid by the amount of distance which its bounding sphere crosses the max_xyz values.
                    // If bounding sphere and its centroid crosses max_xyz values, shift the centroid by the amount of distance between the centroid and the max_xyz values, plus the radius, so that the whole bounding sphere does not cross the max_xyz values.
                    // -= is to bring the values below the max_xyz values
                    allCentroidNodeForCalc[i].position[j] -= (allCentroidNodeForCalc[i].radius - (max_xyz[j] - allCentroidNodeForCalc[i].position[j]));
                }
            }
            
        }
    }



    public void EnsureAllBoundsInsideMesh()
    {
        int counterLimit;
        for(int i = 0; i < allCentroidNodeForCalc.Count; i++)
        {
            counterLimit = 0; 

            currentTriangleNode = (TriangleNode) triangleCenterVerticesTree.StartSearch(allCentroidNodeForCalc[i].position);
            currentVector3 = allCentroidNodeForCalc[i].position - currentTriangleNode.position;                                     // TAKE NOTE OF THE ORDER OF SUBTRACTION!!!

            while (Vector3.Dot(currentVector3, currentTriangleNode.triangleNormal) > maximumAcceptableCosAngle && counterLimit < 15)
            {
                // If vector from triangle midpoint to centroid is almost parallel to the triangle normal, shift whole bounding sphere of that centroid in the direction opposite of the triangle normal,
                //  with the magnitude of distance shifted being 1.5 the radius of the bounding sphere of this centroid.
                allCentroidNodeForCalc[i].position += 1.5f * allCentroidNodeForCalc[i].radius * (-1) * currentTriangleNode.triangleNormal;

                currentTriangleNode = (TriangleNode)triangleCenterVerticesTree.StartSearch(allCentroidNodeForCalc[i].position);

                currentVector3 = allCentroidNodeForCalc[i].position - currentTriangleNode.position;

                // Set the radius to the distance between centroid and the new nearest triangle face so that any subsequent shifting would not
                //  cause too much (if sphere is at a narrow area of the mesh) or too little shifting (if sphere accidentally shifted too far due to the shift of 2x its old radius). 
                allCentroidNodeForCalc[i].radius = Vector3.Magnitude(currentVector3);

                counterLimit++;
            }



            if(Vector3.Magnitude(currentVector3) < allCentroidNodeForCalc[i].radius)
            {
                // In case the while loop isn't accessed, but the triangle midpoint is within the bounding sphere.
                // It should not be within the bounding sphere of the centroid as the bounding sphere is meant to determine all the areas the particles can move in.
                // So if the triangle midpoint is in the sphere, it means that the bounding sphere is passing the walls of the mesh, which would cause the particles to go out of the mesh.
                // So just check and set in case.

                // Shift centroid such that the bounding sphere is not overshooting mesh walls.
                allCentroidNodeForCalc[i].position -= currentTriangleNode.triangleNormal * -1 * (allCentroidNodeForCalc[i].radius - Vector3.Magnitude(currentVector3));


                // Search latest nearest triangle midpoint.
                currentTriangleNode = (TriangleNode)triangleCenterVerticesTree.StartSearch(allCentroidNodeForCalc[i].position);

                // Calculate distance from midpoint to centroid.
                currentVector3 = allCentroidNodeForCalc[i].position - currentTriangleNode.position;
                
                allCentroidNodeForCalc[i].radius = Vector3.Magnitude(currentVector3);
            }

            // Reduce chances of going out of mesh.
            allCentroidNodeForCalc[i].radius *= 0.8f;
        }
    }



    public void ResetCentroidChangedList(List<bool> centroidChangedBoolList)
    {
        for(int i = 0; i < centroidChangedBoolList.Count; i++)
        {
            centroidChangedBoolList[i] = false;
        }
    }
 
    public void ResetEachCentroidNodeForCalc(List<CentroidNodeForCalc> centroidNodeForCalcList)
    {
        // Reset for next calculation if any.
        for(int i = 0; i < centroidNodeForCalcList.Count; i++)
        {
            centroidNodeForCalcList[i].nearestVertexIndices.Clear();
            centroidNodeForCalcList[i].leftChild = null;
            centroidNodeForCalcList[i].rightChild = null;
        }
        
    }

 
    public void ResetAllReuseVariables()
    {
        currentVector3[0] = 0;
        currentVector3[1] = 0;
        currentVector3[2] = 0;

        currentVector3_temp[0] = 0;
        currentVector3_temp[1] = 0;
        currentVector3_temp[2] = 0;

        

        currentCentroidNodeForCalc = null;
        currentTriangleNode = null;
        nearestNode = null;
        nearestNodeInTempTree = null;


        tempKDTree.SetRootNode(null);
        tempCentroidNodeForCalcList.Clear();
        tempCentroidChanged.Clear();
        tempListOfTriNodesTooFar.Clear();

        // Reference to clear a multidimensional array (not list) : https://stackoverflow.com/questions/19669482/fastest-way-to-zero-out-a-2d-array-in-c-sharp
        Array.Clear(arrayToDistributeEqually, 0, arrayToDistributeEqually.Length);

    }



    


    

    public KDTree ConvertToLowMemoryKDTree()
    {
        KDTree outputKDTree = new();
        CentroidNode currentNode = new();
        CentroidNode oldCentroidNodeForCalc = kDTreeUnoptimised.GetRootNode();
        currentNode.position = oldCentroidNodeForCalc.position;
        currentNode.radius = oldCentroidNodeForCalc.radius;

        outputKDTree.SetRootNode(currentNode);

        Debug.Log($"{oldCentroidNodeForCalc.position}");

        ConvertChildren(ref oldCentroidNodeForCalc, ref outputKDTree);

        return outputKDTree;
    }

    public void ConvertChildren(ref CentroidNode oldCentroidNodeForCalc, ref KDTree outputKDTree)
    {
        
        if(oldCentroidNodeForCalc.rightChild != null)
        {
            outputKDTree.Insert(new CentroidNode {
                position = oldCentroidNodeForCalc.rightChild.position, 
                radius = oldCentroidNodeForCalc.rightChild.radius 
            });

            ConvertChildren(ref oldCentroidNodeForCalc.rightChild, ref outputKDTree);
        }

        if (oldCentroidNodeForCalc.leftChild != null)
        {
            outputKDTree.Insert(new CentroidNode
            {
                position = oldCentroidNodeForCalc.leftChild.position,
                radius = oldCentroidNodeForCalc.leftChild.radius
            });

            ConvertChildren(ref oldCentroidNodeForCalc.leftChild, ref outputKDTree);
        }

        return;
    }










    ///////////////////////////////////////////// TO REMOVE AFTER TESTING OF CLASSES OR PARTICLES IF NEEDED /////////////////////////////////////////////

    public void ShowSpheres()
    {
        GameObject sphere;

        Debug.Log($"allCentroidNodeForCalc.Count is {allCentroidNodeForCalc.Count}");

        for (int i = 0; i < allCentroidNodeForCalc.Count; i++)
        {
            sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            // Primitive Sphere Mesh has 0.5 radius for unit sphere. So must scale by 2x of radius from centroid. Reference to sphere details: https://docs.unity3d.com/510/Documentation/Manual/PrimitiveObjects.html
            sphere.transform.localScale = Vector3.one * allCentroidNodeForCalc[i].radius * 2;
            sphere.transform.position = allCentroidNodeForCalc[i].position;
            allSpheres.Add( sphere );
        }
    }

    public void DestroySpheres()
    {
        for (int i = 0; i < allSpheres.Count; i++)
        {
            Destroy(allSpheres[i]);
        }
    }

    ///////////////////////////////////////////// TO REMOVE AFTER TESTING OF CLASSES OR PARTICLES IF NEEDED /////////////////////////////////////////////

}


