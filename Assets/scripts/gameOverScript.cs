using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class gameOverScript : MonoBehaviour
{

    public GameObject player;
    public Transform startingPosition;
    public void GameOver()
    {
        gameObject.SetActive(true);
    }

    public void RestartButton()
    {
        print("button Works");
        /*SceneManager.LoadScene("[OLD]sample22");
        Debug.Log("Restart");
        player.transform.position = startingPosition.position;
        gameObject.SetActive(false);*/
    }
}
