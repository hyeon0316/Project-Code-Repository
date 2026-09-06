using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class TutorialHoleMesh : MaskableGraphic, ICanvasRaycastFilter
{
    private const int CIRCLE_SEGMENTS = 48;

    private Vector2 m_HoleCenter = Vector2.zero;
    private float m_HoleRadius = 0f;
    private bool m_HasHole = false;
    private Rect m_HoleClickRect = new Rect();
    private Tween m_Tween;

    public void SetCircleHole(Vector2 worldCenter, float worldRadius, RectTransform clickRectTarget, float duration = 0f)
    {
        Vector2 localCenter = rectTransform.InverseTransformPoint(worldCenter);
        float localRadius = worldRadius / rectTransform.lossyScale.x;
        m_HoleClickRect = CalcLocalClickRect(clickRectTarget);

        if (duration <= 0f)
        {
            m_HoleCenter = localCenter;
            m_HoleRadius = localRadius;
            m_HasHole = true;
            SetVerticesDirty();
            return;
        }

        Vector2 startCenter = m_HoleCenter;
        float startRadius = m_HoleRadius;
        m_Tween.Stop();
        m_Tween = Tween.Custom(this, 0f, 1f, duration, (self, t) =>
        {
            self.m_HoleCenter = Vector2.Lerp(startCenter, localCenter, t);
            self.m_HoleRadius = Mathf.Lerp(startRadius, localRadius, t);
            self.m_HasHole = true;
            self.SetVerticesDirty();
        });
    }

    public void ClearHole()
    {
        m_Tween.Stop();
        m_HasHole = false;
        m_HoleRadius = 0f;
        m_HoleClickRect = new Rect();
        SetVerticesDirty();
    }

    /// <summary>
    /// false이면 대상 anchor rect 안의 영역 클릭 (원은 시각 강조용, 클릭 통과는 rect 경계로 제한)
    /// </summary>
    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (!m_HasHole || m_HoleRadius <= 0f)
            return true;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, screenPoint, eventCamera, out var localPoint);
        return !m_HoleClickRect.Contains(localPoint);
    }

    private Rect CalcLocalClickRect(RectTransform target)
    {
        if (target == null)
            return new Rect();

        var corners = new Vector3[4];
        target.GetWorldCorners(corners);

        Vector2 min = rectTransform.InverseTransformPoint(corners[0]);
        Vector2 max = min;
        for (int i = 1; i < 4; i++)
        {
            Vector2 local = rectTransform.InverseTransformPoint(corners[i]);
            min = Vector2.Min(min, local);
            max = Vector2.Max(max, local);
        }

        return new Rect(min, max - min);
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect full = rectTransform.rect;

        if (!m_HasHole || m_HoleRadius <= 0f)
        {
            AddQuad(vh, full.min, full.max);
            return;
        }

        float outerR = (full.size.magnitude + m_HoleRadius) * 2f;

        for (int i = 0; i < CIRCLE_SEGMENTS; i++)
        {
            float a0 = 2f * Mathf.PI * i / CIRCLE_SEGMENTS;
            float a1 = 2f * Mathf.PI * (i + 1) / CIRCLE_SEGMENTS;

            Vector2 inner0 = m_HoleCenter + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * m_HoleRadius;
            Vector2 inner1 = m_HoleCenter + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * m_HoleRadius;
            Vector2 outer0 = m_HoleCenter + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * outerR;
            Vector2 outer1 = m_HoleCenter + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * outerR;

            int idx = vh.currentVertCount;
            AddVertex(vh, inner0);
            AddVertex(vh, inner1);
            AddVertex(vh, outer1);
            AddVertex(vh, outer0);
            vh.AddTriangle(idx, idx + 1, idx + 2);
            vh.AddTriangle(idx, idx + 2, idx + 3);
        }
    }

    private void AddVertex(VertexHelper vh, Vector2 pos)
    {
        var vert = new UIVertex();
        vert.position = pos;
        vert.color = color;
        vh.AddVert(vert);
    }

    private void AddQuad(VertexHelper vh, Vector2 min, Vector2 max)
    {
        int i = vh.currentVertCount;
        AddVertex(vh, new Vector2(min.x, min.y));
        AddVertex(vh, new Vector2(min.x, max.y));
        AddVertex(vh, new Vector2(max.x, max.y));
        AddVertex(vh, new Vector2(max.x, min.y));
        vh.AddTriangle(i, i + 1, i + 2);
        vh.AddTriangle(i, i + 2, i + 3);
    }
}
