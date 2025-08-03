using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading;
using Yarn.Unity;

public class WordDisplayManager : DialoguePresenterBase
{
    
    private DialogueOption selectedOption;
    public override YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
    {
        return YarnTask.CompletedTask;
    }

    public void SelectOption(DialogueOption option)
    {
        selectedOption = option;
    }

    public override async YarnTask<DialogueOption> RunOptionsAsync(DialogueOption[] options, CancellationToken cancellationToken)
    {
        selectedOption = null;

        foreach (var option in options)
        {
            string line = option.Line.Text.Text;
            WordPoolManager.Instance.CreateSentenceFromText(line);
        }

        while (selectedOption == null && !cancellationToken.IsCancellationRequested)
            await YarnTask.Yield();

        return selectedOption;
    }


    public override YarnTask OnDialogueStartedAsync()
    {
        return YarnTask.CompletedTask;
    }

    public override YarnTask OnDialogueCompleteAsync()
    {
        return YarnTask.CompletedTask;
    }

    private void CreateSentence()
    {
        
        
    }

  
}
