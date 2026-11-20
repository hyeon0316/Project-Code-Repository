using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class TutorialOverlayUI : SingletonLoadAsset<TutorialOverlayUI>, GlobalEvent<TutorialContentsParam>.IEventHandler
{
    [SerializeField] private CanvasGroup m_CanvasGroup;
    [SerializeField] private TutorialHoleMesh m_HoleMesh;
    [SerializeField] private GameObject m_ClickAdvanceRoot;
    [SerializeField] private GameObject m_PageRoot;
    [SerializeField] private Text m_DescriptionText;
    [SerializeField] private Text m_PageIndicatorText;
    [SerializeField] private Button m_NextButton;
    [SerializeField] private GameObject m_CornerTextRoot;
    [SerializeField] private Text m_CornerText;
    [SerializeField] private Text m_ExtraText;

    private void Awake()
    {
        m_CanvasGroup.gameObject.SetActive(false);
        ContentsManager.Instance.Get<TutorialContents>().RegisterHandler(this);
    }

    private void OnDestroy()
    {
        ContentsManager.Instance.Get<TutorialContents>().UnRegisterHandler(this);
    }

    private void Show()
    {
        m_CanvasGroup.gameObject.SetActive(true);
        m_CanvasGroup.alpha = 0f;
        Tween.Alpha(m_CanvasGroup, 1f, 0.2f);
    }

    private void RefreshStep(TutorialContentsParam param)
    {
        var step = param.Step;
        RefreshHoleMesh(step, param.IsFirstStep);
        RefreshClickAdvanceArea(step);
        RefreshCornerText(step);
        RefreshExtraText(step);
        RefreshCenterPage(step, param.PageIndex);
    }

    private void RefreshHoleMesh(TutorialStep step, bool isFirstStep)
    {
        if (!step.UseHoleMesh)
        {
            m_HoleMesh.ClearHole();
            m_HoleMesh.gameObject.SetActive(false);
            return;
        }

        m_HoleMesh.gameObject.SetActive(true);
        var anchor = TutorialAnchor.FindRect(step.AnchorID);
        if (anchor != null)
        {
            Canvas.ForceUpdateCanvases();

            var corners = new Vector3[4];
            anchor.GetWorldCorners(corners);
            Vector2 worldCenter = (corners[0] + corners[2]) * 0.5f;
            float worldRadius = Vector2.Distance(corners[0], corners[2]) * 0.5f;
            m_HoleMesh.SetCircleHole(worldCenter, worldRadius, anchor, isFirstStep ? 0f : 0.25f);
        }
        else
        {
            m_HoleMesh.ClearHole();
            m_HoleMesh.gameObject.SetActive(false);
        }
    }

    private void RefreshClickAdvanceArea(TutorialStep step)
    {
        m_ClickAdvanceRoot.SetActive(step.UseAdvanceArea);
    }

    private void RefreshCornerText(TutorialStep step)
    {
        m_CornerTextRoot.SetActive(step.UseCornerText);
        if (step.UseCornerText)
            m_CornerText.text = step.CornerText;
    }

    private void RefreshExtraText(TutorialStep step)
    {
        m_ExtraText.gameObject.SetActive(step.UseExtraText);
        if (step.UseExtraText)
            m_ExtraText.text = step.ExtraText;
    }

    private void RefreshCenterPage(TutorialStep step, int pageIndex)
    {
        m_PageRoot.SetActive(step.UseCenterPage);
        if (step.UseCenterPage)
            RefreshPage(step, pageIndex);
    }

    private void RefreshPage(TutorialStep step, int pageIndex)
    {
        m_DescriptionText.text = step.Pages[pageIndex];

        bool hasMultiPage = step.Pages.Length > 1;
        m_PageIndicatorText.gameObject.SetActive(hasMultiPage);
        if (hasMultiPage)
            m_PageIndicatorText.text = $"{pageIndex + 1} / {step.Pages.Length}";

        m_NextButton.gameObject.SetActive(true);
    }

    public void OnClickEvent_AdvanceStep()
    {
        ContentsManager.Instance.Get<TutorialContents>().AdvancePage();
    }

    private void Hide()
    {
        Tween.Alpha(m_CanvasGroup, 0f, 0.2f).OnComplete(() =>
        {
            m_CanvasGroup.gameObject.SetActive(false);
            m_HoleMesh.ClearHole();
            m_ClickAdvanceRoot.SetActive(false);
            m_CornerTextRoot.SetActive(false);
            m_ExtraText.gameObject.SetActive(false);
            m_PageRoot.SetActive(false);
        });
    }

    public void OnEvent(TutorialContentsParam param)
    {
        switch (param.Type)
        {
            case TutorialContentsParam.EType.Show:
                Show();
                break;
            case TutorialContentsParam.EType.RefreshStep:
                RefreshStep(param);
                break;
            case TutorialContentsParam.EType.Hide:
                Hide();
                break;
        }
    }
}
