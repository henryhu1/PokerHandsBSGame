using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(1000)]
public class TransitionIntoPlace : MonoBehaviour
{
    public enum InDirection { Left, Right, Up, Down }
    // public static Dictionary<InDirection, Vector3> s_DirectionToMove = new Dictionary<InDirection, Vector3>
    // {
        // { InDirection.Left, Vector3.left },
        // { InDirection.Right, Vector3.right },
        // { InDirection.Up, Vector3.up },
        // { InDirection.Down, Vector3.down },
    // };

    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private InDirection m_inDirection;
    [SerializeField] private AnimationCurve movementCurve;
    [SerializeField] private float m_movementDuration;
    [SerializeField] private List<TriggerUITransition> m_triggeringSubjects;
    [SerializeField] private bool m_startOffScreen = true;
    private Vector3 m_originalPosition;

    private void Awake()
    {
        m_originalPosition = transform.position;
        if (m_startOffScreen)
        {
            transform.position = GetOffScreenPosition();
        }

        foreach (TriggerUITransition triggerUITransition in m_triggeringSubjects) triggerUITransition.RegisterCallback(StartDoTransition);
        // InGameUI.Instance.OnShowUI += StartDoTransition;
    }

    private void StartDoTransition()
    {
        StartCoroutine(DoTransition());
    }

    private Vector3 GetOffScreenPosition()
    {
        float width = rectTransform.sizeDelta.x;
        float height = rectTransform.sizeDelta.y;
        switch (m_inDirection)
        {
            case InDirection.Left:
                return new Vector3(Screen.width + width / 2, transform.position.y, 0);
            case InDirection.Right:
                return new Vector3(-width / 2, transform.position.y, 0);
            case InDirection.Up:
                return new Vector3(transform.position.x, -height / 2, 0);
            case InDirection.Down:
                return new Vector3(transform.position.x, Screen.height + height / 2, 0);
            default:
                return m_originalPosition;
        }
    }

    private IEnumerator DoTransition()
    {
        Vector3 startPosition = transform.position;
        Vector3 finalPosition = m_originalPosition;
        if (transform.position == m_originalPosition)
        {
            finalPosition = GetOffScreenPosition();
        }
        float time = 0f;

        while (time < m_movementDuration)
        {
            time += Time.deltaTime;

            float step = time / m_movementDuration;
            float curveStep = movementCurve.Evaluate(step);
            transform.position = Vector3.Lerp(startPosition, finalPosition, curveStep);

            yield return null;
        }

        transform.position = finalPosition;
    }

}
