using System;

public static class WordPuzzleChecker
{
    public static event Action OnPuzzleSolved;

    public static void MarkSolved()
    {
        OnPuzzleSolved?.Invoke();
    }
}