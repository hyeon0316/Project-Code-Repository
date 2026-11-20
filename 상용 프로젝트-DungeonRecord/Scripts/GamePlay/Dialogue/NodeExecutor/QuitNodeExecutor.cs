using System;
using XNode;

public class QuitNodeExecutor : IDialogueNodeExecutor
{
    public void Execute(DialogueNode node)
    {
        var options = DialogueManager.Instance.ConsumeDynamicOptions();
        if (options.Count > 0)
        {
            options.Add(new DialogueNodeOption(Localize.Get("DIALOGUE_EXIT"), DialogueManager.Instance.QuitDialogue));
            DialogueManager.Instance.UI.SetInteractableNextButton(false);
            DialogueManager.Instance.UI.SetOption(options);
            return;
        }

        DialogueManager.Instance.QuitDialogue();
    }
}
