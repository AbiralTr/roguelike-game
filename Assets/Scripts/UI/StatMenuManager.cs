using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class StatMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject statMenuUI;
    [SerializeField] private PlayerData playerData;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private TMP_Text speedText;

    private bool isOpen;

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.tabKey.wasPressedThisFrame)
        {
            isOpen = !isOpen;
            statMenuUI.SetActive(isOpen);
            Time.timeScale = isOpen ? 0f : 1f;
        }

        if (!isOpen) return;

        healthText.text = $"Health: {playerData.currentHealth} / {playerData.maxHealth}";
        damageText.text = $"Melee Damage: {playerData.meleeDamage}\nProjectile Damage: {playerData.projectileDamage}";
        speedText.text = $"Speed: {playerData.moveSpeed:0.0}";
    }
}
