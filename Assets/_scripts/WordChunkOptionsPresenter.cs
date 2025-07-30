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

    public override async YarnTask<DialogueOption> RunOptionsAsync(DialogueOption[] options, CancellationToken cancellationToken)
    {
        DialogueOption selected = null;

        foreach (var option in options)
        {
            string line = option.Line.Text.Text;
            Debug.Log("called");
            WordPoolManager.Instance.CreateSentenceFromText(line);
            
        }
        while (selected == null && !cancellationToken.IsCancellationRequested)
            await YarnTask.Yield();

        return selected;
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
