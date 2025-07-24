using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager2D : MonoBehaviour
{
    [Header("Prefab & Lines")]
    public GameObject horsePrefab;
    public Transform startLine;
    public Transform finishLine;

    [Header("Race Settings")]
    public int numberOfHorses = 6;
    public float laneSpacingY = 1.5f;
    public float minSpeed = 2f;
    public float maxSpeed = 5f;

    private List<Horse2D> horses = new List<Horse2D>();

    void Start()
    {
        SetupRace();
        CalculateOdds();
        DisplayOddsOnHorses();
        StartCoroutine(RaceSequence());
    }

    IEnumerator RaceSequence()
    {
        yield return StartCoroutine(IntroduceHorses());
        yield return StartCoroutine(Countdown());
        yield return StartCoroutine(RunRaceWithBroadcast());
    }

    IEnumerator IntroduceHorses()
    {
        float introTime = 10f / horses.Count;
        for (int i = 0; i < horses.Count; i++)
        {
            Debug.Log($"🎤 Introducing Horse #{i + 1}! Odds: {horses[i].oddsText.text}");
            yield return new WaitForSeconds(introTime);
        }
        Debug.Log("All horses introduced!");
    }

    IEnumerator Countdown()
    {
        Debug.Log("Get ready for the race!");
        yield return new WaitForSeconds(1f);
        Debug.Log("3...");
        yield return new WaitForSeconds(1f);
        Debug.Log("2...");
        yield return new WaitForSeconds(1f);
        Debug.Log("1...");
        yield return new WaitForSeconds(1f);
        Debug.Log("GO!");
    }

    void SetupRace()
    {
        horses.Clear();
        for (int i = 0; i < numberOfHorses; i++)
        {
            Vector3 pos = startLine.position + Vector3.up * i * laneSpacingY;
            var go = Instantiate(horsePrefab, pos, Quaternion.identity, transform);
            var h = go.GetComponent<Horse2D>() ?? go.AddComponent<Horse2D>();
            h.finishLine = finishLine;
            h.baseSpeed = Random.Range(minSpeed, maxSpeed);
            h.isRacing = false;

            // Randomize horse color
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = Random.ColorHSV();

            horses.Add(h);
        }
    }

    void CalculateOdds()
    {
        float total = 0f;
        foreach (var h in horses) total += h.baseSpeed;
        foreach (var h in horses)
        {
            float p = h.baseSpeed / total;
            h.fractionalOdds = (1f / p) - 1f;
        }
    }

    void DisplayOddsOnHorses()
    {
        foreach (var h in horses)
        {
            int ratio = Mathf.Max(1, Mathf.RoundToInt(h.fractionalOdds));
            h.oddsText.text = $"1:{ratio}";
            h.oddsText.gameObject.SetActive(true);
        }
    }

    IEnumerator RunRaceWithBroadcast()
    {
        yield return new WaitForSeconds(0.5f);

        foreach (var h in horses)
        {
            h.oddsText.gameObject.SetActive(false);
            h.isRacing = true;
        }

        bool raceFinished = false;
        int winnerIndex = -1;

        while (!raceFinished)
        {
            // Find the horse furthest along the X axis
            float maxX = float.MinValue;
            int leaderIndex = -1;
            for (int i = 0; i < horses.Count; i++)
            {
                if (horses[i].transform.position.x > maxX)
                {
                    maxX = horses[i].transform.position.x;
                    leaderIndex = i;
                }
                if (!horses[i].isRacing)
                {
                    raceFinished = true;
                    winnerIndex = i;
                }
            }

            Debug.Log($"📢 Current Leader: Horse #{leaderIndex + 1}");
            yield return new WaitForSeconds(1f); // Broadcast every second
        }

        Debug.Log($"🏆 Horse #{winnerIndex + 1} wins!");
    }
}