using UnityEngine;
using UnityEngine.UI;

public class OrderCardsUI : RotatableUIBase
{
    public static OrderCardsUI Instance;

    [Header("UI")]
    [SerializeField] private Button m_orderButton;

    [Header("Firing Events")]
    [SerializeField] private VoidEventChannelSO OnOrderCards;

    private bool isPointingInAscendingDirection = true;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        m_orderButton.onClick.AddListener(() =>
        {
            if (!isPointingInAscendingDirection || PlayerCardsInHandManager.Instance.cardSortState != CardSortState.UNSORTED)
            {
                StartAnimation();
                isPointingInAscendingDirection = PlayerCardsInHandManager.Instance.cardSortState != CardSortState.ASCENDING;
            }
            OnOrderCards.RaiseEvent();
        });
    }

    private void OnDisable()
    {
        m_orderButton.onClick.RemoveAllListeners();
    }

    public void Show()
    {
        transform.eulerAngles = m_originalRotation;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
