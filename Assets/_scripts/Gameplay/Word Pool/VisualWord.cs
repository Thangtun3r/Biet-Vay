using System;
using TMPro;
using UnityEngine;

public class VisualWord : MonoBehaviour
{
    public GameObject theTransform;
    public GameObject logicWordObject;
    public Transform target;
    public string logicWordText;
    public TextMeshProUGUI text;
    public float followSpeed = 10f;
    public float horizontalOffset = 0f; // Positive for right, negative for left

    private void Update()
    {
        if (text != null)
        {
            text.text = logicWordObject.GetComponent<WordID>().word;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Calculate offset position
        Vector3 offsetPosition = target.position + target.right * horizontalOffset;

        // Smooth follow (position only)
        theTransform.transform.position = Vector3.Lerp(transform.position, offsetPosition, followSpeed * Time.deltaTime);

    }
}