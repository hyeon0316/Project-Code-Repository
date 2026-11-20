using System;

public class BasePopupParam
{
    private AssetID m_AssetID;
    private string m_Message;

    public void Set(AssetID assetID, string message)
    {
        m_AssetID = assetID;
        m_Message = message;
    }

    public void SetMessage(string message)
    {
        m_Message = message;
    }

    public AssetID GetAssetID()
    {
        return m_AssetID;
    }

    public string GetMessage()
    {
        return m_Message;
    }
}

public abstract class CommonMessagePopupPararm : BasePopupParam
{
    public enum EMessageOption
    {
        Ok,
        YesNo,
    }

    private string m_Title;

    public void SetTitle(string title)
    {
        m_Title = title;
    }

    public virtual void OK()
    {

    }

    public virtual void Yes()
    {

    }

    public virtual void No()
    {

    }

    public virtual string GetOKString()
    {
        return "";
    }

    public virtual string GetYesString()
    {
        return "";
    }

    public virtual string GetNoString()
    {
        return "";
    }

    public string GetTitle()
    {
        return m_Title;
    }

    public abstract EMessageOption GetOption();

}

public class OKPopupParam : CommonMessagePopupPararm
{
    private Action m_OkCallback;

    public void SetEvent(Action ok)
    {
        m_OkCallback = ok;
    }

    public override void OK()
    {
        m_OkCallback?.Invoke();
    }

    public override EMessageOption GetOption()
    {
        return EMessageOption.Ok;
    }
}

public class YesNoPopupPararm : CommonMessagePopupPararm
{
    private Action m_YesCallback;
    private Action m_NoCallback;

    public void SetEvents(Action yes, Action no)
    {
        m_YesCallback = yes;
        m_NoCallback = no;
    }

    public override void Yes()
    {
        m_YesCallback?.Invoke();
    }

    public override void No()
    {
        m_NoCallback?.Invoke();
    }

    public sealed override void OK()
    { }

    public sealed override string GetOKString()
    {
        return "";
    }

    public override EMessageOption GetOption()
    {
        return EMessageOption.YesNo;
    }
}
