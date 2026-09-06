using System;
using XNode;

public class ShopNodeExecutor : IDialogueNodeExecutor
{
    public void Execute(DialogueNode node)
    {
        var shopNode = (ShopNode)node;
        ContentsManager.Instance.Get<ShopContents>().OpenVirtualShop(shopNode.ShopID);
        DialogueManager.Instance.QuitDialogue();
    }
}
