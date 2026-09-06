using System;
using UnityEngine;

public static class SystemPopupTemplate
{
    public static void ShowExitPopup(string title, string msg)
    {
        var param = UIManager.Instance.PopupSystem.PushMessage<OKPopupParam>(SystemPopup.AssetID, msg);
        param.SetTitle(title);
        param.SetEvent(() =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        });
    }

    public static void ShowExitYesNoPopup(string title, string msg)
    {
        var param = UIManager.Instance.PopupSystem.PushMessage<YesNoPopupPararm>(SystemPopup.AssetID, msg);
        param.SetTitle(title);
        param.SetEvents(() =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }, null);
    }

    public static void ShowOKPopup(string title, string msg)
    {
        var param = UIManager.Instance.PopupSystem.PushMessage<OKPopupParam>(SystemPopup.AssetID, msg);
        param.SetTitle(title);
    }

    public static void ShowOKPopup(string title, string msg, Action onConfirm)
    {
        var param = UIManager.Instance.PopupSystem.PushMessage<OKPopupParam>(SystemPopup.AssetID, msg);
        param.SetTitle(title);
        param.SetEvent(onConfirm);
    }

    public static void ShowYesNoPopup(string title, string msg, Action onYes, Action onNo = null)
    {
        var param = UIManager.Instance.PopupSystem.PushMessage<YesNoPopupPararm>(SystemPopup.AssetID, msg);
        param.SetTitle(title);
        param.SetEvents(onYes, onNo);
    }
}
