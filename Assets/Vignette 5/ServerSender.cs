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

    [Header("References")]
    [Tooltip("Centralized data storage (SubmitStorer).")]
    [SerializeField] private SubmitStorer storer;

    [Header("UI")]
    [SerializeField] private Button sendButton;
    [SerializeField] private TextMeshProUGUI statusLabel;   // optional

    void Start()
    {
        if (sendButton) sendButton.onClick.AddListener(OnSendClicked);
        if (statusLabel) statusLabel.text = "";

        if (!storer)
        {
            storer = FindObjectOfType<SubmitStorer>();
            if (!storer)
                Debug.LogWarning("SubmitForm: No SubmitStorer found. Please assign one in the Inspector.");
        }
    }

    private void OnSendClicked()
    {
        if (!storer)
        {
            if (statusLabel) statusLabel.text = "No data source (SubmitStorer) found!";
            return;
        }

        string name = (storer.nameValue ?? "").Trim();
        string email = (storer.emailValue ?? "").Trim();
        string bietVay = (storer.bietVayValue ?? "").Trim();
        string overcome = (storer.overcomeValue ?? "").Trim();

        if (string.IsNullOrEmpty(name) ||
            string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(bietVay) ||
            string.IsNullOrEmpty(overcome))
        {
            if (statusLabel) statusLabel.text = "Please fill all fields.";
            return;
        }

        // basic email check
        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            if (statusLabel) statusLabel.text = "Please enter a valid email.";
            return;
        }

        FormData data = new FormData
        {
            name = name,
            email = email,
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
        public string email;
        public string bietVay;
        public string overcome;
    }
}
