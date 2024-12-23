using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotationLookAtTarget : MonoBehaviour
{
    public static CameraRotationLookAtTarget Instance { get; private set; }

    public Transform target;
    [SerializeField] private AnimationCurve movementCurve;
    [SerializeField] private float m_movementDuration;

    [HideInInspector]
    public delegate void CameraInPositionDelegateHandler();
    [HideInInspector]
    // TODO: UI gets displayed on camera in position, instead should UI be displayed when cards are distributed?
    public event CameraInPositionDelegateHandler OnCameraInPosition;

    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(RotateCameraToTarget());
    }

    private IEnumerator RotateCameraToTarget()
    {
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.LookRotation(target.position - transform.position);
        float time = 0f;

        while (time < m_movementDuration)
        {
            time += Time.deltaTime;

            float step = time / m_movementDuration;
            float curveStep = movementCurve.Evaluate(step);
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, curveStep);

            yield return null;
        }

        OnCameraInPosition?.Invoke();
        transform.rotation = endRotation;
    }
}
