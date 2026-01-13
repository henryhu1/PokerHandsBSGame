using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionHandler : MonoBehaviour
{
    public static SceneTransitionHandler Instance { get; private set; }

    [Header("Firing Events")]
    [SerializeField] private UlongEventChannelSO OnClientLoadedScene;
    [SerializeField] private IntEventChannelSO OnSceneStateChanged;
    [SerializeField] private StringEventChannelSO OnAllClientsLoadedScene;

    private readonly HashSet<ulong> loadedClients = new();

    public const string k_MainMenuScene = "MainMenuScene";
    public const string k_InGameSceneName = "GameScene";

    private SceneStates m_SceneState;

    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
        SetSceneState(SceneStates.Start);
        DontDestroyOnLoad(this);
    }

    public void RegisterNetworkCallbacks()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        loadedClients.Add(NetworkManager.Singleton.LocalClientId);

        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
    }

    public void UnregisterNetworkCallbacks()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        loadedClients.Clear();

        NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadComplete;
    }

    // TODO: is local scene state needed?
    public void SetSceneState(SceneStates sceneState)
    {
        m_SceneState = sceneState;
        OnSceneStateChanged.RaiseEvent((int)m_SceneState);
    }

    private void SwitchScene(string sceneName)
    {
        if (NetworkManager.Singleton.IsListening)
        {
            loadedClients.Clear();
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
        else
        {
            SceneManager.LoadSceneAsync(sceneName);
        }
    }

    private void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId)) return;
        if (sceneName != SceneManager.GetActiveScene().name) return;

#if UNITY_EDITOR
        Debug.Log($"client #{clientId} has loaded scene {sceneName}");
#endif
        OnClientLoadedScene.RaiseEvent(clientId);
        loadedClients.Add(clientId);
        if (AreAllClientsAreLoaded())
        {
            OnAllClientsLoadedScene.RaiseEvent(sceneName);

            if (IsInMainMenuScene())
            {
                SetSceneState(SceneStates.InGame);
                SwitchScene(k_InGameSceneName);
            }
        }
    }

    public bool AreAllClientsAreLoaded()
    {
        return loadedClients.Count == NetworkManager.Singleton.ConnectedClients.Count;
    }

    public bool IsInMainMenuScene()
    {
        return m_SceneState == SceneStates.MainMenu;
    }

    public bool IsInGameScene()
    {
        return m_SceneState == SceneStates.InGame;
    }

    public void ExitAndLoadStartMenu()
    {
        SetSceneState(SceneStates.MainMenu);
        SceneManager.LoadScene(k_MainMenuScene);
    }
}
