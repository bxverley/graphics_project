using UnityEngine;
using System.Collections;

public class ObjectRotate : MonoBehaviour
{

    private float sensitivity;
    private Vector3 mouseRef;
    private Vector3 mouseOffset;
    private Vector3 rotation;
    private bool isRotating;

    void Start ()
    {
        sensitivity = .5f;
        rotation = Vector3.zero;

    }

    void Update()
    {
        if(isRotating)
        {
            mouseOffset = (Input.mousePosition - mouseRef);

            rotation.y = -(mouseOffset.x) * sensitivity;

            rotation.x = -(mouseOffset.y) * sensitivity;

            transform.eulerAngles += rotation;

            mouseRef = Input.mousePosition;
        }
    }

    void onMouseDown()
    {
        // set flag to true 
        isRotating = true;

        mouseRef - Input.mousePosition;
    }

    void onMouseUp()
    {
        isRotating = false;
    }

}