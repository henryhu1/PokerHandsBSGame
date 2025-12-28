using System.Collections.Generic;
using UnityEngine;

public class InvalidPlayMessageUI : FadableUIBase
{
    public static Dictionary<InvalidPlays, string> s_invalidPlayMessage = new()
    {
        { InvalidPlays.HandTooLow, "Must play a higher hand"},
        { InvalidPlays.FlushNotAllowed, "Cannot play flush now" },
    };

    [Header("Listening Events")]
    [SerializeField] private IntEventChannelSO OnInvalidPlay;

    protected override void OnEnable()
    {
        base.OnEnable();

        OnInvalidPlay.OnEventRaised += InvalidPlay;
    }

    private void OnDisable()
    {
        OnInvalidPlay.OnEventRaised -= InvalidPlay;
    }

    private void InvalidPlay(int invalidPlay)
    {
        fadingText.text = s_invalidPlayMessage[(InvalidPlays)invalidPlay];
        StartAnimation();
    }
}
