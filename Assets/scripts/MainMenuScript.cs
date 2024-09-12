using System.Collections;
using System.Collections.Generic;
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
