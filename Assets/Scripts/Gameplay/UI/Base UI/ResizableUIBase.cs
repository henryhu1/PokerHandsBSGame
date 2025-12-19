using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ResizableUIBase : MonoBehaviour, IAnimatable
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
    private Vector2 resizedSize;

    private Tween resizeTween;

    protected virtual void Awake()
    {
        originalSize = resizingRect.sizeDelta;
        Vector2 scaled = Vector2.Scale(s_DirectionToScale[sideToChange], change);
        if (sideToChange == ResizingSide.Vertical)
        {
            resizedSize = new(originalSize.x, scaled.y);
        }
        else if (sideToChange == ResizingSide.Horizontal)
        {
            resizedSize = new(scaled.x, originalSize.y);
        }
        else
        {
            resizedSize = scaled;
        }

        // triggerUITransition.RegisterCallback(StartDoScale);
    }

    private Tween GetResizeTween()
    {
        Vector2 finalSize;
        if (resizingRect.sizeDelta == originalSize)
        {
            finalSize = resizedSize;
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
