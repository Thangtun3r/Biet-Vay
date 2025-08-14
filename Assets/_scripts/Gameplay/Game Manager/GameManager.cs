using System.Collections;
using UnityEngine;
using Yarn.Unity;

public class GameManager : MonoBehaviour
{
    [YarnCommand("collapse")]
    public static void Collapse() => GameTransition.Instance.Collapse();

    [YarnCommand("expand")]
    public static void Expand() => GameTransition.Instance.Expand();

    // Safer coroutine: give physics + rendering a chance to settle
    // before Yarn continues to the next command.
    [YarnCommand("spawn")]
    public static IEnumerator SpawnPoint(string spawnPointName)
    {
        var name = (spawnPointName ?? string.Empty).Trim();

        if (!SpawnPointHandler.TryGetSpawnPoint(name, out var point) || point == null)
        {
            Debug.LogError($"[GameManager] Yarn <<spawn {name}>>: spawn point not found.");
            yield break; // dialogue continues
        }

        // Perform the spawn/teleport.
        SpawnPointHandler.InvokePlayerSpawn(point);

        // --- Robust settling sequence ---
        // If your spawn uses CharacterController/rigidbody or navmesh,
        // let one FixedUpdate tick process first:
        yield return new WaitForFixedUpdate();

        // Then wait for the next Update so transforms are visible to scripts:
        yield return null;

        // Then wait until the end of that frame so rendering catches up:
        yield return new WaitForEndOfFrame();
        // --------------------------------
    }
}