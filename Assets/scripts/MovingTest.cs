using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingTest : MonoBehaviour
{

    public GameObject ob;
    // Start is called before the first frame update
    void Start()
    {
        ob.transform.position = new Vector3(-10,10,10);
    }

    // Update is called once per frame
    void Update()
    {
        ob.transform.position = new Vector3(-10, 10, 10);
    }
}
