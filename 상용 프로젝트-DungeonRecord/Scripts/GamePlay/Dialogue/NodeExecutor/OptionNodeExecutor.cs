using System;
using System.Linq;
using BackEnd;
using XNode;

public class OptionNodeExecutor : IDialogueNodeExecutor
{
    public void Execute(DialogueNode node)
    {
        var optionNode = (OptionNode)node;
        DialogueManager.Instance.UI.SetInteractableNextButton(false);

        var staticOptions = optionNode.DialogueOptions.Select(opt => new DialogueNodeOption(
            TextTokenResolver.Resolve(Localize.Get(opt.Sentence)),
            () =>
            {
                DialogueManager.Instance.SetCurNode(opt.ConnectingNode);
                DialogueManager.Instance.ContinueDialogue();
            }));

        var combinedOptions = DialogueManager.Instance.ConsumeDynamicOptions()
            .Concat(staticOptions)
            .ToList();
        DialogueManager.Instance.UI.SetOption(combinedOptions);
    }
}
