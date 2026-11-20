using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageFactory
{
    public PageBehaviour CurPage => m_HistoryPages.Count > 0 ? m_HistoryPages[m_HistoryPages.Count - 1] : null;
    public PageBehaviour PrevPage => m_HistoryPages.Count > 1 ? m_HistoryPages[m_HistoryPages.Count - 2] : null;

    private List<PageBehaviour> m_HistoryPages = new();
    private List<AssetID> m_HistoryAssetIDs = new();
    private Transform m_PageParent;
    private CanvasGroup m_CanvasGroup;

    public void SetParent(Transform parent)
    {
        m_PageParent = parent;
        m_CanvasGroup = parent.GetComponent<CanvasGroup>();
    }

    public UniTask<PageBehaviour> GoAsync(AssetID assetID, string query = "", ELoadingType loadingType = ELoadingType.Background, bool isPrevActive = false)
    {
        return GoAsync<PageBehaviour>(assetID, query, loadingType, isPrevActive);
    }

    public async UniTask<T> GoAsync<T>(AssetID assetID, string query = "",
        ELoadingType loadingType = ELoadingType.Background, bool isPrevActive = false) where T : PageBehaviour
    {
        if (m_HistoryAssetIDs.Contains(assetID))
            return null;

        PageBehaviour newPage = null;
        m_CanvasGroup.alpha = 0;
        using (new LoadingScope(loadingType))
        {
            newPage = AddressableBundleManager.Instance.AssetInstantiate<PageBehaviour>(assetID, m_PageParent);

            if (!string.IsNullOrEmpty(query))
            {
                PageQuery.SetQuery(query, newPage);
                await UniTask.Yield();
            }

            await UniTask.Yield();
            await newPage.OnCreate();

            m_HistoryPages.Add(newPage);
            m_HistoryAssetIDs.Add(assetID);

            await UniTask.Yield();
            await newPage.OnLoad();
        }

        if (PrevPage != null)
        {
            await UniTask.Yield();
            await PrevPage.OnPause();
            PrevPage.gameObject.SetActive(isPrevActive);
        }

        await UniTask.Yield();
        m_CanvasGroup.alpha = 1;
        await newPage.OnTransitionStart();

        await UniTask.Yield();
        await newPage.OnResume();

        return (T)newPage;
    }

    public async UniTask Back()
    {
        var curPage = CurPage;
        var prevPage = PrevPage;

        m_HistoryPages.RemoveAt(m_HistoryPages.Count - 1);
        m_HistoryAssetIDs.RemoveAt(m_HistoryAssetIDs.Count - 1);

        if (curPage != null)
        {
            await curPage.OnPause();
            curPage.gameObject.SetActive(false);
        }

        if (prevPage != null)
        {
            prevPage.gameObject.SetActive(true);
            await prevPage.OnResume();
        }

        if (curPage != null)
        {
            await curPage.OnFinish();
            GameObject.Destroy(curPage.gameObject);
        }
    }

    public async UniTask RemoveAllPage()
    {
        for (int i = 0; i < m_HistoryPages.Count; i++)
        {
            await m_HistoryPages[i].OnFinish();
            GameObject.Destroy(m_HistoryPages[i].gameObject);
        }
        m_HistoryPages.Clear();
        m_HistoryAssetIDs.Clear();
    }

    public void HideUI(bool isHide)
    {
        m_CanvasGroup.alpha = isHide ? 0 : 1;
        m_CanvasGroup.blocksRaycasts = !isHide;
    }
}
