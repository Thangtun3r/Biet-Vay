// GachaObjectSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "GachaEntry", menuName = "Gacha/Entry")]
public class GachaObjectSO : ScriptableObject
{
    [Header("Display")]
    public string displayName;

    [Header("Prefab to spawn for this entry")]
    public GameObject prefab; // the whole card/object prefab
}