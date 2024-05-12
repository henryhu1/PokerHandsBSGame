using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ResizableUIBase : MonoBehaviour, IAnimatable
{
    public static Dictionary<ResizingSide, Vector2> s_DirectionToScale = new Dictionary<ResizingSide, Vector2>
    {
        { ResizingSide.Vertical, Vector2.up },
        { ResizingSide.Horizontal, Vector2.right },
        { ResizingSide.Both, Vector2.up + Vector2.right },
    };

    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private ResizingSide m_sideToChange;
    [SerializeField] private Vector2 m_change;
    [SerializeField] private AnimationCurve movementCurve;
    [SerializeField] private float m_movementDuration;
    // [SerializeField] private TriggerUITransition m_triggerUITransition;
    private Vector2 m_originalSize;
    private Vector2 m_changedSize;
    private Vector2 m_startingSize;
    private Coroutine m_resizingCoroutine;

    protected virtual void Awake()
    {
        m_originalSize = rectTransform.sizeDelta;
        m_startingSize = rectTransform.sizeDelta;
        m_changedSize = Vector2.Scale(s_DirectionToScale[m_sideToChange], m_change);

        // m_triggerUITransition.RegisterCallback(StartDoScale);
    }

    public IEnumerator DoAnimation()
    {
        m_startingSize = rectTransform.sizeDelta;
        Vector2 finalSize = m_originalSize;
        if (rectTransform.sizeDelta == m_originalSize)
        {
            finalSize = m_changedSize;
        }
        float time = 0f;

        while (time < m_movementDuration)
        {
            time += Time.deltaTime;

            float step = time / m_movementDuration;
            float curveStep = movementCurve.Evaluate(step);
            rectTransform.sizeDelta = Vector2.Lerp(m_startingSize, finalSize, curveStep);

            yield return null;
        }

        m_resizingCoroutine = null;
        rectTransform.sizeDelta = finalSize;
        m_startingSize = finalSize;
    }

    public void StartAnimation()
    {
        StopAnimation();
        m_resizingCoroutine = StartCoroutine(DoAnimation());
    }

    public void StopAnimation()
    {
        if (m_resizingCoroutine != null)
        {
            StopCoroutine(m_resizingCoroutine);
            m_resizingCoroutine = null;
            rectTransform.sizeDelta = m_startingSize;
        }
    }
}
