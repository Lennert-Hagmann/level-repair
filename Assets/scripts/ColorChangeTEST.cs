using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChangeTEST : MonoBehaviour
{
    Renderer ren;
    // Start is called before the first frame update
    void Start()
    {
        Transform childTransform = transform.GetChild(0);
        GameObject childGameObject = childTransform.gameObject;
        ren = childGameObject.GetComponent<Renderer>();
        Debug.Log(ren.ToString());
        ren.material.SetColor("_Color",Color.white);
    }

    
}
