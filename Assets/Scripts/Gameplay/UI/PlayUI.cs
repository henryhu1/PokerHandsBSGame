using UnityEngine;

public class PlayUI : MonoBehaviour
{
    public static PlayUI Instance { get; private set; }

    [Header("UI Children")]
    [SerializeField] private ActionsUI actionsUI;
    [SerializeField] private HandSelectionUI handSelectionUI;
    [SerializeField] private HandTypeUI handTypeUI;
    [SerializeField] private OrderCardsUI orderCardsUI;

    [Header("Listening Events")]
    [SerializeField] private VoidEventChannelSO OnCameraInPosition;

    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    private void Start()
    {
        GameManager.Instance.RegisterPlayUIObservers();
        if (!GameManager.Instance.IsNotOut())
        {
            Hide();
        }
    }

    private void OnDestroy()
    {
        GameManager.Instance.UnregisterPlayUIObservers();
    }

    private void OnEnable()
    {
        OnCameraInPosition.OnEventRaised += Show;
    }

    private void OnDisable()
    {
        OnCameraInPosition.OnEventRaised -= Show;
    }

    public void Show()
    {
        if (GameManager.Instance.IsNotOut())
        {
            actionsUI.StartAnimation();
            handSelectionUI.StartAnimation();
            handTypeUI.StartAnimation();
            orderCardsUI.Show();
        }
    }

    public void Hide()
    {
        if (GameManager.Instance.IsNotOut())
        {
            actionsUI.StartAnimation();
            handSelectionUI.StartAnimation();
            handTypeUI.StartAnimation();
            orderCardsUI.Hide();
        }
    }
}
