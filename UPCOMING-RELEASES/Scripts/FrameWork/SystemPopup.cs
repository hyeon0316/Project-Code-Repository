using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SystemPopup : PopupBehaviour
{
    public static AssetID AssetID = new("Prefabs/UI/Popup/SystemPopup");
    [SerializeField] private Text m_TitleText;
    [SerializeField] private Text m_GuideText;
    [SerializeField] private UIMultiView m_UIMultiView;

    private CommonMessagePopupPararm m_Param;

    public override void SetParam(BasePopupParam pararm)
    {
        m_Param = pararm as CommonMessagePopupPararm;
    }

    public override void Show()
    {
        if(!m_UIMultiView.SetSelectView(m_Param.GetOption().ToString()))
        {
            Debug.LogError("Not loaded view name");
        }
        m_TitleText.text = m_Param.GetTitle();
        m_GuideText.text = m_Param.GetMessage();
    }

    public void OnClickEvent_OK()
    {
        m_Param.OK();
        Close();
    }

    public void OnClickEvent_Yes()
    {
        m_Param.Yes();
        Close();
    }

    public void OnClickEvent_No()
    {
        m_Param.No();
        Close();
    }
}
