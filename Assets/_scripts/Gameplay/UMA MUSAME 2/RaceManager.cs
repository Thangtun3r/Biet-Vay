using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager2D : MonoBehaviour
{
    [Header("Prefab & Lines")]
    public GameObject horsePrefab;
    public Transform  startLine;    // e.g. left edge
    public Transform  finishLine;   // e.g. right edge

    [Header("Race Settings")]
    public int   numberOfHorses = 6;
    public float laneSpacingY   = 1.5f;
    public float minSpeed       = 2f;
    public float maxSpeed       = 5f;

    private List<Horse2D> horses = new List<Horse2D>();

    void Start()
    {
        SetupRace();
        CalculateOdds();
        DisplayOddsOnHorses();
        StartCoroutine(RunRace());
    }

    void SetupRace()
    {
        horses.Clear();
        for (int i = 0; i < numberOfHorses; i++)
        {
            // stack vertically
            Vector3 pos = startLine.position 
                        + Vector3.up * i * laneSpacingY;
            var go = Instantiate(
                horsePrefab, 
                pos, 
                Quaternion.identity, 
                transform
            );
            var h = go.GetComponent<Horse2D>() 
                  ?? go.AddComponent<Horse2D>();

            h.finishLine = finishLine;
            h.baseSpeed  = Random.Range(minSpeed, maxSpeed);
            h.isRacing   = false;
            horses.Add(h);
        }
    }

    void CalculateOdds()
    {
        // simple fixed‐odds: weight by speed
        float total = 0f;
        foreach (var h in horses) total += h.baseSpeed;

        foreach (var h in horses)
        {
            float p = h.baseSpeed / total;                // win probability
            h.fractionalOdds = (1f / p) - 1f;             // e.g. 2.3
        }
    }

    void DisplayOddsOnHorses()
    {
        foreach (var h in horses)
        {
            // round to nearest int, at least 1:1
            int ratio = Mathf.Max(1, Mathf.RoundToInt(h.fractionalOdds));
            h.oddsText.text = $"1:{ratio}";
            h.oddsText.gameObject.SetActive(true);
        }
    }

    // RaceManager2D.cs
    IEnumerator RunRace()
    {
        yield return new WaitForSeconds(0.5f);

        // ───── Start each horse AND hide its odds label ─────
        foreach (var h in horses)
        {
            h.oddsText.gameObject.SetActive(false);
            h.isRacing = true;
        }
        // ────────────────────────────────────────────────────

        // wait for the first one to finish…
        while (true)
        {
            for (int i = 0; i < horses.Count; i++)
            {
                if (!horses[i].isRacing)
                {
                    Debug.Log($"🏆 Horse #{i+1} wins!");
                    yield break;
                }
            }
            yield return null;
        }
    }

}
