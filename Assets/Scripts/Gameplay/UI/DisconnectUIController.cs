using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class DisconnectUIController : MonoBehaviour
{
    [SerializeField] private Button disconnectButton;

    private void Start()
    {
        disconnectButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            SceneTransitionHandler.Instance.ExitAndLoadStartMenu();
        });
    }
}
