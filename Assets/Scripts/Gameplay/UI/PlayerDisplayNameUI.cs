using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[DefaultExecutionOrder(1000)]
public class PlayerDisplayNameUI : MonoBehaviour
{

    [SerializeField] private TMP_InputField m_playerDisplayNameInputField;

    private void Awake()
    {
        m_playerDisplayNameInputField.onDeselect.AddListener((string playerName) =>
        {
            LobbyManager.Instance.UpdatePlayerName(playerName);
        });
    }

    private void Start()
    {
        LobbyListUI.Instance.OnCreatingNewLobby += LobbyListUI_OnCreatingNewLobby;
        LobbyCreateUI.Instance.OnCloseCreation += LobbyCreateUI_OnCloseCreation;
        LobbyManager.Instance.OnJoinedLobby += LobbyManager_OnJoinedLobby;
        LobbyManager.Instance.OnAuthenticated += LobbyManager_OnAuthenticated;
        // LobbyManager.Instance.OnGameStarted += LobbyManager_OnGameStarted;
        SceneTransitionHandler.Instance.OnSceneStateChanged += SceneTransitionHandler_OnSceneStateChanged;

        Hide();
    }
    private void LobbyListUI_OnCreatingNewLobby(object sender, EventArgs e)
    {
        Hide();
    }

    private void LobbyCreateUI_OnCloseCreation(object sender, EventArgs e)
    {
        Show();
    }

    private void LobbyManager_OnAuthenticated(string _)
    {
        Show();
    }

    private void LobbyManager_OnJoinedLobby(object sender, EventArgs e)
    {
        Show();
    }

    private void SceneTransitionHandler_OnSceneStateChanged(SceneTransitionHandler.SceneStates newState)
    {
        // TODO: investigate what happens when game finishes and returns to menu scene
        Hide();
    }
    // private void LobbyManager_OnGameStarted(object sender, EventArgs e)
    // {
    //     // TODO: investigate what happens when game finishes and returns to menu scene
    //     Hide();
    // }


    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Show()
    {
        m_playerDisplayNameInputField.text = LobbyManager.Instance.GetPlayerName();
        gameObject.SetActive(true);
    }
}
