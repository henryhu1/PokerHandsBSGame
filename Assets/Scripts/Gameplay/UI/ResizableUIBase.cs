using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public abstract class ResizableUIBase : MonoBehaviour, IAnimatable
{
    public static Dictionary<ResizingSide, Vector2> s_DirectionToScale = new()
    {
        { ResizingSide.Vertical, Vector2.up },
        { ResizingSide.Horizontal, Vector2.right },
        { ResizingSide.Both, Vector2.up + Vector2.right },
    };

    [SerializeField] private RectTransform resizingRect;
    [SerializeField] private ResizingSide sideToChange;
    [SerializeField] private Vector2 change;
    [SerializeField] private Ease easingFunction = Ease.OutCubic;
    [SerializeField] private float movementDuration;
    // [SerializeField] private TriggerUITransition triggerUITransition;
    private Vector2 originalSize;

    private Tween resizeTween;

    protected virtual void Awake()
    {
        originalSize = resizingRect.sizeDelta;

        // triggerUITransition.RegisterCallback(StartDoScale);
    }

    public Tween GetResizeTween()
    {
        Vector2 finalSize;
        if (resizingRect.sizeDelta == originalSize)
        {
            finalSize = Vector2.Scale(s_DirectionToScale[sideToChange], change);
        }
        else
        {
            finalSize = originalSize;
        }

        return resizingRect.DOSizeDelta(finalSize, movementDuration).SetEase(easingFunction);
    }

    public void StartAnimation()
    {
        StopAnimation();
        resizeTween = GetResizeTween();
        resizeTween.Play();
    }

    public void StopAnimation()
    {
        if (resizeTween != null && resizeTween.IsPlaying())
        {
            resizeTween.Kill();
            resizingRect.sizeDelta = originalSize;
        }
    }
}
