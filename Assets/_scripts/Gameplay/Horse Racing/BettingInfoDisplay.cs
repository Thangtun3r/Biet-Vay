using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BettingInfoDisplay : MonoBehaviour
{
    [Header("UI References")]
    public Image silksImage;
    public TMP_Text horseNameText;
    public TMP_Text payoutOddsText;
    public TMP_Text perHorsePoolText;

    // Data fields
    private string _horseName;
    private Color _silksColor;
    private string _payoutOdds;
    private long _perHorsePool;

    public void SetHorseName(string name)
    {
        _horseName = name;
        if (horseNameText != null)
            horseNameText.text = name;
    }

    public void SetSilksColor(Color color)
    {
        _silksColor = color;
        if (silksImage != null)
            silksImage.color = color;
    }

    public void SetPayoutOdds(string odds)
    {
        _payoutOdds = odds;
        if (payoutOddsText != null)
            payoutOddsText.text = odds;
    }

    public void SetPerHorsePool(long pool)
    {
        _perHorsePool = pool;
        if (perHorsePoolText != null)
            perHorsePoolText.text = pool.ToString("N0"); // "150,500,345"
    }
}