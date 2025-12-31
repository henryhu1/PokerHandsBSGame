using System;
using System.Threading.Tasks;
using Steamworks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AuthenticationManager : MonoBehaviour
{
    public static AuthenticationManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }

        Instance = this;
    }

    public async void Authenticate(string playerName)
    {
        try
        {
            InitializationOptions initializationOptions = new();
            initializationOptions.SetProfile(playerName);

            await UnityServices.InitializeAsync(initializationOptions);

            AuthenticationService.Instance.SignedIn += () => {
#if UNITY_EDITOR
                Debug.Log("Signed in! " + AuthenticationService.Instance.PlayerId);
#endif
                LocalPlayerSaveSystem.SavePlayerName(playerName);
                SceneTransitionHandler.Instance.SetSceneState(SceneStates.MainMenu);
                SceneManager.LoadScene(SceneTransitionHandler.k_MainMenuScene);
            };

            string steamTicket = GetSteamTicket(playerName);
            if (!string.IsNullOrEmpty(steamTicket))
            {
                await AuthenticationService.Instance.SignInWithSteamAsync(steamTicket, SteamUser.GetSteamID().ToString());
            }
            else
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
        }
        catch (Exception e)
        {
#if UNITY_EDITOR
            Debug.Log(e.Message);
#endif
        }
    }

    public string GetSteamTicket(string playerName)
    {
        try
        {
            byte[] ticketBuffer = new byte[1024];

            SteamNetworkingIdentity identity = new();
            identity.SetSteamID(SteamUser.GetSteamID());

            HAuthTicket authTicket = SteamUser.GetAuthSessionTicket(
                ticketBuffer,
                ticketBuffer.Length,
                out uint ticketSize,
                ref identity
            );

            string steamTicket = BitConverter
                .ToString(ticketBuffer, 0, (int)ticketSize)
                .Replace("-", string.Empty);

            return steamTicket;
        }
        catch (Exception e)
        {
#if UNITY_EDITOR
            Debug.LogError($"Steam auth failed: {e}");
#endif
            return null;
        }
    }

    private async Task LinkWithSteamAsync(string ticket, string identity)
    {
        try
        {
            await AuthenticationService.Instance.LinkWithSteamAsync(ticket, identity);
            Debug.Log("Link is successful.");
        }
        catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
        {
            // Prompt the player with an error message.
            Debug.LogError("This user is already linked with another account. Log in instead.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Link failed.");
            Debug.LogException(ex);
        }
    }
}