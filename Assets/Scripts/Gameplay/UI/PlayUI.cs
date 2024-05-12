using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayUI : MonoBehaviour
{
    public static PlayUI Instance { get; private set; }

    [HideInInspector]
    public delegate void ShowPlayUIDelegateHandler();
    [HideInInspector]
    public event ShowPlayUIDelegateHandler OnShowPlayUI;

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

    public void Show()
    {
        gameObject.SetActive(true);
        OnShowPlayUI?.Invoke();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
