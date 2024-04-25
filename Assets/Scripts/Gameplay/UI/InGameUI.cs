using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameUI : TriggerUITransition
{
    public static InGameUI Instance;

    private void Awake()
    {
        Instance = this;

        // Hide();
    }

    private void Start()
    {
        CameraRotationLookAtTarget.Instance.OnCameraInPosition += Show;
    }

    private void Show()
    {
        gameObject.SetActive(true);
        DoTransition();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
