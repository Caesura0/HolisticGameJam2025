using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI; 
    private bool isPaused = false; 

    public bool speedUp;

    public static EventHandler OnRestart;


    private void Start()
    {
        pauseMenuUI.SetActive(false); // Ensure the pause menu is initially inactive

        Controls.Instance.OnPlayerPause += () =>
        {
            if (isPaused)
            {
                Resume(); // If the game is already paused, resume it
            }
            else
            {
                Pause(); // If the game is not paused, pause it
            }
        };
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        GameplayManager.Instance.PauseGame();
        pauseMenuUI.SetActive(true); // Activate the pause menu UI
        AudioManager.Instance.PlayPauseClick();
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        GameplayManager.Instance.ResumeGame();
        pauseMenuUI.SetActive(false); // Deactivate the pause menu UI
        AudioManager.Instance.PlayResumeClick();
    }

    public void RestartScene()
    {
        //GameManager.Instance.ResetGame();
        Time.timeScale = 1f; // Ensure time scale is set to 1
        OnRestart?.Invoke(this, EventArgs.Empty);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Restart the current scene


    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // Ensure time scale is set to 1
        SceneManager.LoadScene("MainMenu"); // Load the MainMenu scene
        AudioManager.Instance.PlayButtonClick();
    }
}
