using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleSize : MonoBehaviour
{
    public enum ScaleDirection { Vertical, Horizontal, Both }
    public static Dictionary<ScaleDirection, Vector2> s_DirectionToScale = new Dictionary<ScaleDirection, Vector2>
    {
        { ScaleDirection.Vertical, Vector2.up },
        { ScaleDirection.Horizontal, Vector2.right },
        { ScaleDirection.Both, Vector2.up + Vector2.right },
    };

    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private ScaleDirection m_sideToScale;
    [SerializeField] private Vector2 m_scale;
    [SerializeField] private AnimationCurve movementCurve;
    [SerializeField] private float m_movementDuration;
    [SerializeField] private TriggerUITransition m_triggerUITransition;
    private Vector2 m_originalSize;
    private Vector2 m_scaleSize;

    private void Awake()
    {
        m_originalSize = rectTransform.sizeDelta;
        m_scaleSize = Vector2.Scale(s_DirectionToScale[m_sideToScale], m_scale);

        m_triggerUITransition.RegisterCallback(StartDoScale);
    }

    private void StartDoScale()
    {
        if (rectTransform.sizeDelta == m_originalSize)
        {
            StartCoroutine(DoTransition(m_scaleSize));
        }
        else
        {
            StartCoroutine(DoTransition(m_originalSize));
        }
    }

    private IEnumerator DoTransition(Vector2 finalSize)
    {
        Vector2 startingSize = rectTransform.sizeDelta;
        float time = 0f;

        while (time < m_movementDuration)
        {
            time += Time.deltaTime;

            float step = time / m_movementDuration;
            float curveStep = movementCurve.Evaluate(step);
            rectTransform.sizeDelta = Vector2.Lerp(startingSize, finalSize, curveStep);

            yield return null;
        }

        rectTransform.sizeDelta = finalSize;
    }
}
