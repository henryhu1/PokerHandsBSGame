using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(1000)]
public class LobbyListUI : MonoBehaviour
{
    public static LobbyListUI Instance { get; private set; }

    [SerializeField] private GameObject uiItem;
    // TODO: refactor template into prefab
    [SerializeField] private Transform m_lobbyListItemTemplate;
    [SerializeField] private Transform m_container;
    [SerializeField] private Button m_joinWithCodeButton;
    [SerializeField] private Button m_refreshButton;
    [SerializeField] private Button m_createLobbyButton;

    [Header("Firing Events")]
    [SerializeField] private VoidEventChannelSO OnCreatingNewLobby;

    [Header("Listening Events")]
    [SerializeField] private VoidEventChannelSO OnCloseCreation;
    [SerializeField] private LobbyListEventChannelSO OnLobbyListChanged;
    [SerializeField] private LobbyEventChannelSO OnJoinedLobby;
    [SerializeField] private VoidEventChannelSO OnLeftLobby;
    [SerializeField] private LobbyEventChannelSO OnKickedFromLobby;

    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        m_lobbyListItemTemplate.gameObject.SetActive(false);

        m_refreshButton.onClick.AddListener(RefreshButtonClick);
        m_createLobbyButton.onClick.AddListener(CreateLobbyButtonClick);
        m_joinWithCodeButton.onClick.AddListener(() => {
            InputFieldModalUI.Instance.Show("Join with code", 6, "Lobby code", "Join",
                joinCode =>
                {
                    LobbyManager.Instance.JoinLobbyByCode(joinCode);
                },
                "Cancel",
                () => { }
            );
        });
    }

    private void OnEnable()
    {
        OnCloseCreation.OnEventRaised += LobbyCreateUI_OnCloseCreation;
        OnLobbyListChanged.OnEventRaised += LobbyManager_OnLobbyListChanged;
        OnJoinedLobby.OnEventRaised += LobbyManager_OnJoinedLobby;
        OnLeftLobby.OnEventRaised += LobbyManager_OnLeftLobby;
        OnKickedFromLobby.OnEventRaised += LobbyManager_OnKickedFromLobby;
    }

    private void OnDisable()
    {
        OnCloseCreation.OnEventRaised -= LobbyCreateUI_OnCloseCreation;
        OnLobbyListChanged.OnEventRaised -= LobbyManager_OnLobbyListChanged;
        OnJoinedLobby.OnEventRaised -= LobbyManager_OnJoinedLobby;
        OnLeftLobby.OnEventRaised -= LobbyManager_OnLeftLobby;
        OnKickedFromLobby.OnEventRaised -= LobbyManager_OnKickedFromLobby;
    }

    private void LobbyCreateUI_OnCloseCreation()
    {
        Show();
    }

    private void LobbyManager_OnKickedFromLobby(Lobby lobby)
    {
        Show();
    }

    private void LobbyManager_OnLeftLobby()
    {
        Show();
    }

    private void LobbyManager_OnJoinedLobby(Lobby lobby)
    {
        Hide();
    }

    private void LobbyManager_OnLobbyListChanged(List<Lobby> lobbyList)
    {
        UpdateLobbyList(lobbyList);
    }

    private void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach (Transform child in m_container)
        {
            if (child == m_lobbyListItemTemplate) continue;

            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in lobbyList)
        {
            Transform lobbyListItemTransform = Instantiate(m_lobbyListItemTemplate, m_container);
            lobbyListItemTransform.gameObject.SetActive(true);
            LobbyListItemUI lobbyListItemUI = lobbyListItemTransform.GetComponent<LobbyListItemUI>();
            lobbyListItemUI.UpdateLobby(lobby);
        }
    }

    private void RefreshButtonClick()
    {
        LobbyManager.Instance.RefreshLobbyList();
    }

    private void CreateLobbyButtonClick()
    {
        Hide();
        OnCreatingNewLobby.RaiseEvent();
    }

    private void Hide()
    {
        uiItem.SetActive(false);
    }

    private void Show()
    {
        uiItem.SetActive(true);
    }
}