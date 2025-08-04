using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WordsPackage", menuName = "Word System/Words Package")]
public class WordsPackage : ScriptableObject
{
    [Header("Package ID")]
    public string packageID; // Unique identifier for this package
    
    [Header("Words Data")]
    public List<WordsTemplate> words = new List<WordsTemplate>();
    public int wordsCount;
}