using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomGenericEventArgs : MonoBehaviour
{
    public class EventFailureArgs : EventArgs
    {
        public string failureString;
    }

}
