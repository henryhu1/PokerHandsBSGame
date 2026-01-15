using UnityEngine;

public class PlayUI : MonoBehaviour
{
    public static PlayUI Instance { get; private set; }

    [Header("UI Children")]
    [SerializeField] private ActionsUI actionsUI;
    [SerializeField] private HandSelectionUI handSelectionUI;
    [SerializeField] private OrderCardsUI orderCardsUI;

    private TransitionableUIBase actionsAnimatable;
    private TransitionableUIBase handSelectionAnimatable;

    [Header("Listening Events")]
    [SerializeField] private VoidEventChannelSO OnNextRoundStarting;
    [SerializeField] private VoidEventChannelSO OnRoundEnded;

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

        if (!GameManager.Instance.IsClientInPlay())
        {
            Hide();
        }
    }

    private void OnEnable()
    {
        OnRoundEnded.OnEventRaised += Hide;
        OnNextRoundStarting.OnEventRaised += Show;
    }

    private void OnDisable()
    {
        OnRoundEnded.OnEventRaised -= Hide;
        OnNextRoundStarting.OnEventRaised -= Show;
    }

    public void Show()
    {
        actionsAnimatable.TransitionOnToScreen();
        handSelectionAnimatable.TransitionOnToScreen();
        orderCardsUI.Show();
    }

    public void Hide()
    {
        actionsAnimatable.TransitionOffScreen();
        handSelectionAnimatable.TransitionOffScreen();
        orderCardsUI.Hide();
    }
}
