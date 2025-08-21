using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI; 
    private bool isPaused = false; 

    public bool speedUp;
    [SerializeField] float speedUpSpeed = 4;
    [SerializeField] GameObject gameoverPanel;


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
    void Update()
    {

        //if (Input.GetKeyDown(KeyCode.Alpha0))
        //{
        //    speedUp = !speedUp;
        //}
        //if (speedUp)
        //{
        //    Time.timeScale = 4;
        //}
        //else
        //{
        //    Time.timeScale = 2;
        //}
        //if (Input.GetKeyDown(KeyCode.Escape) /*&& !GameManager.Instance.GameOver*/)
        //{
        //    if (isPaused)
        //    {
        //        Resume(); // If the game is already paused, resume it
        //    }
        //    else
        //    {
        //        Pause(); // If the game is not paused, pause it
        //    }
        //}
    }

    public void Pause()
    {

        Time.timeScale = 0f; // Pause the game by setting time scale to 0
        isPaused = true;
        Debug.Log(Time.timeScale);
        pauseMenuUI.SetActive(true); // Activate the pause menu UI
        AudioManager.Instance.PlayPauseClick();
    }

    public void Resume()
    {
        Time.timeScale = 1f; // Resume the game by setting time scale to 1
        isPaused = false;
        pauseMenuUI.SetActive(false); // Deactivate the pause menu UI
        AudioManager.Instance.PlayResumeClick();
    }

    public void RestartScene()
    {
        CloseHighScoreWindow();
        CloseGameOverPanel();
        //GameManager.Instance.ResetGame();
        Time.timeScale = 1f; // Ensure time scale is set to 1
        OnRestart?.Invoke(this, EventArgs.Empty);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Restart the current scene


    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // Load the MainMenu scene
        Time.timeScale = 1f; // Ensure time scale is set to 1
        AudioManager.Instance.PlayButtonClick();
    }

    public void OpenHighScoreWindow()
    {
        //HighscoreTable.Instance.RefreshAndLoadHighscoreList();
    }

    public void CloseHighScoreWindow()
    {
        //HighscoreTable.Instance.CloseVisual();
    }
    public void CloseGameOverPanel()
    {
        gameoverPanel.SetActive(false);
    }
}