using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlTextColors : MonoBehaviour
{
    TextMeshProUGUI LeftClick;
    TextMeshProUGUI RightClick;
    TextMeshProUGUI A;
    TextMeshProUGUI D;
    TextMeshProUGUI W;
    TextMeshProUGUI S;
    TextMeshProUGUI Two;
    TextMeshProUGUI X;
    TextMeshProUGUI C;
    TextMeshProUGUI Shift_C;
    Color fullOpacity;
    Color lessOpacity;
    float opacityValue = 0.2f;


    // Start is called before the first frame update
    void Start()
    {
        // Reference to get text: https://answers.unity.com/questions/1216893/how-do-i-reference-to-a-text-objects-text-in-scrip.html
        // Reference to get child of parent: https://answers.unity.com/questions/924637/access-text-inside-canvas-by-name.html

        // Reference to get TextMeshProUGUI component and not Text component: https://forum.unity.com/threads/changing-color-of-textmeshpro-in-code.949434/
        LeftClick = transform.Find("LeftClick").GetComponent<TextMeshProUGUI>();
        RightClick = transform.Find("RightClick").GetComponent<TextMeshProUGUI>();

        A = transform.Find("A").GetComponent<TextMeshProUGUI>(); 
        D = transform.Find("D").GetComponent<TextMeshProUGUI>(); 
        W = transform.Find("W").GetComponent<TextMeshProUGUI>();
        S = transform.Find("S").GetComponent<TextMeshProUGUI>();
        Two = transform.Find("2").GetComponent<TextMeshProUGUI>();
        X = transform.Find("X").GetComponent<TextMeshProUGUI>();

        C = transform.Find("C").GetComponent<TextMeshProUGUI>();
        Shift_C = transform.Find("Shift_C").GetComponent<TextMeshProUGUI>();

        fullOpacity = Color.white;
        lessOpacity = Color.white;
        lessOpacity.a = opacityValue;

        ChangeBackTextOpacity(LeftClick);
        ChangeBackTextOpacity(RightClick);
        ChangeBackTextOpacity(W);
        ChangeBackTextOpacity(S);
        ChangeBackTextOpacity(A);
        ChangeBackTextOpacity(D);
        ChangeBackTextOpacity(Two);
        ChangeBackTextOpacity(X);
        ChangeBackTextOpacity(C);
        ChangeBackTextOpacity(Shift_C);


    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetMouseButton(0))
        {
            ChangeToFullTextOpacity(LeftClick);
        }
        else
        {
            ChangeBackTextOpacity(LeftClick);
        }

        if (Input.GetMouseButton(1))
        {
            ChangeToFullTextOpacity(RightClick);
        }
        else
        {
            ChangeBackTextOpacity(RightClick);
        }

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            ChangeToFullTextOpacity(W);
        }
        else
        {
            ChangeBackTextOpacity(W);
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            ChangeToFullTextOpacity(S);
        }
        else
        {
            ChangeBackTextOpacity(S);
        }

        

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            ChangeToFullTextOpacity(A);
        }
        else
        {
            ChangeBackTextOpacity(A);
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            ChangeToFullTextOpacity(D);
        }
        else
        {
            ChangeBackTextOpacity(D);
        }



        if (Input.GetKey(KeyCode.Alpha2))
        {
            ChangeToFullTextOpacity(Two);
        }
        else
        {
            ChangeBackTextOpacity(Two);
        }

        if (Input.GetKey(KeyCode.X))
        {
            ChangeToFullTextOpacity(X);
        }
        else
        {
            ChangeBackTextOpacity(X);
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.C))
        {
            ChangeToFullTextOpacity(Shift_C);
        }
        else if(Input.GetKey(KeyCode.C))
        {
            ChangeToFullTextOpacity(C);
        }
        else
        {
            ChangeBackTextOpacity(C);
            ChangeBackTextOpacity(Shift_C);
        }


    }

    void ChangeToFullTextOpacity(TextMeshProUGUI text)
    {
        text.color = fullOpacity;
    }
    
    void ChangeBackTextOpacity(TextMeshProUGUI text)
    {
        text.color = lessOpacity;
    }
}
