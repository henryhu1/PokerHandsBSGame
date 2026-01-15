using System;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

[DefaultExecutionOrder(1000)]
public class PlayerDisplayNameUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField m_playerDisplayNameInputField;

    [Header("Listening Events")]
    [SerializeField] private IntEventChannelSO OnSceneStateChanged;
    [SerializeField] private VoidEventChannelSO OnCreatingNewLobby;
    [SerializeField] private VoidEventChannelSO OnCloseCreation;
    [SerializeField] private LobbyEventChannelSO OnJoinedLobby;
    [SerializeField] private VoidEventChannelSO OnGameStarted;
    [SerializeField] private VoidEventChannelSO OnGameFailedToStart;

    [Header("Firing Events")]
    [SerializeField] private StringEventChannelSO OnUpdatePlayerDisplayName;

    private void Awake()
    {
        m_playerDisplayNameInputField.onDeselect.AddListener(playerName =>
        {
            OnUpdatePlayerDisplayName.RaiseEvent(playerName);
        });
    }

    private void OnEnable()
    {
        m_playerDisplayNameInputField.text = PlayerManager.Instance.GetLocalPlayerName();

        OnCreatingNewLobby.OnEventRaised += LobbyListUI_OnCreatingNewLobby;
        OnCloseCreation.OnEventRaised += LobbyCreateUI_OnCloseCreation;
        OnJoinedLobby.OnEventRaised += LobbyManager_OnJoinedLobby;
        OnGameStarted.OnEventRaised += LobbyManager_GameStarted;
        OnGameFailedToStart.OnEventRaised += LobbyManager_GameFailedToStart;
        OnSceneStateChanged.OnEventRaised += SceneStateChanged;
    }

    private void OnDisable()
    {
        OnCreatingNewLobby.OnEventRaised -= LobbyListUI_OnCreatingNewLobby;
        OnCloseCreation.OnEventRaised -= LobbyCreateUI_OnCloseCreation;
        OnJoinedLobby.OnEventRaised -= LobbyManager_OnJoinedLobby;
        OnGameStarted.OnEventRaised -= LobbyManager_GameStarted;
        OnGameFailedToStart.OnEventRaised -= LobbyManager_GameFailedToStart;
        OnSceneStateChanged.OnEventRaised -= SceneStateChanged;
    }

    private void LobbyListUI_OnCreatingNewLobby()
    {
        Hide();
    }

    private void LobbyCreateUI_OnCloseCreation()
    {
        Show();
    }

    private void LobbyManager_OnJoinedLobby(Lobby lobby)
    {
        Show();
    }

    private void LobbyManager_GameStarted()
    {
        Hide();
    }

    private void LobbyManager_GameFailedToStart()
    {
        Show();
    }

    private void SceneStateChanged(int newState)
    {
        Hide();
    }


    private void Hide()
    {
        m_playerDisplayNameInputField.gameObject.SetActive(false);
    }

    private void Show()
    {
        m_playerDisplayNameInputField.gameObject.SetActive(true);
    }
}
