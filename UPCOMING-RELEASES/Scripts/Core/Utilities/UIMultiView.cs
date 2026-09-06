using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class UIMultiView : MonoBehaviour
{
    [System.Serializable]
    public class WidgetList
    {
        public string Key;
        public List<Widget> List;
    }

    [System.Serializable]
    public class Widget
    {
        public GameObject Object;
        public Vector2 AnchoredPosition;
        public Vector2 SizeDelta;
        public bool Active;

        public void Apply()
        {
            Object.SetActive(Active);
            RectTransform RectTransform = Object.GetComponent<RectTransform>();

            RectTransform.anchoredPosition = AnchoredPosition;
            RectTransform.sizeDelta = SizeDelta;
        }
    }

    [SerializeField] private List<WidgetList> m_Widgets = new List<WidgetList>();

    private string m_SelectView = "";

    public string GetSelectView()
    {
        return m_SelectView;
    }

    public bool SetSelectView(string viewName)
    {
        WidgetList widgetList = GetWidget(viewName);
        if (widgetList == null)
        {
            return false;
        }

        m_SelectView = viewName;

        LoadChildrens();

        return true;
    }

    public WidgetList GetWidget(string name)
    {
        foreach (var widgetList in m_Widgets)
        {
            if (widgetList.Key == name)
            {
                return widgetList;
            }
        }

        return null;
    }

    private bool LoadChildrens()
    {
        var widgetList = GetWidget(m_SelectView);
        if (widgetList == null)
        {
            return false;
        }

        for (int i = 0; i < widgetList.List.Count; ++i)
        {
            widgetList.List[i].Apply();
        }

        return true;
    }

#if UNITY_EDITOR
    public bool Add(string name)
    {
        if (GetWidget(name) != null)
        {
            return false;
        }

        WidgetList widgetList = new WidgetList();
        widgetList.Key = name;
        widgetList.List = new List<Widget>();

        m_Widgets.Add(widgetList);

        SaveChildrens(widgetList);

        return true;
    }

    public void Remove(string name)
    {
        var widgetList = GetWidget(name);

        if (null == widgetList)
        {
            return;
        }

        m_Widgets.Remove(widgetList);
    }

    public void RemoveCurrent()
    {
        Remove(m_SelectView);
        m_SelectView = "";
    }

    public bool Save()
    {
        if (this == null || !this.gameObject.activeInHierarchy) //비활성화 상태에서 저장 방지
            return false;

        var widgetList = GetWidget(m_SelectView);
        if (widgetList == null)
        {
            return false;
        }

        return SaveChildrens(widgetList);
    }

    public void RemoveDeletedChildrens()
    {
        foreach (var widgetList in m_Widgets)
        {
            widgetList.List.RemoveAll(widget => widget.Object == null);
        }
    }

    private bool SaveChildrens(WidgetList widgetList)
    {
        widgetList.List.Clear();

        if (this == null || this.gameObject == null) //에디터에서 사용중일때 에러 방지
        {
            return false;
        }

        int childCount = this.gameObject.transform.childCount;
        for (int i = 0; i < childCount; ++i)
        {
            Transform child = this.gameObject.transform.GetChild(i);

            Widget widget = CreateWidget(child.gameObject);
            if (widget == null)
            {
                Debug.Log($"Null widget. name = {child.gameObject.name}");
                continue;
            }
            widgetList.List.Add(widget);
        }

        return true;
    }

    private Widget CreateWidget(GameObject obj)
    {
        var objTransform = obj.GetComponent<RectTransform>();
        if (objTransform == null)
        {
            return null;
        }

        Widget widget = new Widget();
        widget.Object = obj;
        widget.Active = obj.activeSelf;
        widget.AnchoredPosition = objTransform.anchoredPosition;
        widget.SizeDelta = objTransform.sizeDelta;

        return widget;
    }

    public string[] GetKeyArray()
    {
        List<string> keyNames = new List<string>();

        foreach (var w in m_Widgets)
        {
            keyNames.Add(w.Key);
        }

        return keyNames.ToArray();
    }
#endif
}

