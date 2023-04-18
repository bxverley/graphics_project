using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraParentRotation : MonoBehaviour
{
    public float cameraParentRotationSpeed = 100;

    
    // Update is called once per frame
    void Update()
    {

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(Vector3.up, cameraParentRotationSpeed * Time.deltaTime);
        }

        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(Vector3.up, cameraParentRotationSpeed * Time.deltaTime * -1);
        }



    }
}
