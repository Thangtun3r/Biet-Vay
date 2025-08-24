using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading;
using Yarn.Unity;

public class WordDisplayManager : DialoguePresenterBase
{
    public static event Action OnOptionsDisplayed;
    private List<DialogueOption> lastOptions;
    private DialogueOption selectedOption;

    public override YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token) => YarnTask.CompletedTask;
    public override YarnTask OnDialogueStartedAsync() => YarnTask.CompletedTask;
    public override YarnTask OnDialogueCompleteAsync() => YarnTask.CompletedTask;

    public void SelectOptionByID(int id)
    {
        if (id >= 0 && id < lastOptions.Count)
        {
            selectedOption = lastOptions[id];
        }
        else
        {
            Debug.LogWarning($"Invalid ID passed to SelectOptionByID: {id}");
        }
    }

    public override async YarnTask<DialogueOption> RunOptionsAsync(DialogueOption[] options, CancellationToken cancellationToken)
    {
        selectedOption = null;
        lastOptions = options.ToList();
        OnOptionsDisplayed?.Invoke();

        WordPoolManager.Instance.ResetPool();

        for (int i = 0; i < options.Length; i++)
        {
            var parsed = options[i].Line.Text; // MarkupParseResult
            WordPoolManager.Instance.CreateSentenceFromText(parsed, i);
        }

        while (selectedOption == null && !cancellationToken.IsCancellationRequested)
            await YarnTask.Yield();

        return selectedOption;
    }
}