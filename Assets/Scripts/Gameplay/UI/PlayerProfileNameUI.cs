using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(1000)]
public class PlayerProfileNameUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField m_inputField;
    [SerializeField] private Button m_confirmButton;
    private static Regex s_noWhiteSpace = new Regex(@"\s+");

    private void Awake()
    {
        string playerName = LocalPlayerSaveSystem.LoadPlayerName();
        if (!string.IsNullOrEmpty(playerName))
        {
            m_inputField.text = playerName;
        }

        m_confirmButton.onClick.AddListener(() =>
        {
            AuthenticationManager.Instance.Authenticate(s_noWhiteSpace.Replace(m_inputField.text, ""));
        });
    }
}
