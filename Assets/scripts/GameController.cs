using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public GameObject SpawnPosition;
    public GameObject background;
    public GameObject player;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(player.transform.position.y < 0)
        {
            //Restart
            //Stop Scene
            Time.timeScale = 0;
            Cursor.visible = true;
            background.SetActive(true);
            ReloadScene();

        }
        else
        {
            Time.timeScale = 1;
        }
    }

    public void ReloadScene()
    {
        Debug.Log("RELOAD");
        Time.timeScale = 1;
        SceneManager.LoadScene("sampleScene"); //SpawnPosition des Spielers wird nicht übernommen, er wird da gespawnt, wo er in der Scene platziert ist
                                               

    }
}
