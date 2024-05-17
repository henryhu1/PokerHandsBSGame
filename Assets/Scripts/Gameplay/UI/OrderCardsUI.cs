using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrderCardsUI : RotatableUIBase
{
    public static OrderCardsUI Instance;

    [SerializeField] private Toggle m_orderToggle;

    [SerializeField]
    public delegate void OnOrderCardsDelegateHandler(bool isAscending);
    [SerializeField]
    public event OnOrderCardsDelegateHandler OnOrderCards;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        m_orderToggle.onValueChanged.AddListener((bool isOn) =>
        {
            OnOrderCards?.Invoke(isOn);
            StartAnimation();
        });
    }

    private void Start()
    {
        CameraRotationLookAtTarget.Instance.OnCameraInPosition += CameraRotationLookAtTarget_CameraInPosition;
        GameManager.Instance.OnEndOfRound += GameManager_EndOfRound;
        GameManager.Instance.OnPlayerLeft += GameManager_PlayerLeft;
        GameManager.Instance.OnNextRoundStarting += GameManager_NextRoundStarting;
        GameManager.Instance.OnRestartGame += GameManager_RestartGame;

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        CameraRotationLookAtTarget.Instance.OnCameraInPosition -= CameraRotationLookAtTarget_CameraInPosition;
        GameManager.Instance.OnEndOfRound -= GameManager_EndOfRound;
        GameManager.Instance.OnPlayerLeft -= GameManager_PlayerLeft;
        GameManager.Instance.OnNextRoundStarting -= GameManager_NextRoundStarting;
        GameManager.Instance.OnRestartGame -= GameManager_RestartGame;
    }

    private void GameManager_RestartGame()
    {
        Show();
    }

    private void GameManager_NextRoundStarting()
    {
        Show();
    }

    private void GameManager_PlayerLeft(string playerLeftName, List<bool> playedHandsPresent, List<PokerHand> allHandsInPlay)
    {
        Hide();
    }

    private void GameManager_EndOfRound(List<bool> _, List<PokerHand> __)
    {
        Hide();
    }

    private void CameraRotationLookAtTarget_CameraInPosition()
    {
        Show();
    }

    private void Show()
    {
        transform.eulerAngles = m_originalRotation;
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}