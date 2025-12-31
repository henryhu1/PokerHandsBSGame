using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionHandler : MonoBehaviour
{
    public static SceneTransitionHandler Instance { get; private set; }

    [Header("Firing Events")]
    [SerializeField] private UlongEventChannelSO OnClientLoadedScene;
    [SerializeField] private IntEventChannelSO OnSceneStateChanged;
    [SerializeField] private VoidEventChannelSO OnAllClientsLoadedScene;

    private int numberOfClientsLoadedInScene;

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
        OnSceneStateChanged.RaiseEvent((int)m_SceneState);
    }

    private void SwitchScene(string sceneName)
    {
        if (NetworkManager.Singleton.IsListening)
        {
            numberOfClientsLoadedInScene = 0;
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
        OnClientLoadedScene.RaiseEvent(clientId);
        numberOfClientsLoadedInScene += 1;
        if (numberOfClientsLoadedInScene == NetworkManager.Singleton.ConnectedClients.Count)
        {
            OnAllClientsLoadedScene.RaiseEvent();

            if (IsInMainMenuScene())
            {
                SetSceneState(SceneStates.InGame);
                SwitchToGameScene();
            }
        }
    }

    public bool AreAllClientsAreLoaded()
    {
        return numberOfClientsLoadedInScene == NetworkManager.Singleton.ConnectedClients.Count;
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
