using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreenManager : MonoBehaviour
{
    [SerializeField] private GameObject deathScreenUI;
    [SerializeField] private GameStateManager gameStateManager;

    public void Show()
    {
        gameStateManager.Die();
        deathScreenUI.SetActive(true);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
