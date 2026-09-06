using Cysharp.Threading.Tasks;
using System;
using XNode;

public class ActorNodeExecutor : IDialogueNodeExecutor
{
    public void Execute(DialogueNode node)
    {
        var actorNode = (ActorNode)node;
        DialogueManager.Instance.UI.SetInteractableNextButton(true);
        DialogueManager.Instance.SetActorSubText();

        var curActor = DialogueManager.Instance.CurrentActor;
        if (curActor != null)
        {
            DialogueManager.Instance.MovePanelToWorldPos(curActor.GetOverHeadPos());
        }

        string raw = Localize.Get(actorNode.Sentence);
        string text = TextTokenResolver.Resolve(raw);

        DialogueManager.Instance.UI.TypeTextAsync(text).ContinueWith(() =>
        {
            DialogueManager.Instance.SetNextNode();
        }).Forget();
    }
}
