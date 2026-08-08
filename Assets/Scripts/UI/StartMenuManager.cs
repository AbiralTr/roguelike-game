using UnityEngine;

public class StartMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject startMenuUI;

    void Awake()
    {
        Time.timeScale = 0f;
        startMenuUI.SetActive(true);
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
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
