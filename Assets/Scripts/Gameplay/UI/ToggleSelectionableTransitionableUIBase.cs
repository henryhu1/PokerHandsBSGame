using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ToggleSelectionableTransitionableUIBase<T> : ToggleSelectionableUIBase<T>, IAnimatable
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private TransitionInDirection m_inDirection;
    [SerializeField] private AnimationCurve movementCurve;
    [SerializeField] private float m_movementDuration;
    [SerializeField] private float m_startDelay = 0f;
    [SerializeField] protected bool m_startOffScreen = true;
    private Vector3 m_originalPosition;
    private Vector3 m_offScreenPosition;
    private Vector3 m_transitionStartPosition;
    protected Coroutine m_transitioningCoroutine;

    protected abstract void RegisterForEvents();

    protected virtual void Awake()
    {
        m_originalPosition = transform.position;
        m_transitionStartPosition = transform.position;
        float width = rectTransform.sizeDelta.x;
        float height = rectTransform.sizeDelta.y;
        switch (m_inDirection)
        {
            case TransitionInDirection.Left:
                m_offScreenPosition = new Vector3(Screen.width + width / 2, transform.position.y, 0);
                break;
            case TransitionInDirection.Right:
                m_offScreenPosition = new Vector3(-width / 2, transform.position.y, 0);
                break;
            case TransitionInDirection.Up:
                m_offScreenPosition = new Vector3(transform.position.x, -height / 2, 0);
                break;
            case TransitionInDirection.Down:
                m_offScreenPosition = new Vector3(transform.position.x, Screen.height + height / 2, 0);
                break;
        }
        if (m_startOffScreen)
        {
            transform.position = m_offScreenPosition;
        }
    }

    protected virtual void Start()
    {
        gameObject.SetActive(!m_startOffScreen);
    }

    public IEnumerator DoAnimation()
    {
        m_transitionStartPosition = transform.position;
        Vector3 finalPosition = m_originalPosition;
        if (transform.position == m_originalPosition)
        {
            finalPosition = m_offScreenPosition;
        }
        else
        {
            yield return new WaitForSeconds(m_startDelay);
        }
        float time = 0f;

        while (time < m_movementDuration)
        {
            time += Time.deltaTime;

            float step = time / m_movementDuration;
            float curveStep = movementCurve.Evaluate(step);
            transform.position = Vector3.Lerp(m_transitionStartPosition, finalPosition, curveStep);

            yield return null;
        }

        m_transitioningCoroutine = null;
        transform.position = finalPosition;
        m_transitionStartPosition = finalPosition;
        if (transform.position == m_offScreenPosition)
        {
            gameObject.SetActive(false);
        }
    }

    public void StartAnimation()
    {
        StopAnimation();
        gameObject.SetActive(true);
        m_transitioningCoroutine = StartCoroutine(DoAnimation());
    }

    public void StopAnimation()
    {
        if (m_transitioningCoroutine != null)
        {
            StopCoroutine(m_transitioningCoroutine);
            m_transitioningCoroutine = null;
            transform.position = m_transitionStartPosition;
        }
    }
}
