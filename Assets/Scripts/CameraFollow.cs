using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, -10f);

    [Header("Vertical Look")]
    [SerializeField] private float lookDownOffset = -3f;
    [SerializeField] private float lookTransitionSpeed = 3f;

    [Header("Jump Dampening")]
    [SerializeField] private float airborneYDamping = 0.1f;
    [SerializeField] private PlayerMovement playerMovement;

    private float currentYOffset;
    private float lockedY;

    void Awake()
    {
        lockedY = target.position.y;
    }

    void LateUpdate()
    {
        if (target == null) return;

        var keyboard = Keyboard.current;
        float targetYOffset = 0f;

        if (keyboard != null && keyboard.sKey.isPressed)
        {
            targetYOffset = lookDownOffset;
        }

        currentYOffset = Mathf.Lerp(currentYOffset, targetYOffset, lookTransitionSpeed * Time.deltaTime);

        if (playerMovement.IsGrounded)
        {
            lockedY = Mathf.Lerp(lockedY, target.position.y, smoothSpeed * Time.deltaTime);
        }
        else
        {
            lockedY = Mathf.Lerp(lockedY, target.position.y, airborneYDamping * Time.deltaTime);
        }

        Vector3 desiredPosition = new Vector3(
            target.position.x,
            lockedY,
            target.position.z
        ) + offset + new Vector3(0, currentYOffset, 0);

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}