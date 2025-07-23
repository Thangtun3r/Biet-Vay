// MoneyManager.cs
using UnityEngine;
using TMPro;

public class MoneyManager : MonoBehaviour
{
    [Header("Starting Balance")]
    public float startingBalance = 100f;
    private float balance;

    [Header("UI References")]
    public TextMeshProUGUI balanceText;   // shows current balance
    public TMP_InputField amountInput;    // enter an amount

    void Start()
    {
        // initialize
        balance = startingBalance;
        UpdateBalanceUI();
    }

    /// <summary>
    /// Called by your “Tap In” button.
    /// </summary>
    public void OnTapInButton()
    {
        if (TryGetAmount(out float amt))
        {
            balance += amt;
            Debug.Log($"Tapped IN {amt:F2}. New balance: {balance:F2}");
            UpdateBalanceUI();
        }
    }

    /// <summary>
    /// Called by your “Tap Out” button.
    /// </summary>
    public void OnTapOutButton()
    {
        if (TryGetAmount(out float amt))
        {
            if (amt > balance)
            {
                Debug.LogWarning($"Cannot tap OUT {amt:F2}: only {balance:F2} available.");
                // you could flash a UI warning here
            }
            else
            {
                balance -= amt;
                Debug.Log($"Tapped OUT {amt:F2}. New balance: {balance:F2}");
                UpdateBalanceUI();
            }
        }
    }

    // Parse & validate the input field
    private bool TryGetAmount(out float result)
    {
        result = 0f;
        if (amountInput == null)
        {
            Debug.LogError("Amount InputField not assigned!");
            return false;
        }

        if (!float.TryParse(amountInput.text, out result) || result <= 0f)
        {
            Debug.LogWarning("Please enter a valid positive number.");
            return false;
        }

        return true;
    }

    // Refresh the balance display
    private void UpdateBalanceUI()
    {
        if (balanceText != null)
            balanceText.text = $"Balance: {balance:F2}";
    }
}
