using UnityEngine;

public class StartMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject startMenuUI;
    [SerializeField] private GameStateManager gameStateManager;

    void Awake()
    {
        gameStateManager.ShowMainMenu();
        startMenuUI.SetActive(true);
    }

    public void StartGame()
    {
        gameStateManager.StartGame();
        startMenuUI.SetActive(false);
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
