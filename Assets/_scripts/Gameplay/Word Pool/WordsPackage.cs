using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WordsPackage", menuName = "Word System/Words Package")]
public class WordsPackage : ScriptableObject
{
    public List<WordsTemplate> words = new List<WordsTemplate>();
    public int wordsCount;
}