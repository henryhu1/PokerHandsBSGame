using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatableUIBase : MonoBehaviour, IAnimatable
{
    [SerializeField] private AnimationCurve movementCurve;
    [SerializeField] private float m_movementDuration;
    [SerializeField] protected float m_rotationAmount;
    protected Vector3 m_originalRotation;
    protected Vector3 m_rotatedRotation;
    // protected Quaternion m_rotationStart; do not need this if StopAnimation() is not resetting rotation
    protected Coroutine m_rotatingCoroutine;

    protected virtual void Awake()
    {
        m_originalRotation = transform.eulerAngles;
        m_rotatedRotation = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z + m_rotationAmount);
    }

    public IEnumerator DoAnimation()
    {
        Vector3 rotationStart = transform.eulerAngles;
        Vector3 finalRotation = m_rotatedRotation;
        if (rotationStart == m_rotatedRotation)
        {
            finalRotation = m_originalRotation;
        }
        float time = 0f;

        while (time < m_movementDuration)
        {
            time += Time.deltaTime;

            float step = time / m_movementDuration;
            float curveStep = movementCurve.Evaluate(step);
            transform.eulerAngles = Vector3.Lerp(rotationStart, finalRotation, curveStep);

            yield return null;
        }

        m_rotatingCoroutine = null;
        transform.eulerAngles = finalRotation;
    }

    public void StartAnimation()
    {
        StopAnimation();
        m_rotatingCoroutine = StartCoroutine(DoAnimation());
    }

    public void StopAnimation()
    {
        if (m_rotatingCoroutine != null)
        {
            StopCoroutine(m_rotatingCoroutine);
            m_rotatingCoroutine = null;
        }
    }
}
