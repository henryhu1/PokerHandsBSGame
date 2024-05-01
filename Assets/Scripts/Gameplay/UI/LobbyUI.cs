using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(1000)]
public class LobbyUI : MonoBehaviour
{
    public static LobbyUI Instance { get; private set; }

    private const string LOBBY_CODE_PREFIX = "Code: ";

    private const string k_GameStartingPrefixText = "Game Starting";

    // TODO: refactor template into prefab
    [SerializeField] private Transform m_playerListItemTemplate;
    [SerializeField] private Transform m_container;
    [SerializeField] private TextMeshProUGUI m_lobbyNameText;
    [SerializeField] private TextMeshProUGUI m_lobbyJoinCodeText;
    [SerializeField] private TextMeshProUGUI m_playerCountText;
    [SerializeField] private TextMeshProUGUI m_gameModeText;
    [SerializeField] private TextMeshProUGUI m_changeGameModeButtonText;
    [SerializeField] private TextMeshProUGUI m_gameStartingText;
    [SerializeField] private Button m_changeGameModeButton;
    [SerializeField] private Button m_startGameButton;
    [SerializeField] private Button m_leaveLobbyButton;


    private void Awake()
    {
        Instance = this;

        m_playerListItemTemplate.gameObject.SetActive(false);
        m_lobbyJoinCodeText.gameObject.SetActive(false);
        m_gameStartingText.gameObject.SetActive(false);

        m_changeGameModeButton.enabled = true;

        m_leaveLobbyButton.onClick.AddListener(() => {
            LobbyManager.Instance.LeaveLobby();
        });

        m_changeGameModeButton.onClick.AddListener(() => {
            LobbyManager.Instance.ChangeGameType();
        });

        m_startGameButton.onClick.AddListener(() => {
            m_changeGameModeButton.enabled = false;
            LobbyManager.Instance.StartGame();
        });
    }

    private void Start()
    {
        LobbyManager.Instance.OnJoinedLobby += UpdateLobby_Event;
        LobbyManager.Instance.OnJoinedLobbyUpdate += UpdateLobby_Event;
        LobbyManager.Instance.OnLobbyGameTypeChanged += UpdateLobby_Event;
        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnGameStarted += LobbyManager_OnGameStarted;
        SceneTransitionHandler.Instance.OnSceneStateChanged += SceneTransitionHandler_OnSceneStateChanged;

        Hide();
    }

    /*
    private void OnEnable()
    {
        LobbyManager.Instance.OnJoinedLobby += UpdateLobby_Event;
        LobbyManager.Instance.OnJoinedLobbyUpdate += UpdateLobby_Event;
        LobbyManager.Instance.OnLobbyGameModeChanged += UpdateLobby_Event;
        LobbyManager.Instance.OnLeftLobby += LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby += LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnGameStarted += LobbyManager_OnGameStarted;
    }

    private void OnDisable()
    {
        LobbyManager.Instance.OnJoinedLobby -= UpdateLobby_Event;
        LobbyManager.Instance.OnJoinedLobbyUpdate -= UpdateLobby_Event;
        LobbyManager.Instance.OnLobbyGameModeChanged -= UpdateLobby_Event;
        LobbyManager.Instance.OnLeftLobby -= LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby -= LobbyManager_OnLeftLobby;
    }
    */

    private IEnumerator UpdateStartingGameUI()
    {
        float updateTime = 1f;
        while (m_gameStartingText.IsActive())
        {
            updateTime -= Time.deltaTime;
            if (updateTime < 0f)
            {
                if (m_gameStartingText.text.EndsWith("..."))
                {
                    m_gameStartingText.text = k_GameStartingPrefixText;
                }
                else
                {
                    m_gameStartingText.text += ".";
                }

                updateTime = 1f;
            }
            yield return null;
        }
        m_gameStartingText.text = k_GameStartingPrefixText;
    }

    private void SceneTransitionHandler_OnSceneStateChanged(SceneTransitionHandler.SceneStates newState)
    {
        Hide();
    }

    private void LobbyManager_OnGameStarted(object sender, EventArgs e)
    {
        // TODO: investigate what happens when game finishes and returns to menu scene
        m_gameStartingText.gameObject.SetActive(true);
        StartCoroutine(UpdateStartingGameUI());
    }

    private void LobbyManager_OnLeftLobby(object sender, EventArgs e)
    {
        ClearLobby();
        Hide();
    }

    private void UpdateLobby_Event(object sender, LobbyManager.LobbyEventArgs e)
    {
        UpdateLobby();
    }

    private void UpdateLobby()
    {
        UpdateLobby(LobbyManager.Instance.GetJoinedLobby());
    }

    private void UpdateLobby(Lobby lobby)
    {
        if (lobby.Data[LobbyManager.KEY_START_GAME].Value != "0") return;
        ClearLobby();

        if (m_container != null)
        {
            foreach (Player player in lobby.Players)
            {
                Transform playerListItemTransform = Instantiate(m_playerListItemTemplate, m_container);
                playerListItemTransform.gameObject.SetActive(true);
                LobbyPlayerListItemUI lobbyPlayerListItemUI = playerListItemTransform.GetComponent<LobbyPlayerListItemUI>();

                lobbyPlayerListItemUI.SetKickPlayerButtonVisible(
                    LobbyManager.Instance.IsLobbyHost() &&
                    player.Id != AuthenticationService.Instance.PlayerId // Don't allow kick self
                );

                lobbyPlayerListItemUI.UpdatePlayer(player);
            }
        }

        m_changeGameModeButton.gameObject.SetActive(LobbyManager.Instance.IsLobbyHost());
        m_changeGameModeButton.gameObject.SetActive(LobbyManager.Instance.IsLobbyHost());
        m_startGameButton.gameObject.SetActive(LobbyManager.Instance.IsLobbyHost());
        m_startGameButton.enabled = lobby.Players.Count >= 2;

        m_gameModeText.gameObject.SetActive(!LobbyManager.Instance.IsLobbyHost());

        m_lobbyNameText.text = lobby.Name;
        m_playerCountText.text = lobby.Players.Count + "/" + lobby.MaxPlayers;
        m_gameModeText.text = lobby.Data[LobbyManager.KEY_GAME_MODE].Value;
        m_changeGameModeButtonText.text = m_gameModeText.text;
        if (lobby.IsPrivate && lobby.LobbyCode != "")
        {
            m_lobbyJoinCodeText.gameObject.SetActive(true);
            m_lobbyJoinCodeText.text = LOBBY_CODE_PREFIX + lobby.LobbyCode;
        }
        else
        {
            m_lobbyJoinCodeText.text = "";
            m_lobbyJoinCodeText.gameObject.SetActive(false);
        }

        Show();
    }

    private void ClearLobby()
    {
        if (m_container != null)
        {
            foreach (Transform child in m_container)
            {
                if (child == m_playerListItemTemplate) continue;
                Destroy(child.gameObject);
            }
        }
    }

    private void Hide()
    {
        m_gameStartingText.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

}