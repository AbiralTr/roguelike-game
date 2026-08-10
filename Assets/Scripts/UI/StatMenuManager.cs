using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class StatMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject statMenuUI;
    [SerializeField] private PlayerData playerData;
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text weaponText;

    private bool isOpen;

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.tabKey.wasPressedThisFrame)
        {
            bool accepted = isOpen ? gameStateManager.RequestCloseStatMenu() : gameStateManager.RequestOpenStatMenu();
            if (accepted)
            {
                isOpen = !isOpen;
                statMenuUI.SetActive(isOpen);
            }
        }

        if (!isOpen) return;

        healthText.text = $"Health: {playerData.currentHealth} / {playerData.maxHealth}";
        damageText.text = $"Melee Damage: {playerData.meleeDamage}\nProjectile Damage: {playerData.projectileDamage}";
        speedText.text = $"Speed: {playerData.moveSpeed:0.0}";
        weaponText.text = $"Weapon: {playerAttack.EquippedWeaponName}";
    }
}
