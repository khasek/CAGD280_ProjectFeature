/*******************************************************************************
 * Author: Kendal Hasek
 * Date: 05/09/2024
 * Description: GameManager.cs holds the world seed and manages scene transitions
*******************************************************************************/

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public static string worldSeed;
    public string customSeed;
    public static string randomSeed;
    public bool useCustomSeed = false;
    public bool useRandomSeed = false;


    // Unity functions ---------------------------------------------------------

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
            Destroy(this.gameObject);
    }

    // -------------------------------------------------------------------------

    void Start()
    {
        if (worldSeed == null)
        {
            RandomSeed();
            worldSeed = randomSeed;
        }
    }


    // Custom functions --------------------------------------------------------

    /// <summary>
    /// Uses System time to generate a random 8-digit seed
    /// </summary>
    public void RandomSeed()
    {
        System.DateTime t = System.DateTime.UtcNow;
        randomSeed = t.Minute.ToString() + t.Hour.ToString() + 
                     t.Second.ToString() + t.Millisecond.ToString();

        useCustomSeed = false;
        useRandomSeed = true;
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Finalizes world seed and loads the game world scene
    /// </summary>
    public void LoadWorldScene()
    {
        // Confirm world seed
        if (useCustomSeed)
            worldSeed = customSeed;
        else if (useRandomSeed)
            worldSeed = randomSeed;
        else
        {
            RandomSeed();
            worldSeed = randomSeed;
        }

        // Load game scene
        SceneManager.LoadScene(sceneName: "GameWorld");
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns to the main menu and resets the world seed
    /// </summary>
    public void LeaveWorld()
    {
        SceneManager.LoadScene(sceneName: "StartMenu");
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Exits the application
    /// </summary>
    public void ExitGame()
    {
        Application.Quit();
    }
}
