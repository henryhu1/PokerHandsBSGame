using DG.Tweening;
using UnityEngine;

public class RotatableUIBase : MonoBehaviour, IAnimatable
{
    [SerializeField] protected RectTransform rotatingRect;
    [SerializeField] private Ease easingFunction = Ease.OutCubic;
    [SerializeField] private float movementDuration;
    [SerializeField] private float rotationAmount;

    protected Vector3 originalRotation;
    protected Vector3 rotatedRotation;

    private Tween rotationTween;

    protected virtual void Awake()
    {
        originalRotation = rotatingRect.eulerAngles;
        rotatedRotation = rotatingRect.eulerAngles + new Vector3(0, 0, rotationAmount);
    }

    private Tween GetRotationTween()
    {
        Vector3 finalRotation;
        if (rotatingRect.eulerAngles == rotatedRotation)
        {
            finalRotation = originalRotation;
        }
        else
        {
            finalRotation = rotatedRotation;
        }

        return rotatingRect.DORotate(finalRotation, movementDuration).SetEase(easingFunction);
    }

    public void StartAnimation()
    {
        StopAnimation();
        rotationTween = GetRotationTween();
        rotationTween.Play();
    }

    public void StopAnimation()
    {
        if (rotationTween != null && rotationTween.IsPlaying())
        {
            rotationTween.Kill();
            rotatingRect.eulerAngles = originalRotation;
        }
    }
}
