using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(1000)]
public class LobbyCreateUI : MonoBehaviour
{

    public static LobbyCreateUI Instance { get; private set; }

    // TODO: maybe refactor events to be delegate (arg type) instead of EventHandler
    public event EventHandler<EventArgs> OnCloseCreation;

    [SerializeField] private Button m_createButton;
    [SerializeField] private Button m_cancelButton;
    [SerializeField] private TMP_InputField m_lobbyNameInputField;
    [SerializeField] private CreateLobbySettingTogglesUI m_lobbyTypeSetting;
    [SerializeField] private CreateLobbySettingTogglesUI m_gameTypeSetting;
    // [SerializeField] private Button m_publicPrivateButton;
    // [SerializeField] private Button maxPlayersButton;
    // [SerializeField] private Button m_gameModeButton;
    // [SerializeField] private TextMeshProUGUI m_publicPrivateText;
    // [SerializeField] private TextMeshProUGUI maxPlayersText;
    // [SerializeField] private TextMeshProUGUI m_gameModeText;

    private string m_lobbyName;
    private int m_maxPlayers = 6;

    private void Awake()
    {
        Instance = this;

        SetToggleText<PokerHandsBullshitGame.GameType>(m_gameTypeSetting);
        SetToggleText<LobbyManager.LobbyType>(m_lobbyTypeSetting);

        m_createButton.onClick.AddListener(() => {
            LobbyManager.Instance.CreateLobby(
                m_lobbyName,
                m_maxPlayers,
                (LobbyManager.LobbyType)m_lobbyTypeSetting.GetActiveToggle(),
                (PokerHandsBullshitGame.GameType)m_gameTypeSetting.GetActiveToggle()
            );
            Hide();
        });

        m_cancelButton.onClick.AddListener(() => {
            Hide();
            OnCloseCreation?.Invoke(this, EventArgs.Empty);
        });

        m_lobbyNameInputField.text = m_lobbyName;
        m_lobbyNameInputField.onValueChanged.AddListener((string lobbyName) =>
        {
            m_lobbyName = lobbyName;
        });

        /* maxPlayersButton.onClick.AddListener(() => {
            UI_InputWindow.Show_Static("Max Players", maxPlayers,
            () => {
                // Cancel
            },
            (int maxPlayers) => {
                this.maxPlayers = maxPlayers;
                UpdateText();
            });
        }); */
    }

    private void Start()
    {
        LobbyListUI.Instance.OnCreatingNewLobby += LobbyListUI_OnCreatingNewLobby;

        Hide();
    }

    private void LobbyListUI_OnCreatingNewLobby(object sender, EventArgs e)
    {
        Show();
    }

    private static void SetToggleText<T>(CreateLobbySettingTogglesUI settingToggles) where T : Enum
    {
        string settingName = typeof(T).Name;
        settingToggles.SetSettingText(StringUtils.SplitCapitals(settingName));

        foreach (var value in Enum.GetValues(typeof(T)))
        {
            settingToggles.SetToggleLabel((int)value, value.ToString());
        }
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Show()
    {
        gameObject.SetActive(true);

        m_lobbyName = "";
        m_lobbyNameInputField.text = m_lobbyName;
    }
}