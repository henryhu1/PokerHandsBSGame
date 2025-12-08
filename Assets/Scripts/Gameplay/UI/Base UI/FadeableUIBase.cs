using DG.Tweening;
using TMPro;
using UnityEngine;

public class FadableUIBase : MonoBehaviour, IAnimatable
{
    [SerializeField] private float fadeInDuration;
    [SerializeField] private float freezeDuration = 1f;
    [SerializeField] private float fadeOutDuration;
    [SerializeField] private Ease easingFunction = Ease.OutCubic;
    [SerializeField] protected TextMeshProUGUI fadingText;
    private Sequence fadingSequence;

    private void Start()
    {
        fadingSequence = DOTween.Sequence();
        Tween fadeIn = ChangeTextColorAlpha(fadeInDuration, 1f);
        Tween fadeOut = ChangeTextColorAlpha(fadeOutDuration, 0f);

        fadingSequence.Append(fadeIn);
        fadingSequence.AppendInterval(freezeDuration);
        fadingSequence.Append(fadeOut);
        fadingSequence.OnComplete(() => fadingText.gameObject.SetActive(false));

        fadingSequence.Pause();
        fadingSequence.SetAutoKill(false);
    }

    protected virtual void OnEnable()
    {
        fadingText.gameObject.SetActive(false);
    }

    private void SetTextColorAlphaToStarting()
    {
        Color textColor = fadingText.color;
        textColor.a = 0f;
        fadingText.color = textColor;
    }

    private Tween ChangeTextColorAlpha(float duration, float targetOpacity)
    {
        Color targetColor = fadingText.color;
        targetColor.a = targetOpacity;
        return fadingText.DOColor(targetColor, duration).SetEase(easingFunction);
    }

    public void StartAnimation()
    {
        StopAnimation();

        fadingText.gameObject.SetActive(true);
        SetTextColorAlphaToStarting();

        fadingSequence.Restart();
        fadingSequence.Play();
    }

    public void StopAnimation()
    {
        fadingSequence.Pause();

        fadingText.gameObject.SetActive(false);
    }
}
