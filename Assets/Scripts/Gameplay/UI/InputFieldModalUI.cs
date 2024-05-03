using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(1000)]
public class InputFieldModalUI : MonoBehaviour
{

    public static InputFieldModalUI Instance { get; private set; }

    [SerializeField] private Button m_enterButton;
    [SerializeField] private Button m_cancelButton;
    [SerializeField] private TMP_InputField m_inputField;
    [SerializeField] private TMP_Text m_inputFieldPlaceholder;
    [SerializeField] private TextMeshProUGUI m_promptText;
    [SerializeField] private TextMeshProUGUI m_errorText;

    private void Awake()
    {
        Instance = this;

        Hide();
    }

    private void Start()
    {
        LobbyListUI.Instance.OnCreatingNewLobby += Event_Close;
    }

    /* TODO
    / public void OnFailure(object sender, CustomGenericEventArgs.EventFailureArgs e)
    {
        m_errorText.text = e.failureString;
    }
    */

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Event_Close(object sender, EventArgs e)
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
        gameObject.SetActive(true);

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
