using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public GameObject SpawnPosition;
    public GameObject background;
    public GameObject RestartButton;
    public GameObject player;
    private bool alive = true;
    // Start is called before the first frame update
    void Start()
    {
        //background.SetActive(false);
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (alive)
        {
            if (player.transform.position.y < 0)
            {
                alive = false;
                //Restart
                //Stop Scene
                Cursor.visible = true;
                Debug.LogWarning("RESTART");
                Time.timeScale = 0;
                RestartButton.SetActive(true);
                background.SetActive(true);
                //ReloadScene();

            }
            else
            {
                Time.timeScale = 1;
            }
        }
    }

    public void ReloadScene()
    {
        Debug.Log("RELOAD");
        Time.timeScale = 1;
        SceneManager.LoadScene("sampleScene"); //SpawnPosition des Spielers wird nicht übernommen, er wird da gespawnt, wo er in der Scene platziert ist
                                               

    }
}
