using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerScript : MonoBehaviour
{

    public GameObject player;

    void Update()
    {
        if (player.transform.position.y < 0)
        {
            Debug.Log("HH");
        }
    }
}
