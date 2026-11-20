using Cysharp.Threading.Tasks;

public class PlayerNodeExecutor : IDialogueNodeExecutor
{
    public void Execute(DialogueNode node)
    {
        var playerNode = (PlayerNode)node;
        DialogueManager.Instance.UI.SetInteractableNextButton(true);
        DialogueManager.Instance.SetPlayerSubText();

        if (TownCharacter.OverHeadPosProvider != null)
        {
            DialogueManager.Instance.MovePanelToWorldPos(TownCharacter.OverHeadPosProvider());
        }

        string raw = Localize.Get(playerNode.Sentence);
        string text = TextTokenResolver.Resolve(raw);

        DialogueManager.Instance.UI.TypeTextAsync(text).ContinueWith(() =>
        {
            DialogueManager.Instance.SetNextNode();
        }).Forget();
    }
}
