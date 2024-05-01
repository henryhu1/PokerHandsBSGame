using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(1000)]
public class LobbyListUI : MonoBehaviour
{
    public static LobbyListUI Instance { get; private set; }

    // TODO: maybe refactor events to be delegate (arg type) instead of EventHandler
    public event EventHandler<EventArgs> OnCreatingNewLobby;

    // TODO: refactor template into prefab
    [SerializeField] private Transform m_lobbyListItemTemplate;
    [SerializeField] private Transform m_container;
    [SerializeField] private Button m_joinWithCodeButton;
    [SerializeField] private Button m_refreshButton;
    [SerializeField] private Button m_createLobbyButton;

    private void Awake()
    {
        Instance = this;

        m_lobbyListItemTemplate.gameObject.SetActive(false);

        m_refreshButton.onClick.AddListener(RefreshButtonClick);
        m_createLobbyButton.onClick.AddListener(CreateLobbyButtonClick);
        m_joinWithCodeButton.onClick.AddListener(() => {
            InputFieldModalUI.Show_Static("Join with code", 6, "Lobby code", "Join",
                (string joinCode) => {
                    LobbyManager.Instance.JoinLobbyByCode(joinCode);
                },
                "Cancel",
                () => { }
            );
        });
    }

    private void Start()
    {
        LobbyCreateUI.Instance.OnCloseCreation += LobbyCreateUI_OnCloseCreation;
        LobbyManager.Instance.OnAuthenticated += LobbyManager_OnAuthenticated;
        LobbyManager.Instance.OnLobbyListChanged += LobbyManager_OnLobbyListChanged;
        LobbyManager.Instance.OnJoinedLobby += LobbyManager_OnJoinedLobby;
        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnKickedFromLobby;
        // InputFieldFocusHandler.Instance.OnTextChanged += InputFieldFocusHandler_OnTextChanged;

        Hide();
    }

    /*
    private void OnEnable()
    {
        LobbyCreateUI.Instance.OnCloseCreation += LobbyCreateUI_OnCloseCreation;
        LobbyManager.Instance.OnLobbyListChanged += LobbyManager_OnLobbyListChanged;
        LobbyManager.Instance.OnJoinedLobby += LobbyManager_OnJoinedLobby;
        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnKickedFromLobby;
    }

    private void OnDisable()
    {
        LobbyManager.Instance.OnLobbyListChanged -= LobbyManager_OnLobbyListChanged;
        LobbyManager.Instance.OnJoinedLobby -= LobbyManager_OnJoinedLobby;
        LobbyManager.Instance.OnLeftLobby -= LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby -= LobbyManager_OnKickedFromLobby;
    }
    */

    private void LobbyCreateUI_OnCloseCreation(object sender, EventArgs e)
    {
        Show();
    }

    private void LobbyManager_OnAuthenticated(string _)
    {
        Show();
    }

    private void LobbyManager_OnKickedFromLobby(object sender, LobbyManager.LobbyEventArgs e)
    {
        Show();
    }

    private void LobbyManager_OnLeftLobby(object sender, EventArgs e)
    {
        Show();
    }

    private void LobbyManager_OnJoinedLobby(object sender, LobbyManager.LobbyEventArgs e)
    {
        Hide();
    }

    private void LobbyManager_OnLobbyListChanged(object sender, LobbyManager.OnLobbyListChangedEventArgs e)
    {
        UpdateLobbyList(e.lobbyList);
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
        OnCreatingNewLobby?.Invoke(this, EventArgs.Empty);
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