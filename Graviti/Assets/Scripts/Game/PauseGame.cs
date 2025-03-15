using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PauseGame : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel; 
    private bool isPaused = false;


    private void Start()
    {
        pausePanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }  
            else
            {
                Pause();
            }
                
        }
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;  
        pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;  
        pausePanel.SetActive(false);
    }
}
