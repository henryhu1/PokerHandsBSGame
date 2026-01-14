using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(1000)]
public class InputFieldModalUI : MonoBehaviour
{

    public static InputFieldModalUI Instance { get; private set; }

    [SerializeField] private GameObject uiItem;
    [SerializeField] private Button m_enterButton;
    [SerializeField] private Button m_cancelButton;
    [SerializeField] private TMP_InputField m_inputField;
    [SerializeField] private TMP_Text m_inputFieldPlaceholder;
    [SerializeField] private TextMeshProUGUI m_promptText;
    [SerializeField] private TextMeshProUGUI m_errorText;

    [Header("Listening Events")]
    [SerializeField] private VoidEventChannelSO OnCreatingNewLobby;

    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        Hide();
    }

    private void OnEnable()
    {
        OnCreatingNewLobby.OnEventRaised += Event_Close;
    }

    private void OnDisable()
    {
        OnCreatingNewLobby.OnEventRaised += Event_Close;
    }

    /* TODO
    / public void OnFailure(object sender, CustomGenericEventArgs.EventFailureArgs e)
    {
        m_errorText.text = e.failureString;
    }
    */

    private void Hide()
    {
        uiItem.SetActive(false);
    }

    public void Event_Close()
    {
        Hide();
    }

    private void Show(
        string promptString,
        int charLimit,
        string placeholder,
        string enterString,
        Action<string> onEnter,
        string cancelString,
        Action onCancel
    )
    {
        uiItem.SetActive(true);

        m_promptText.text = promptString;
        m_inputField.characterLimit = charLimit;
        m_inputField.text = "";
        m_inputFieldPlaceholder.text = placeholder;
        m_enterButton.GetComponentInChildren<TextMeshProUGUI>().text = enterString;
        m_cancelButton.GetComponentInChildren<TextMeshProUGUI>().text = cancelString;
        m_errorText.text = "";

        m_enterButton.onClick.AddListener(() =>
        {
            if (m_inputField.text != "")
            {
                onEnter(m_inputField.text);
                Hide();
            }
        });

        m_cancelButton.onClick.AddListener(() =>
        {
            onCancel();
            Hide();
        });
    }

    public static void Show_Static(
        string promptString,
        int charLimit,
        string placeholder,
        string enterString,
        Action<string> onEnter,
        string cancelString,
        Action onCancel
    )
    {
        Instance.Show(promptString, charLimit, placeholder, enterString, onEnter, cancelString, onCancel);
    }
}
