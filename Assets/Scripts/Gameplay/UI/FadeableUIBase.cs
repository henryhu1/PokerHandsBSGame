using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class FadableUIBase : MonoBehaviour, IAnimatable
{
    [SerializeField] private float m_fadeInDuration;
    [SerializeField] private AnimationCurve m_fadeInCurve;
    [SerializeField] private float m_freezeDuration = 1f;
    [SerializeField] private float m_fadeOutDuration;
    [SerializeField] private AnimationCurve m_fadeOutCurve;
    [SerializeField] protected TextMeshProUGUI m_fadingText;
    private Coroutine m_fadingCoroutine;

    protected virtual void Awake()
    {
        SetTextColorColorAlphaToStarting();
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

    public IEnumerator DoAnimation()
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

        m_fadingCoroutine = null;
        gameObject.SetActive(false);
    }

    public void StartAnimation()
    {
        gameObject.SetActive(true);
        SetTextColorColorAlphaToStarting();
        StopAnimation();
        m_fadingCoroutine = StartCoroutine(DoAnimation());
    }

    public void StopAnimation()
    {
        if (m_fadingCoroutine != null)
        {
            StopCoroutine(m_fadingCoroutine);
            m_fadingCoroutine = null;
            gameObject.SetActive(false);
        }
    }
}
