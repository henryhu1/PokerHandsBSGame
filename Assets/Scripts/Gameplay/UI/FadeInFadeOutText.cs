using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FadeInFadeOutText : MonoBehaviour
{
    [SerializeField] private float m_fadeInDuration;
    [SerializeField] private AnimationCurve m_fadeInCurve;
    [SerializeField] private float m_freezeDuration = 1f;
    [SerializeField] private float m_fadeOutDuration;
    [SerializeField] private AnimationCurve m_fadeOutCurve;
    [SerializeField] private TextMeshProUGUI m_fadingText;
    [SerializeField] private TriggerUITransition m_triggerUITransition;
    private Coroutine m_fadeCoroutine;

    private void Awake()
    {
        SetTextColorColorAlphaToStarting();
        gameObject.SetActive(false);

        m_triggerUITransition.RegisterCallback(StartDoFade);
    }

    private void SetTextColorColorAlphaToStarting()
    {
        Color textColor = m_fadingText.color;
        textColor.a = 0f;
        m_fadingText.color = textColor;
    }

    private void ChangeTextColorAlpha(float time, float duration, AnimationCurve curve, float targetOpacity)
    {
        float step = time / duration;
        float curveStep = curve.Evaluate(step);
        Color textColor = m_fadingText.color;
        textColor.a = Mathf.Lerp(textColor.a, targetOpacity, curveStep);
        m_fadingText.color = textColor;
    }

    private void StartDoFade()
    {
        StopDoFade();
        SetTextColorColorAlphaToStarting();
        gameObject.SetActive(true);
        m_fadeCoroutine = StartCoroutine(DoFade());
    }

    public void StopDoFade()
    {
        if (m_fadeCoroutine != null)
        {
            StopCoroutine(m_fadeCoroutine);
        }
        m_fadeCoroutine = null;
    }

    private IEnumerator DoFade()
    {
        float fadeInTime = 0.0f;
        while (fadeInTime < m_fadeInDuration)
        {
            fadeInTime += Time.deltaTime;
            ChangeTextColorAlpha(fadeInTime, m_fadeInDuration, m_fadeInCurve, 1f);
            yield return null;
        }

        yield return new WaitForSeconds(m_freezeDuration);

        float fadeOutTime = 0.0f;
        while (fadeOutTime < m_fadeOutDuration)
        {
            fadeOutTime += Time.deltaTime;
            ChangeTextColorAlpha(fadeOutTime, m_fadeOutDuration, m_fadeOutCurve, 0f);
            yield return null;
        }
    }

}
