/*******************************************************************************
 * Author: Kendal Hasek
 * Date: 05/09/2024
 * Description: StartUI.cs manages UI actions for the main menu scene
*******************************************************************************/

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField customSeed;
    [SerializeField] private TMP_InputField randomSeed;


    // Unity functions ---------------------------------------------------------

    private void Start()
    {
        SetRandomSeed();
    }


    // Custom functions --------------------------------------------------------

    /// <summary>
    /// Adjusts the custom seed input length then updates the GameManager
    /// </summary>
    public void SetCustomSeed()
    {
        // Adjust seed string to make sure it is neither too long nor too short
        while (customSeed.text.Length < 7)
            customSeed.text = "1" + customSeed.text;

        if (customSeed.text.Length > 9)
            customSeed.text = customSeed.text[..9];

        // Update the GameManager
        GameManager.Instance.customSeed = customSeed.text;
        GameManager.Instance.useCustomSeed = true;
        GameManager.Instance.useRandomSeed = false;
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Generate a new random seed value and update the input field text
    /// </summary>
    public void SetRandomSeed()
    {
        GameManager.Instance.RandomSeed();
        randomSeed.text = GameManager.randomSeed;
    }
}
