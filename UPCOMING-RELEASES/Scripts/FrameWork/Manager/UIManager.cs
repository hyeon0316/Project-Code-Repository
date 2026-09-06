using Cysharp.Threading.Tasks;
using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : SingletonLoadAsset<UIManager>
{
    public PageFactory PageSystem => m_PageSystem;
    public PopupFactory PopupSystem => m_PopupSystem;
    public Transform HudLayer => m_HudLayer;

    [SerializeField] private Transform m_PageLayer;
    [SerializeField] private Transform m_PopupLayer;
    [SerializeField] private Transform m_HudLayer;
    [SerializeField] private GameObject m_LoadingObj;
    [SerializeField] private GameObject m_LoadingWithBGObj;
    [SerializeField] private CanvasGroup m_ToastCanvasGroup;
    [SerializeField] private Text m_ToastMessage;
    [SerializeField] private AchievementToast m_AchievementToast;

    private Sequence m_ToastMsgSeq;
    private PopupFactory m_PopupSystem = new();
    private PageFactory m_PageSystem = new();


    private void Awake()
    {
        m_PopupSystem.SetParent(m_PopupLayer);
        m_PageSystem.SetParent(m_PageLayer);
        m_LoadingObj.SetActive(false);
        m_LoadingWithBGObj.SetActive(false);
        m_ToastCanvasGroup.alpha = 0;
    }

    private void Update()
    {
        m_PopupSystem.OnUpdate().Forget();
    }

    public void SetLoadingUI(ELoadingType type, bool isActive)
    {
        switch (type)
        {
            case ELoadingType.None:
                break;
            case ELoadingType.Center:
                m_LoadingObj.SetActive(isActive);
                break;
            case ELoadingType.Background:
                m_LoadingWithBGObj.SetActive(isActive);
                break;
        }
    }

    public void ShowToastMessage(string message)
    {
        if (m_ToastMsgSeq.isAlive)
            m_ToastMsgSeq.Stop();

        m_ToastCanvasGroup.alpha = 0f;
        m_ToastMessage.text = message;

        m_ToastMsgSeq = Sequence.Create()
            .Chain(Tween.Alpha(m_ToastCanvasGroup, 1f, 0.5f))
            .ChainDelay(1f)
            .Chain(Tween.Alpha(m_ToastCanvasGroup, 0f, 0.5f));
    }

    public void ShowAchievementToast(string title)
    {
        m_AchievementToast.Show(title);
    }

    public T CreateUIItem<T>(Transform parent, AssetID assetID) where T : ItemBehaviour
    {
        T item = AddressableBundleManager.Instance.AssetInstantiate<T>(assetID, parent);

        if (item == null)
        {
            Debug.LogError($"Item is null, {assetID.GetID()}");
            return null;
        }

        return item;
    }

#if UNITY_EDITOR
    [Space(20)]
    public string AssetPath;
    [TextArea] public string Query;

    [Button("Go to Test Page")]
    private void GoTestPage()
    {
        m_PageSystem.GoAsync(new AssetID(AssetPath), Query).Forget();
    }

    [Button("Back")]
    private void BackCurPage()
    {
        m_PageSystem.Back().Forget();
    }
#endif
}
