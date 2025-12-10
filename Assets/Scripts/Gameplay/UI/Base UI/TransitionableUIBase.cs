using DG.Tweening;
using UnityEngine;

public class TransitionableUIBase : MonoBehaviour, IAnimatable
{
    [SerializeField] private TransitionInDirection m_inDirection;
    [SerializeField] private Ease easingFunction = Ease.OutCubic;
    [SerializeField] private float m_movementDuration;
    [SerializeField] private float m_startDelay = 0f;
    [SerializeField] private RectTransform transitioningRect;

    private Vector2 size;

    protected bool isOffScreen = true;
    protected Vector3 m_originalPosition;
    protected Sequence transitioningSequence;

    protected virtual void Awake()
    {
        m_originalPosition = transitioningRect.position;
        size = GetComponent<RectTransform>().sizeDelta;

        // InGameUI.Instance.OnShowUI += StartDoTransition;
    }

    private void Start()
    {
        transitioningRect.position = GetOffScreenPosition();
        isOffScreen = true;

        transitioningRect.gameObject.SetActive(false);
    }

    public bool IsOffScreen()
    {
        return isOffScreen;
    }

    private Vector3 GetOffScreenPosition()
    {
        float width = size.x;
        float height = size.y;
        return m_inDirection switch
        {
            TransitionInDirection.Left => new Vector3(Screen.width + width / 2, transform.position.y, 0),
            TransitionInDirection.Right => new Vector3(-width / 2, transform.position.y, 0),
            TransitionInDirection.Up => new Vector3(transform.position.x, -height / 2, 0),
            TransitionInDirection.Down => new Vector3(transform.position.x, Screen.height + height / 2, 0),
            _ => new Vector3(Screen.width * 2, Screen.height * 2, 0),
        };
    }   

    private Sequence GetTransitionSequence()
    {
        Sequence newSequence = DOTween.Sequence();

        if (m_startDelay > 0 && isOffScreen)
        {
            newSequence.AppendInterval(m_startDelay);
        }

        Vector3 finalPosition = isOffScreen ? m_originalPosition : GetOffScreenPosition();
        newSequence.Append(transitioningRect.DOMove(finalPosition, m_movementDuration).SetEase(easingFunction));

        newSequence.Pause();
        newSequence.OnComplete(() => {
            isOffScreen = !isOffScreen;
            if (isOffScreen)
            {
                transitioningRect.gameObject.SetActive(false);
            }
        });
        return newSequence;
    }

    public void StartAnimation()
    {
        StopAnimation();
        transitioningRect.gameObject.SetActive(true);
        transitioningSequence = GetTransitionSequence();
        transitioningSequence.Play();
    }

    public void StopAnimation()
    {
        if (transitioningSequence != null && transitioningSequence.IsPlaying())
        {
            transitioningSequence.Kill();
            transitioningRect.position = m_originalPosition;
            transitioningRect.gameObject.SetActive(false);
        }
    }
}
