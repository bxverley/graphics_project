using System;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    float speed;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(0, 0, -5);
        speed = 10f;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            if(Vector3.Magnitude(transform.position - Vector3.zero) > 0.5f){
                transform.Translate(Vector3.forward * Time.deltaTime * speed * 1);
            }
        }

        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            transform.Translate(Vector3.back * Time.deltaTime * speed * 1);
        }

        
        else if (Input.GetKey(KeyCode.Alpha2))
        {
            transform.Translate(Vector3.up * Time.deltaTime * speed * 0.5f);               // Ascend upwards.
        }


        else if (Input.GetKey(KeyCode.X))
        {
            transform.Translate(Vector3.down * Time.deltaTime * speed * 0.5f);              // Descend downwards.
        }
        
    }
}
