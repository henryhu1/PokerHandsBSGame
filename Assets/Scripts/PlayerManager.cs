using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    private string localPlayerName;

    [Header("Listening Events")]
    [SerializeField] private StringEventChannelSO OnUpdatePlayerDisplayName;

    private void Awake()
    {
        if (Instance != this)
        {
            Destroy(Instance.gameObject);
        }

        Instance = this;

        string savedPlayerName = LocalPlayerSaveSystem.LoadPlayerName();
        if (!string.IsNullOrEmpty(savedPlayerName))
        {
            localPlayerName = savedPlayerName;
        }
    }

    private void OnEnable()
    {
        OnUpdatePlayerDisplayName.OnEventRaised += SetLocalPlayerName;
    }

    private void OnDisable()
    {
        OnUpdatePlayerDisplayName.OnEventRaised -= SetLocalPlayerName;
    }

    private void SetLocalPlayerName(string playerName)
    {
        localPlayerName = playerName;
    }

    public string GetLocalPlayerName()
    {
        return localPlayerName;
    }
}
