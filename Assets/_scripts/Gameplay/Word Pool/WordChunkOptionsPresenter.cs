using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading;
using Yarn.Unity;

public class WordDisplayManager : DialoguePresenterBase
{
    private List<DialogueOption> lastOptions;
    private DialogueOption selectedOption;
    public override YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
    {
        return YarnTask.CompletedTask;
    }

    public void SelectOptionByID(int id)
    {
        if (id >= 0 && id < lastOptions.Count)
        {
            selectedOption = lastOptions[id]; // ✅ use index directly
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

        for (int i = 0; i < options.Length; i++)
        {
            string line = options[i].Line.Text.Text;
            WordPoolManager.Instance.CreateSentenceFromText(line, i); 
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
