using System.Threading.Tasks;
using UnityEngine;
using Yarn.Unity;

public class GameManager : MonoBehaviour
{
    [YarnCommand("collapse")]
    public static void Collapse() => GameTransition.Instance.Collapse();

    [YarnCommand("expand")]
    public static void Expand() => GameTransition.Instance.Expand();

    // Async version: Dialogue waits until the Task completes.
    [YarnCommand("spawn")]
    public static async Task SpawnPoint(string spawnPointName)
    {
        // keep or remove Trim(), up to you
        var name = (spawnPointName ?? string.Empty).Trim();

        if (!SpawnPointHandler.TryGetSpawnPoint(name, out var point) || point == null)
        {
            Debug.LogError($"[GameManager] Yarn <<spawn {name}>>: spawn point not found.");
            return; // dialogue continues after this Task completes
        }

        SpawnPointHandler.InvokePlayerSpawn(point);

        // Yield one frame so the move is applied before the next command (e.g., <<expand>>)
        await Task.Yield();
    }
}