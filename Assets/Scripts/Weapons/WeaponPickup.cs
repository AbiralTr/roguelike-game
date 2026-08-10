using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private InputActionsHub inputActionsHub;

    private PlayerAttack playerInRange;

    void Awake()
    {
        ApplySprite();
    }

    public void Init(WeaponData data, InputActionsHub hub)
    {
        weaponData = data;
        inputActionsHub = hub;
        ApplySprite();
    }

    private void ApplySprite()
    {
        if (spriteRenderer != null) spriteRenderer.sprite = weaponData != null ? weaponData.icon : null;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = other.GetComponent<PlayerAttack>();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = null;
    }

    void Update()
    {
        if (playerInRange == null) return;

        if (inputActionsHub.Actions.Player.Interact.WasPressedThisFrame())
        {
            playerInRange.Equip(weaponData, transform.position);
            Destroy(gameObject);
        }
    }
}
