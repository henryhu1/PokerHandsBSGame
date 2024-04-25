using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Start()
    {
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
