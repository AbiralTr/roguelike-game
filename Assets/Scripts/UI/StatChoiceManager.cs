using UnityEngine;
using TMPro;

public class StatChoiceManager : MonoBehaviour
{
    [SerializeField] private GameObject choiceMenuUI;
    [SerializeField] private PlayerData playerData;
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private StatPickupData[] pool;

    [SerializeField] private TMP_Text optionTextA;
    [SerializeField] private TMP_Text optionTextB;
    [SerializeField] private TMP_Text optionTextC;

    private readonly StatPickupData[] currentOptions = new StatPickupData[3];

    public void Open()
    {
        if (pool.Length < 3)
        {
            Debug.LogWarning("StatChoiceManager needs at least 3 entries in its pool.");
            return;
        }

        if (!gameStateManager.RequestOpenChoiceMenu()) return;

        RollOptions();
        choiceMenuUI.SetActive(true);
    }

    private void RollOptions()
    {
        StatPickupData[] shuffled = (StatPickupData[])pool.Clone();
        for (int i = 0; i < shuffled.Length; i++)
        {
            int swapIndex = Random.Range(i, shuffled.Length);
            (shuffled[i], shuffled[swapIndex]) = (shuffled[swapIndex], shuffled[i]);
        }

        currentOptions[0] = shuffled[0];
        currentOptions[1] = shuffled[1];
        currentOptions[2] = shuffled[2];

        optionTextA.text = Describe(currentOptions[0]);
        optionTextB.text = Describe(currentOptions[1]);
        optionTextC.text = Describe(currentOptions[2]);
    }

    private string Describe(StatPickupData data)
    {
        return $"+{data.amount:0.#} {data.statType}";
    }

    public void ChooseA() => Choose(0);
    public void ChooseB() => Choose(1);
    public void ChooseC() => Choose(2);

    private void Choose(int index)
    {
        StatPickupData chosen = currentOptions[index];
        playerData.ApplyStatBoost(chosen.statType, chosen.amount);
        choiceMenuUI.SetActive(false);
        gameStateManager.RequestCloseChoiceMenu();
    }
}
