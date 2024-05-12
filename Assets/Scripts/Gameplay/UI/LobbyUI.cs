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
    private bool m_isCreatingGame;
    private Coroutine m_startingTextCoroutine;


    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        m_isCreatingGame = false;
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
            m_startGameButton.enabled = false;
            m_isCreatingGame = true;
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
        LobbyManager.Instance.OnGameFailedToStart += LobbyManager_OnGameFailedToStart;

        Hide();
    }

    private void OnDestroy()
    {
        LobbyManager.Instance.OnJoinedLobby -= UpdateLobby_Event;
        LobbyManager.Instance.OnJoinedLobbyUpdate -= UpdateLobby_Event;
        LobbyManager.Instance.OnLobbyGameTypeChanged -= UpdateLobby_Event;
        LobbyManager.Instance.OnLeftLobby -= LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnKickedFromLobby -= LobbyManager_OnLeftLobby;
        LobbyManager.Instance.OnGameStarted -= LobbyManager_OnGameStarted;
        LobbyManager.Instance.OnGameFailedToStart -= LobbyManager_OnGameFailedToStart;
    }

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
        m_gameStartingText.gameObject.SetActive(false);
    }

    private void LobbyManager_OnGameStarted(object sender, EventArgs e)
    {
        m_gameStartingText.gameObject.SetActive(true);
        m_startingTextCoroutine = StartCoroutine(UpdateStartingGameUI());
    }

    private void LobbyManager_OnGameFailedToStart()
    {
        StopCoroutine(m_startingTextCoroutine);
        m_isCreatingGame = false;
        m_startingTextCoroutine = null;
        m_gameStartingText.gameObject.SetActive(false);
        m_startGameButton.enabled = true;
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

        m_gameModeText.gameObject.SetActive(!LobbyManager.Instance.IsLobbyHost());

        m_changeGameModeButton.gameObject.SetActive(LobbyManager.Instance.IsLobbyHost());
        m_startGameButton.gameObject.SetActive(LobbyManager.Instance.IsLobbyHost());
        m_startGameButton.enabled = !m_isCreatingGame && lobby.Players.Count >= 2;

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