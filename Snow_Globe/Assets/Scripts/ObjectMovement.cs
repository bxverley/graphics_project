using UnityEngine;
using System.Collections;

public class ObjectMovement : MonoBehaviour
{

    private Vector3 _PrevPos;
    private Vector3 _PosDelta;

    void Start ()
    {

        Vector3 _PrevPos = Vector3.zero;
        Vector3 _PosDelta = Vector3.zero;

    }

    void Update()
    {
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
    }

}