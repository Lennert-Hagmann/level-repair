using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerFall : MonoBehaviour
{
    
        public float fallThreshold = -1f; // Höhe, unter der der Spieler als "gefallen" gilt
        public GameObject gameOverUI; // Referenz auf das UI-Overlay
    private bool stopped = false;

        public Vector3 initialPosition = new Vector3(0,10,0); // Startposition des Spielers
    private GameObject player;
    public GameObject cube;
    public GameObject WorldGeneration;


    public int counter = 0;
    void Start()
        {
        player = transform.parent.gameObject;
        //initialPosition = transform.position; // Speichert die Startposition des Spielers
        Time.timeScale = 1f;
        stopped = false;
             gameOverUI.SetActive(false); // Stellt sicher, dass das UI zu Beginn deaktiviert ist
        //Invoke("RestorePlayer", 5f);
        //Invoke("RestorePlayer", 10f);
    }

        void Update()
        {
            if (!stopped && player.transform.position.y < fallThreshold)
            {
                stopped = true; 
            //player.transform.position=new Vector3(10,10,10);

            ActivateGameOverUI();
            //player.transform.position = initialPosition;
        }
        
    }
    

    void ActivateGameOverUI()
        {
            Time.timeScale = 0f; // Stoppt das Spiel
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Debug.LogWarning("PLAYER FALL; Start position: " + initialPosition.ToString());
        gameOverUI.SetActive(true); // Aktiviert das UI
        }

        public void RestorePlayer()
        {
        
        Debug.LogWarning("Restore Player; Start position: " + initialPosition.ToString());
        //cube.transform.position = initialPosition; // Setzt den Spieler an die Startposition zurück
        //Transform a = new Transform(initialPosition,);
        Debug.LogWarning("SpielerPosition VORHER: "+player.transform.position);
        player.SetActive(false);
        player.transform.position = new Vector3(1,4,1);
        player.SetActive(true );
        stopped = false;
        Debug.LogWarning("SpielerPosition nachHER: " + player.transform.position);
        //WorldGeneration.GetComponent<WorldGeneration>().ReloadPlayerPos();
        Time.timeScale = 1f; // Setzt das Spiel fort
        Cursor.visible = false;
        gameOverUI.SetActive(false); // Deaktiviert das UI
        }

        public void ReloadScene()
        {
            Time.timeScale = 1f; // Setzt die Zeit zurück, falls pausiert
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Lädt die aktuelle Szene neu
        stopped = false;
        Time.timeScale = 1f; // Setzt die Zeit zurück, falls pausiert
    }

    public void Menu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    
}
