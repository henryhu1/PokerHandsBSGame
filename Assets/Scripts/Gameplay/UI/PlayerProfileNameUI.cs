using System;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(1000)]
public class PlayerProfileNameUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField m_inputField;
    [SerializeField] private Button m_confirmButton;
    private static Regex s_noWhiteSpace = new Regex(@"\s+");

    private void Awake()
    {
        m_confirmButton.onClick.AddListener(() =>
        {
            Authenticate(s_noWhiteSpace.Replace(m_inputField.text, ""));
        });
    }

    public async void Authenticate(string playerName)
    {
        try
        {
            InitializationOptions initializationOptions = new InitializationOptions();
            initializationOptions.SetProfile(playerName);

            await UnityServices.InitializeAsync(initializationOptions);

            AuthenticationService.Instance.SignedIn += () => {
#if UNITY_EDITOR
                Debug.Log("Signed in! " + AuthenticationService.Instance.PlayerId);
#endif
                GameManager.Instance.SetLocalPlayerId(AuthenticationService.Instance.PlayerId);
                GameManager.Instance.SetLocalPlayerName(playerName);
                SceneTransitionHandler.Instance.SetSceneState(SceneStates.MainMenu);
                SceneManager.LoadScene(SceneTransitionHandler.k_MainMenuScene);
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (Exception e)
        {
#if UNITY_EDITOR
            Debug.Log(e.Message);
#endif
        }
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
