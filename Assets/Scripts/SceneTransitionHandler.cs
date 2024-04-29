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
    private int m_numberOfClientsExpected;

    private const string k_StartMainMenuScene = "MainMenuScene";
    private const string k_InGameSceneName = "GameScene";

    public enum SceneStates
    {
        Start,
        InGame
    }

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

    public void SwitchToGameScene() { SwitchScene(k_InGameSceneName); }

    private void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        Debug.Log("Client has loaded scene");
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

    public bool IsInGameScene()
    {
        return m_SceneState == SceneStates.InGame;
    }

    public void ExitAndLoadStartMenu()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadComplete;
        OnClientLoadedScene = null;
        SetSceneState(SceneStates.Start);
        SceneManager.LoadScene(k_StartMainMenuScene);
    }
}
