using UnityEngine;

public class InputActionsHub : MonoBehaviour
{
    public InputSystem_Actions Actions { get; private set; }

    void Awake()
    {
        Actions = new InputSystem_Actions();
        Actions.Player.Enable();
    }

    void OnDestroy()
    {
        Actions.Dispose();
    }
}
