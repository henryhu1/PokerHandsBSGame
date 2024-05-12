using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvalidPlayMessageUI : FadableUIBase
{
    public static Dictionary<InvalidPlays, string> s_invalidPlayMessage = new Dictionary<InvalidPlays, string>
    {
        { InvalidPlays.HandTooLow, "Must play a higher hand"},
        { InvalidPlays.FlushNotAllowed, "Cannot play flush now" },
    };

    private void Start()
    {
        GameManager.Instance.OnInvalidPlay += GameManager_InvalidPlay;

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnInvalidPlay -= GameManager_InvalidPlay;
    }

    public void GameManager_InvalidPlay(InvalidPlays invalidPlay)
    {
        gameObject.SetActive(true);
        m_fadingText.text = s_invalidPlayMessage[invalidPlay];
        StartAnimation();
    }
}
