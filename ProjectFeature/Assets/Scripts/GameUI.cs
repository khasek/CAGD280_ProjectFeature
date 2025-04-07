/*******************************************************************************
 * Author: Kendal Hasek
 * Date: 05/09/2024
 * Description: GameUI.cs manages UI actions for the game world scene
*******************************************************************************/

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [SerializeField] private GameObject menu;
    [SerializeField] private TextMeshProUGUI seed;

    [SerializeField] private TextMeshProUGUI coordinates;
    private Vector3 playerCoords;
    private string strCoords;


    // Unity functions ---------------------------------------------------------

    private void Start()
    {
        seed.text = GameManager.worldSeed;
    }

    // -------------------------------------------------------------------------

    void Update()
    {
        DisplayCoordinates();
    }


    // Custom functions --------------------------------------------------------

    /// <summary>
    /// Update the displayed coordinates to match the player's current coordinates
    /// </summary>
    private void DisplayCoordinates()
    {
        playerCoords = Player.Instance.gameObject.transform.position;
        strCoords = "Position: " + Mathf.RoundToInt(playerCoords.x).ToString() +
                         ", " + Mathf.FloorToInt(playerCoords.y).ToString() +
                         ", " + Mathf.RoundToInt(playerCoords.z).ToString();

        coordinates.text = strCoords;
    }

    // -------------------------------------------------------------------------

    /// <summary>
    /// Toggle the game menu
    /// </summary>
    public void ToggleMenu(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            menu.SetActive(!menu.activeSelf);
        }
    }
}
