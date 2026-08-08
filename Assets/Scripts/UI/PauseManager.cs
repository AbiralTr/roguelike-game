using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameStateManager gameStateManager;

    private bool isPaused;

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    private void Pause()
    {
        if (!gameStateManager.RequestPause()) return;
        isPaused = true;
        pauseMenuUI.SetActive(true);
    }

    private void Resume()
    {
        if (!gameStateManager.RequestResume()) return;
        isPaused = false;
        pauseMenuUI.SetActive(false);
    }
}
