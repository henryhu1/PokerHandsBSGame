using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(1000)]
public class PlayerProfileNameUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField m_inputField;
    [SerializeField] private Button m_confirmButton;

    private void Awake()
    {
        m_confirmButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.Authenticate(m_inputField.text);
        });
    }

    private void Start()
    {
        LobbyManager.Instance.OnAuthenticated += LobbyManager_OnAuthenticated;
    }

    private void LobbyManager_OnAuthenticated(string _)
    {
        Hide();
    }

    private void Hide()
    {
        LobbyManager.Instance.OnAuthenticated -= LobbyManager_OnAuthenticated;
        gameObject.SetActive(false);
    }
}
