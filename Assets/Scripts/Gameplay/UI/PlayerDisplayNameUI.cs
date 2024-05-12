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
        m_playerDisplayNameInputField.text = GameManager.Instance.LocalPlayerName;

        m_playerDisplayNameInputField.onDeselect.AddListener((string playerName) =>
        {
            GameManager.Instance.SetLocalPlayerName(playerName);
            LobbyManager.Instance.UpdatePlayerName(playerName);
        });
    }

    private void Start()
    {
        LobbyListUI.Instance.OnCreatingNewLobby += LobbyListUI_OnCreatingNewLobby;
        LobbyCreateUI.Instance.OnCloseCreation += LobbyCreateUI_OnCloseCreation;
        LobbyManager.Instance.OnJoinedLobby += LobbyManager_OnJoinedLobby;
        LobbyManager.Instance.OnGameStarted += LobbyManager_GameStarted;
        LobbyManager.Instance.OnGameFailedToStart += LobbyManager_GameFailedToStart;
        SceneTransitionHandler.Instance.OnSceneStateChanged += SceneTransitionHandler_OnSceneStateChanged;
    }

    private void OnDestroy()
    {
        LobbyListUI.Instance.OnCreatingNewLobby -= LobbyListUI_OnCreatingNewLobby;
        LobbyCreateUI.Instance.OnCloseCreation -= LobbyCreateUI_OnCloseCreation;
        LobbyManager.Instance.OnJoinedLobby -= LobbyManager_OnJoinedLobby;
        LobbyManager.Instance.OnGameStarted -= LobbyManager_GameStarted;
        LobbyManager.Instance.OnGameFailedToStart -= LobbyManager_GameFailedToStart;
        SceneTransitionHandler.Instance.OnSceneStateChanged -= SceneTransitionHandler_OnSceneStateChanged;
    }

    private void LobbyListUI_OnCreatingNewLobby(object sender, EventArgs e)
    {
        Hide();
    }

    private void LobbyCreateUI_OnCloseCreation(object sender, EventArgs e)
    {
        Show();
    }

    private void LobbyManager_OnJoinedLobby(object sender, EventArgs e)
    {
        Show();
    }

    private void LobbyManager_GameStarted(object sender, EventArgs e)
    {
        Hide();
    }

    private void LobbyManager_GameFailedToStart()
    {
        Show();
    }

    private void SceneTransitionHandler_OnSceneStateChanged(SceneStates newState)
    {
        Hide();
    }


    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
}
