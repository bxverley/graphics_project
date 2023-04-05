using System;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    float speed, rotationSpeed;
    bool toRotate, toFly;


    ///////////////////////////////////////////// TO REMOVE AFTER USING FOR PARTICLES / TESTING OF CLASSES /////////////////////////////////////////////
    SnowGlobe snowGlobe;
    ObjectMovement objectMovementScript;
    Matrix4x4 combinedInverseMatrix;


    CollisionChecker collisionChecker = new();
    KDTree meshCollisionKDTree;
    ///////////////////////////////////////////// TO REMOVE AFTER USING FOR PARTICLES / TESTING OF CLASSES /////////////////////////////////////////////





    // Start is called before the first frame update
    void Start()
    {
        speed = 10f;
        rotationSpeed = 50;
        toRotate = false;
        toFly = false;


        ///////////////////////////////////////////// TO REMOVE AFTER USING FOR PARTICLES / TESTING OF CLASSES /////////////////////////////////////////////
        snowGlobe = GameObject.Find("SnowGlobe").GetComponent<SnowGlobe>();
        ///////////////////////////////////////////// TO REMOVE AFTER USING FOR PARTICLES / TESTING OF CLASSES /////////////////////////////////////////////


    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            if (toFly)
            {
                transform.Translate(Vector3.up * Time.deltaTime * speed * 1);               // Ascend upwards.
            }
            else
            {
                transform.Translate(Vector3.forward * Time.deltaTime * speed * 1);
            }
            
        }

        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            if (toFly)
            {
                transform.Translate(Vector3.down * Time.deltaTime * speed * 1);              // Descend downwards.
            }
            else
            {
                transform.Translate(Vector3.back * Time.deltaTime * speed * 1);
            }
        }

        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            if (toRotate)
            {
                transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed * 1);          // When using Rotate(), Vector3.up is for the axis.
            }
            else
            {
                transform.Translate(Vector3.right * Time.deltaTime * speed * 1);   
            }
            
        }

        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            if (toRotate)
            {
                transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed * -1);         // When using Rotate(), Vector3.up is for the axis.
            }
            else
            {
                transform.Translate(Vector3.left * Time.deltaTime * speed * 1);    
            }
        }

        if (Input.GetKeyDown(KeyCode.R)){
            toRotate = !toRotate;                                                           // Set the opposite bool of the current bool value.
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            toFly = !toFly;                                                                 // Set the opposite bool of the current bool value.
        }





        ///////////////////////////////////////////// TO REMOVE AFTER USING FOR PARTICLES / TESTING OF CLASSES /////////////////////////////////////////////
        
        if (snowGlobe.meshCollisionKDTree != null)
        {
            meshCollisionKDTree = snowGlobe.meshCollisionKDTree;
        }

        if (snowGlobe.objectMovementScript.combinedInverseTransformMatrix != null)
        {
            combinedInverseMatrix = snowGlobe.objectMovementScript.combinedInverseTransformMatrix;
        }

        if (collisionChecker.CheckWallCollision(combinedInverseMatrix * new Vector4(transform.position.x, transform.position.y, transform.position.z, 1), meshCollisionKDTree))
        {
            Debug.Log("COLLIDED!");
        }
        else
        {
            Debug.Log("No collision");
        }

        
        ///////////////////////////////////////////// TO REMOVE AFTER USING FOR PARTICLES / TESTING OF CLASSES /////////////////////////////////////////////
    }
}
