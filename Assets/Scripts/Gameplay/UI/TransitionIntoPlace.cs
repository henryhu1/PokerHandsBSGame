using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(1000)]
public class TransitionIntoPlace : MonoBehaviour
{
    public enum MoveFromDirection { Left, Right, Up, Down }
    public static Dictionary<MoveFromDirection, Vector3> s_MoveDistances = new Dictionary<MoveFromDirection, Vector3>
    {
        { MoveFromDirection.Left, Vector3.left },
        { MoveFromDirection.Right, Vector3.right },
        { MoveFromDirection.Up, Vector3.up },
        { MoveFromDirection.Down, Vector3.down },
    };

    private Vector3 m_finalPosition;
    [SerializeField] private MoveFromDirection m_fromDirection;
    [SerializeField] private AnimationCurve movementCurve;
    [SerializeField] private float m_movementDuration;
    [SerializeField] private TriggerUITransition m_triggerUITransition;

    private void Awake()
    {
        m_finalPosition = transform.position;

        SetPositionToStarting();
        m_triggerUITransition.RegisterCallback(StartDoTransition);
        // InGameUI.Instance.OnShowUI += StartDoTransition;
    }

    private void SetPositionToStarting()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        float width = rectTransform.sizeDelta.x;
        float height = rectTransform.sizeDelta.y;
        Vector3 distance = new Vector3(width * 1.2f, height * 1.2f, 0);
        transform.position += Vector3.Scale(s_MoveDistances[m_fromDirection], distance);
    }

    private void StartDoTransition()
    {
        SetPositionToStarting();
        StartCoroutine(DoTransition());
    }

    private IEnumerator DoTransition()
    {
        Vector3 startPosition = transform.position;
        float time = 0f;

        while (time < m_movementDuration)
        {
            time += Time.deltaTime;

            float step = time / m_movementDuration;
            float curveStep = movementCurve.Evaluate(step);
            transform.position = Vector3.Lerp(startPosition, m_finalPosition, curveStep);

            yield return null;
        }

        transform.position = m_finalPosition;
    }

}
