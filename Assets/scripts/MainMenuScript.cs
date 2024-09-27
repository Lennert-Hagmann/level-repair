using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuScript : MonoBehaviour
{
    // Start is called before the first frame update

    private int diff; //0 easy, 1 medium, 2 hard
    public Dropdown difficultyDropdown;
    public Toggle erweiteterAgentToggle;
    public Toggle ColorToggle;
    void Start()
    {
        
    }

    public void ToggleClickErweitert()
    {
        WorldGeneration.erweitert = erweiteterAgentToggle.isOn;
        Debug.Log("erweiterter Agent ist: " + WorldGeneration.erweitert.ToString());
    }

    public void ToggleClickColor()
    {
        AgentScript.ColorChange = ColorToggle.isOn;
        Debug.Log("Color Change ist: " + AgentScript.ColorChange.ToString());
    }

    public void StartGame()
    {
        difficulty.Difficulty = diff;


        // Replace "GameScene" with the name of your game scene
        SceneManager.LoadScene("SampleScene");
    }
    public string mainScene = "MainMenu";
    public string loopScene = "SampleScene";
    public int loopCount = 3;
    public void testGame()
    {
        StartCoroutine(LoadLoopScene());
        /*difficulty.Difficulty = diff;
        Debug.LogWarning("HAHAHA");
        int[] array = new int[10];
        for (int i = 0; i < 10; i++)
        {
            SceneManager.LoadScene("SampleScene");
            array[i] = (int)WorldGeneration.stopwatch.ElapsedMilliseconds;
        }
        for(int i=0; i < array.Length; i++) { Debug.LogWarning(array[i]); }

        // Replace "GameScene" with the name of your game scene
        */

    }
    private IEnumerator LoadLoopScene()
    {
        // Gehe in die andere Szene und lade sie mehrfach
        for (int i = 0; i < loopCount; i++)
        {
            yield return SceneManager.LoadSceneAsync(loopScene);
            Debug.Log("Szene " + loopScene + " geladen, Durchlauf: " + (i + 1));
            // Hier kannst du deine Logik einfügen, die nach dem Laden jeder Szene ausgeführt wird
        }

        // Kehre zur ursprünglichen Szene zurück
        yield return SceneManager.LoadSceneAsync(mainScene);
        Debug.Log("Zurück zur Hauptszene: " + mainScene);
    }

    public void SetDifficultyFromDropdown()
    {
        int selectedIndex = difficultyDropdown.value;
        diff = selectedIndex;
        difficulty.Difficulty = diff;
        Debug.LogWarning("Geändert zu "+diff.ToString());
        Debug.LogWarning(difficulty.Difficulty.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
