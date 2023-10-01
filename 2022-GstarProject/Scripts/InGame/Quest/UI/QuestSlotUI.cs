using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestSlotUI : MonoBehaviour
{
    public QuestData questData;
    public TextMeshProUGUI Name;
    public QuestContentUI ContentUI;
    // Start is called before the first frame update
    public void OnClick()
    {
        ContentUI.UpdateUI(questData);
    }

}
