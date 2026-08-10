using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private InputActionsHub inputActionsHub;

    private bool isPaused;

    void Update()
    {
        if (inputActionsHub.Actions.Player.Pause.WasPressedThisFrame())
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

    public void Resume()
    {
        if (!gameStateManager.RequestResume()) return;
        isPaused = false;
        pauseMenuUI.SetActive(false);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
