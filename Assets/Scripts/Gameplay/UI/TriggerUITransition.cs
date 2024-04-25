using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerUITransition : MonoBehaviour // TODO: is this necessary?
{
    [HideInInspector]
    public delegate void TriggerUITransitionDelegateHandler();
    [HideInInspector]
    public event TriggerUITransitionDelegateHandler OnTriggerUITransition;

    public void RegisterCallback(TriggerUITransitionDelegateHandler callback)
    {
        OnTriggerUITransition += callback;
    }

    protected void DoTransition()
    {
        OnTriggerUITransition?.Invoke();
    }
}
