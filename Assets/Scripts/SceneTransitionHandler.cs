using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionHandler : MonoBehaviour
{
    public static SceneTransitionHandler Instance { get; private set; }

    [HideInInspector]
    public delegate void ClientLoadedSceneDelegateHandler(ulong clientId);
    [HideInInspector]
    public event ClientLoadedSceneDelegateHandler OnClientLoadedScene;
    [HideInInspector]
    public delegate void AllClientsLoadedSceneDelegateHandler();
    [HideInInspector]
    public event AllClientsLoadedSceneDelegateHandler OnAllClientsLoadedScene;
    [HideInInspector]
    public delegate void SceneStateChangedDelegateHandler(SceneStates newState);
    [HideInInspector]
    public event SceneStateChangedDelegateHandler OnSceneStateChanged;
    
    private int m_numberOfClientLoaded;

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

    public void RegisterCallbacks()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
    }

    public void UnregisterCallbacks()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadComplete;
    }

    public void SetSceneState(SceneStates sceneState)
    {
        m_SceneState = sceneState;
        OnSceneStateChanged?.Invoke(m_SceneState);
    }

    private void SwitchScene(string sceneName)
    {
        if (NetworkManager.Singleton.IsListening)
        {
            m_numberOfClientLoaded = 0;
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
        else
        {
            SceneManager.LoadSceneAsync(sceneName);
        }
    }

    public void SwitchToMainMenuScene() { SwitchScene(k_MainMenuScene); }

    public void SwitchToGameScene() { SwitchScene(k_InGameSceneName); }

    private void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
#if UNITY_EDITOR
        Debug.Log($"client #{clientId} has loaded scene {sceneName}");
#endif
        OnClientLoadedScene?.Invoke(clientId);
        m_numberOfClientLoaded += 1;
        if (m_numberOfClientLoaded == NetworkManager.Singleton.ConnectedClients.Count)
        {
            OnAllClientsLoadedScene?.Invoke();
        }
    }

    public bool AllClientsAreLoaded()
    {
        return m_numberOfClientLoaded == NetworkManager.Singleton.ConnectedClients.Count;
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
        OnClientLoadedScene = null;
        SetSceneState(SceneStates.MainMenu);
        SceneManager.LoadScene(k_MainMenuScene);
    }
}
