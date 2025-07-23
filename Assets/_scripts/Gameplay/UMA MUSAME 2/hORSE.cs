using UnityEngine;
using TMPro;

public class Horse2D : MonoBehaviour
{
    [HideInInspector] public float    baseSpeed;      // Set by RaceManager2D
    [HideInInspector] public bool     isRacing;       // Controlled by RaceManager2D
    [HideInInspector] public Transform finishLine;    // Assigned by RaceManager2D
    [HideInInspector] public float    fractionalOdds;// Assigned by RaceManager2D

    [Header("Odds Display")]
    public TextMeshProUGUI oddsText;                  // Drag your child TMP here

    [Header("Gallop Tilt")]
    [Tooltip("Max tilt angle (°)")]    public float tiltAmplitude = 5f;
    [Tooltip("How fast the tilting cycles")]    
    public float tiltFrequency = 10f;

    private Quaternion baseRotation;
    private float      phaseOffset;

    void Awake()
    {
        // Remember original rotation
        baseRotation = transform.rotation;
        // Stagger tilt among horses
        phaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        if (isRacing)
        {
            // 1) Gallop tilt
            float angle = Mathf.Sin(Time.time * tiltFrequency + phaseOffset) 
                          * tiltAmplitude;
            transform.rotation = baseRotation * Quaternion.Euler(0f, 0f, angle);

            // 2) Move on X
            float newX = transform.position.x 
                         + baseSpeed * Time.deltaTime;

            // 3) Stop at finish, snap rotation flat
            if (newX >= finishLine.position.x)
            {
                newX      = finishLine.position.x;
                isRacing  = false;
                transform.rotation = baseRotation;
            }

            // 4) Apply position
            transform.position = new Vector3(
                newX, 
                transform.position.y, 
                transform.position.z
            );
        }
    }
}