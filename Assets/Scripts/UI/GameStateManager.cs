using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public enum State
    {
        MainMenu,
        Playing,
        Paused,
        StatMenu,
        Dead
    }

    public State Current { get; private set; } = State.MainMenu;

    public void ShowMainMenu()
    {
        Current = State.MainMenu;
        Time.timeScale = 0f;
    }

    public void StartGame()
    {
        Current = State.Playing;
        Time.timeScale = 1f;
    }

    public bool RequestPause()
    {
        if (Current != State.Playing) return false;
        Current = State.Paused;
        Time.timeScale = 0f;
        return true;
    }

    public bool RequestResume()
    {
        if (Current != State.Paused) return false;
        Current = State.Playing;
        Time.timeScale = 1f;
        return true;
    }

    public bool RequestOpenStatMenu()
    {
        if (Current != State.Playing) return false;
        Current = State.StatMenu;
        Time.timeScale = 0f;
        return true;
    }

    public bool RequestCloseStatMenu()
    {
        if (Current != State.StatMenu) return false;
        Current = State.Playing;
        Time.timeScale = 1f;
        return true;
    }

    public void Die()
    {
        Current = State.Dead;
        Time.timeScale = 0f;
    }
}
