using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ObjectMovement : MonoBehaviour
{
    /*
    private Vector3 _PrevPos;
    private Vector3 _PosDelta;
    */

    private Vector3 translationMousePrevPos, translationMouseCurrentPos, translationMousePosChangeVector;
    private Vector3 rotationMousePrevPos, rotationMouseCurrentPos, rotationMousePosChangeVector;
    private Quaternion rotationAnglesCombined;

    private bool leftMouseClickStarted = false;
    private bool rightMouseClickStarted = false;


    private Matrix4x4 translationMatrix;

    private float rotationScaleFactor = 30;
    private Matrix4x4 rotationMatrix_MeshLocalSpace, rotationMatrix_AroundOrigin, translateToOriginMatrix, translateToPositionMatrix;
    
    public Matrix4x4 combinedTransformMatrix;
    public Matrix4x4 combinedInverseTransformMatrix;
    public Matrix4x4 combinedTransformMatrix_NoTranslation;
    public Matrix4x4 combinedInverseTransformMatrix_NoTranslation;

    private SnowGlobe snowGlobe;
    public GlobeParticleSystem globeParticleSystem;
    private Vector3[] platformTransformedVertices, platformOriginalTransformedVertices;

    private List<Vector3> snowGlobeVertices;
    private Vector3[] snowGlobeTransformedVertices, snowGlobePrevTransformedVertices;

    private Camera mainCamera;
    private float cameraZDistFromGlobe;


    void Start ()
    {
        /*
        Vector3 _PrevPos = Vector3.zero;
        Vector3 _PosDelta = Vector3.zero;
        */


        mainCamera = Camera.main;

        translationMousePrevPos = Vector3.zero;
        translationMouseCurrentPos = Vector3.zero;
        translationMousePosChangeVector = Vector3.zero;

        rotationMousePrevPos = Vector3.zero;
        rotationMouseCurrentPos = Vector3.zero;
        rotationMousePosChangeVector = Vector3.zero;

        snowGlobe = gameObject.GetComponent<SnowGlobe>();

        snowGlobeVertices = snowGlobe.vertices;
        snowGlobeTransformedVertices = new Vector3[snowGlobeVertices.Count];
        snowGlobePrevTransformedVertices = new Vector3[snowGlobeVertices.Count];

        for (int i = 0; i < snowGlobeVertices.Count; i++)
        { 
            // Sets the initial vertices position.
            snowGlobePrevTransformedVertices[i] = snowGlobeVertices[i];
        }

        platformOriginalTransformedVertices = snowGlobe.platformMesh.vertices;
        platformTransformedVertices = new Vector3[platformOriginalTransformedVertices.Length];



        // Reference to get Matrix4x4 identity matrix: https://docs.unity3d.com/ScriptReference/Matrix4x4-identity.html
        combinedTransformMatrix = Matrix4x4.identity;
        combinedInverseTransformMatrix = Matrix4x4.identity;

    }

    public void RetrieveGlobeParticleSystem()
    {
        globeParticleSystem = snowGlobe.globeParticleSystem;
    }

    void Update()
    {
        /*
        if(Input.GetMouseButton(0))
        {
            _PosDelta = Input.mousePosition - _PrevPos;
            if(Vector3.Dot(transform.up, Vector3.up) >= 0)
            {
                transform.Rotate(transform.up, Vector3.Dot(_PosDelta, Camera.main.transform.right), Space.World);
            }
            else
            {
                transform.Rotate(transform.up, Vector3.Dot(_PosDelta,Camera.main.transform.right), Space.World);
            }
            transform.Rotate(Camera.main.transform.right, Vector3.Dot(_PosDelta, Camera.main.transform.up), Space.World);
        }
        _PrevPos = Input.mousePosition;
        */



        cameraZDistFromGlobe = mainCamera.WorldToScreenPoint(transform.position).z;                 // Used in the z coordinate for the 3D mouse position on screen.


        // Reference to the reason why there is a need to make a new array instead of directly changing the old list: https://answers.unity.com/questions/1580870/position-of-vertices-not-changing.
        if (leftMouseClickStarted)
        {
            SetTranslationCurrentMouseScreenToWorld();
            translationMousePosChangeVector = translationMouseCurrentPos - translationMousePrevPos;
            translationMousePrevPos = translationMouseCurrentPos;

            if (Vector3.Magnitude(translationMousePosChangeVector) > 0)
            {
                // Reference to create Matrix4x4 object for translation matrix: https://docs.unity3d.com/ScriptReference/Matrix4x4.Translate.html
                translationMatrix = Matrix4x4.Translate(translationMousePosChangeVector);

                DotProdToCombinedTransformMatrix(translationMatrix);
                DotProdToInverseCombinedMatrix(Matrix4x4.Inverse(translationMatrix));

                SetMatricesWithoutTranslation();



                if (globeParticleSystem == null)
                {
                    RetrieveGlobeParticleSystem();
                }
                globeParticleSystem.CheckAllParticlesWhenMovement();



                // Performing transformation on previous transformed vertices, NOT THE ORIGINAL VERTICES WHEN THE MESH IS LOADED.
                for (int i = 0; i < snowGlobeVertices.Count; i++)
                {
                    snowGlobeTransformedVertices[i] = translationMatrix.MultiplyPoint3x4(snowGlobePrevTransformedVertices[i]);
                }

                // Reference for copying over array: https://stackoverflow.com/questions/733243/how-to-copy-part-of-an-array-to-another-array-in-c
                // Storing the latest transformed vertices to snowGlobePrevTransformedVertices.
                Array.Copy(snowGlobeTransformedVertices, snowGlobePrevTransformedVertices, snowGlobeVertices.Count);
 
                snowGlobe.UpdateGlobeVertices(snowGlobeTransformedVertices);

            }
                

        }


        if (Input.GetMouseButtonDown(0))
        {
            if (!leftMouseClickStarted)
            {
                leftMouseClickStarted = true;

                SetTranslationPrevMouseScreenToWorld();

            }
        }

        else if (Input.GetMouseButtonUp(0))
        {
            leftMouseClickStarted = false;
        }




        if (rightMouseClickStarted)
        {
            SetRotationCurrentMouseScreenToWorld();
            rotationMousePosChangeVector = rotationScaleFactor * (rotationMouseCurrentPos - rotationMousePrevPos);
            rotationMousePrevPos = rotationMouseCurrentPos;

            if (Vector3.Magnitude(rotationMousePosChangeVector) > 0)
            {


                // Reference to create Matrix4x4 object for rotation matrix: https://docs.unity3d.com/ScriptReference/Matrix4x4.Rotate.html
                // Combines rotation of a value of rotationMousePosChangeVector.y around world x-axis, a value of rotationMousePosChangeVector.x around world y-axis,
                //   and a value of rotationMousePosChangeVector.z around the world z-axis.
                // Used y value for rotation around x-axis as shifting mouse up and down is meant to rotate the object up and down. So rotate around x-axis.
                // Same concept for using x value for rotation around y-axis. Multiplied by -1 as without that, object rotates in opposite direction as mouse movement.
                rotationAnglesCombined = Quaternion.Euler(rotationMousePosChangeVector.y, -1.0f * rotationMousePosChangeVector.x, rotationMousePosChangeVector.z);
                rotationMatrix_AroundOrigin = Matrix4x4.Rotate(rotationAnglesCombined);
                SetRotationMatrix_MeshLocalSpace(ref rotationMatrix_MeshLocalSpace, rotationMatrix_AroundOrigin, translateToOriginMatrix, translateToPositionMatrix);
                DotProdToCombinedTransformMatrix(rotationMatrix_MeshLocalSpace);
                DotProdToInverseCombinedMatrix(Matrix4x4.Inverse(rotationMatrix_MeshLocalSpace));

                SetMatricesWithoutTranslation();


                /*
                // Get the inverse for the rotation matrix of Euler Angles.
                rotationAnglesCombined = Quaternion.Euler(- rotationMousePosChangeVector.y, rotationMousePosChangeVector.x, - rotationMousePosChangeVector.z);
                rotationMatrix_AroundOrigin = Matrix4x4.Rotate(rotationAnglesCombined);
                SetRotationMatrix_MeshLocalSpace(ref rotationMatrix_MeshLocalSpace, rotationMatrix_AroundOrigin, translateToOriginMatrix, translateToPositionMatrix);
                DotProdToInverseCombinedMatrix(rotationMatrix_MeshLocalSpace);
                */


                if (globeParticleSystem == null)
                {
                    RetrieveGlobeParticleSystem();
                }

                globeParticleSystem.CheckAllParticlesWhenMovement();



                // Performing transformation on previous transformed vertices, NOT THE ORIGINAL VERTICES WHEN THE MESH IS LOADED.
                for (int i = 0; i < snowGlobeVertices.Count; i++)
                {
                    snowGlobeTransformedVertices[i] = rotationMatrix_MeshLocalSpace.MultiplyPoint3x4(snowGlobePrevTransformedVertices[i]);
                }

                // Reference for copying over array: https://stackoverflow.com/questions/733243/how-to-copy-part-of-an-array-to-another-array-in-c
                // Storing the latest transformed vertices to snowGlobePrevTransformedVertices.
                Array.Copy(snowGlobeTransformedVertices, snowGlobePrevTransformedVertices, snowGlobeVertices.Count);

                snowGlobe.UpdateGlobeVertices(snowGlobeTransformedVertices);


            }


        }


        if (Input.GetMouseButtonDown(1))
        {
            if (!rightMouseClickStarted)
            {
                rightMouseClickStarted = true;
                SetRotationPrevMouseScreenToWorld();
            }
        }

        else if (Input.GetMouseButtonUp(1))
        {
            rightMouseClickStarted = false;
        }


        if(rightMouseClickStarted || leftMouseClickStarted)
        {
            for (int i = 0; i < platformOriginalTransformedVertices.Length; i++)
            {
                // Transform platform in snow globe local space to snow globe world space.
                platformTransformedVertices[i] = combinedTransformMatrix.MultiplyPoint3x4(platformOriginalTransformedVertices[i]);
            }
            snowGlobe.UpdatePlatformVertices(platformTransformedVertices);
        }

    }

    void SetMatricesWithoutTranslation()
    {
        combinedTransformMatrix_NoTranslation = Matrix4x4.identity;
        combinedTransformMatrix_NoTranslation.SetColumn(0, combinedTransformMatrix.GetColumn(0));
        combinedTransformMatrix_NoTranslation.SetColumn(1, combinedTransformMatrix.GetColumn(1));
        combinedTransformMatrix_NoTranslation.SetColumn(2, combinedTransformMatrix.GetColumn(2));

        combinedInverseTransformMatrix_NoTranslation = Matrix4x4.identity;
        combinedInverseTransformMatrix_NoTranslation.SetColumn(0, combinedInverseTransformMatrix.GetColumn(0));
        combinedInverseTransformMatrix_NoTranslation.SetColumn(1, combinedInverseTransformMatrix.GetColumn(1));
        combinedInverseTransformMatrix_NoTranslation.SetColumn(2, combinedInverseTransformMatrix.GetColumn(2));
    }


    void DotProdToCombinedTransformMatrix(Matrix4x4 transformMatrix)
    {
        combinedTransformMatrix = transformMatrix * combinedTransformMatrix;
    }

    void DotProdToInverseCombinedMatrix(Matrix4x4 inversedTransformMatrix)
    {
        combinedInverseTransformMatrix = combinedInverseTransformMatrix * inversedTransformMatrix;
    }

    void SetRotationMatrix_MeshLocalSpace(ref Matrix4x4 rotationMatrix_MeshLocalSpace, Matrix4x4 rotationMatrix_AroundOrigin, Matrix4x4 translateToOriginMatrix, Matrix4x4 translateToPositionMatrix)
    {

        // Reference to access Matrix4x4 elements: https://forum.unity.com/threads/matrix-index-access-0-_m00_m01_m02_m03-alternative-_m00_m10_m20_m30.390347/
        translateToOriginMatrix = Matrix4x4.identity;
        translateToOriginMatrix[0, 3] = -1 * combinedTransformMatrix[0, 3];
        translateToOriginMatrix[1, 3] = -1 * combinedTransformMatrix[1, 3];
        translateToOriginMatrix[2, 3] = -1 * combinedTransformMatrix[2, 3];

        translateToPositionMatrix = Matrix4x4.identity;
        translateToPositionMatrix[0, 3] = combinedTransformMatrix[0, 3];
        translateToPositionMatrix[1, 3] = combinedTransformMatrix[1, 3];
        translateToPositionMatrix[2, 3] = combinedTransformMatrix[2, 3];
        // translateToPositionMatrix.SetColumn(3, combinedTransformMatrix.GetColumn(3));

        rotationMatrix_MeshLocalSpace = translateToPositionMatrix * rotationMatrix_AroundOrigin * translateToOriginMatrix;
    }


    // Reference of getting mouse position in 3D World Space from mouse position on screen: https://www.youtube.com/watch?v=bK5kYjpqco0
    void SetTranslationPrevMouseScreenToWorld()
    {
        // Set 3D mouse position on SCREEN
        translationMousePrevPos[0] = Input.mousePosition.x;
        translationMousePrevPos[1] = Input.mousePosition.y;
        translationMousePrevPos[2] = cameraZDistFromGlobe;

        // Convert 3D mouse position to actual 3D WORLD SPACE.
        translationMousePrevPos = mainCamera.ScreenToWorldPoint(translationMousePrevPos);
    }

    void SetTranslationCurrentMouseScreenToWorld()
    { 
        // Set 3D mouse position on SCREEN
        translationMouseCurrentPos[0] = Input.mousePosition.x;
        translationMouseCurrentPos[1] = Input.mousePosition.y;
        translationMouseCurrentPos[2] = cameraZDistFromGlobe;

        // Convert 3D mouse position to actual 3D WORLD SPACE.
        translationMouseCurrentPos = mainCamera.ScreenToWorldPoint(translationMouseCurrentPos);
    }

    void SetRotationPrevMouseScreenToWorld()
    {
        // Set 3D mouse position on SCREEN
        rotationMousePrevPos[0] = Input.mousePosition.x;
        rotationMousePrevPos[1] = Input.mousePosition.y;
        rotationMousePrevPos[2] = cameraZDistFromGlobe;

        // Convert 3D mouse position to actual 3D WORLD SPACE.
        rotationMousePrevPos = mainCamera.ScreenToWorldPoint(rotationMousePrevPos);
    }

    void SetRotationCurrentMouseScreenToWorld()
    {
        // Set 3D mouse position on SCREEN
        rotationMouseCurrentPos[0] = Input.mousePosition.x;
        rotationMouseCurrentPos[1] = Input.mousePosition.y;
        rotationMouseCurrentPos[2] = cameraZDistFromGlobe;

        // Convert 3D mouse position to actual 3D WORLD SPACE.
        rotationMouseCurrentPos = mainCamera.ScreenToWorldPoint(rotationMouseCurrentPos);
    }

}