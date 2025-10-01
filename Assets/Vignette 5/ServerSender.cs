using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

public class SubmitForm : MonoBehaviour
{
    [Header("Server")]
    [SerializeField] private string url = "https://thangtuner.com/api/bietvay.php";

    [Header("UI")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_InputField emailInput;     // ✅ new
    [SerializeField] private TMP_InputField bietVayInput;
    [SerializeField] private TMP_InputField overcomeInput;
    [SerializeField] private Button sendButton;
    [SerializeField] private TextMeshProUGUI statusLabel;   // optional

    void Start()
    {
        sendButton.onClick.AddListener(OnSendClicked);
        if (statusLabel) statusLabel.text = "";
    }

    private void OnSendClicked()
    {
        string name = nameInput.text.Trim();
        string email = emailInput.text.Trim();              // ✅ new
        string bietVay = bietVayInput.text.Trim();
        string overcome = overcomeInput.text.Trim();

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(bietVay) || string.IsNullOrEmpty(overcome))
        {
            if (statusLabel) statusLabel.text = "Please fill all fields.";
            return;
        }

        // bare-minimum email check
        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            if (statusLabel) statusLabel.text = "Please enter a valid email.";
            return;
        }

        FormData data = new FormData
        {
            name = name,
            email = email,                                   // ✅ new
            bietVay = bietVay,
            overcome = overcome
        };

        string json = JsonUtility.ToJson(data);
        StartCoroutine(SendFormData(json));
    }

    IEnumerator SendFormData(string json)
    {
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success && req.responseCode == 200)
            {
                if (statusLabel) statusLabel.text = "Submitted successfully!";
                Debug.Log("Server Response: " + req.downloadHandler.text);
            }
            else
            {
                if (statusLabel) statusLabel.text = "Error submitting form.";
                Debug.LogError($"HTTP {req.responseCode}: {req.error} | {req.downloadHandler.text}");
            }
        }
    }

    [System.Serializable]
    public class FormData
    {
        public string name;
        public string email;                                  // ✅ new
        public string bietVay;
        public string overcome;
    }
}
