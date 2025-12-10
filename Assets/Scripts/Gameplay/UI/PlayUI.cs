using UnityEngine;

public class PlayUI : MonoBehaviour
{
    public static PlayUI Instance { get; private set; }

    [Header("UI Children")]
    [SerializeField] private ActionsUI actionsUI;
    [SerializeField] private HandSelectionUI handSelectionUI;
    [SerializeField] private HandTypeUI handTypeUI;
    [SerializeField] private OrderCardsUI orderCardsUI;

    private TransitionableUIBase actionsAnimatable;
    private TransitionableUIBase handSelectionAnimatable;
    private TransitionableUIBase handTypeAnimatable;

    [Header("Listening Events")]
    [SerializeField] private VoidEventChannelSO OnCameraInPosition;
    [SerializeField] private VoidEventChannelSO OnNextRoundStarting;
    [SerializeField] private VoidEventChannelSO OnRoundEnded;
    [SerializeField] private VoidEventChannelSO OnInitializeNewGame;

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
        actionsAnimatable = actionsUI.GetComponent<TransitionableUIBase>();
        handSelectionAnimatable = handSelectionUI.GetComponent<TransitionableUIBase>();
        handTypeAnimatable = handTypeUI.GetComponent<TransitionableUIBase>();

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
        OnRoundEnded.OnEventRaised += Hide;
        OnNextRoundStarting.OnEventRaised += Show;
        OnInitializeNewGame.OnEventRaised += Show;
    }

    private void OnDisable()
    {
        OnCameraInPosition.OnEventRaised -= Show;
        OnRoundEnded.OnEventRaised -= Hide;
        OnNextRoundStarting.OnEventRaised -= Show;
        OnInitializeNewGame.OnEventRaised -= Show;
    }

    public void Show()
    {
        if (GameManager.Instance.IsNotOut())
        {
            actionsAnimatable.StartAnimation();
            handSelectionAnimatable.StartAnimation();
            handTypeAnimatable.StartAnimation();
            orderCardsUI.Show();
        }
    }

    public void Hide()
    {
        if (GameManager.Instance.IsNotOut())
        {
            actionsAnimatable.StartAnimation();
            handSelectionAnimatable.StartAnimation();
            handTypeAnimatable.StartAnimation();
            orderCardsUI.Hide();
        }
    }
}
