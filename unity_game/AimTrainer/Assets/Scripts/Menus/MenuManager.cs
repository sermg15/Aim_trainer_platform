using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject loginMenu;
    [SerializeField] private GameObject registerMenu;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private TMP_Dropdown targetsDropdown;

    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        loginMenu.SetActive(false);
        registerMenu.SetActive(false);
        settingsMenu.SetActive(false);
    }

    public void ShowLoginMenu()
    {
        mainMenu.SetActive(false);
        loginMenu.SetActive(true);
        registerMenu.SetActive(false);
        settingsMenu.SetActive(false);
    }

    public void ShowRegisterMenu()
    {
        mainMenu.SetActive(false);
        loginMenu.SetActive(false);
        registerMenu.SetActive(true);
        settingsMenu.SetActive(false);
    }

    public void ShowSettingsMenu()
    {
        mainMenu.SetActive(false);
        loginMenu.SetActive(false);
        registerMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void CreateNewPlayer()
    {
        Debug.Log("Creating New Player");
    }

    public void LoginPlayer()
    {
        Debug.Log("Logging In Player");
    }

    public void StartGame()
    {
        GameSettings settings = new GameSettings();
        string selectedOption = targetsDropdown.options[targetsDropdown.value].text;

        if (!int.TryParse(selectedOption, out int maxTargets))
        {
            Debug.LogError("Invalid number of targets");
            return;
        }

        settings.maxTargets = maxTargets;

        gameManager.StartGame(settings);
    }
}
