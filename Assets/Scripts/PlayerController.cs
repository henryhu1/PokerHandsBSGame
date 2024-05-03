using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Camera GameCamera;

    public void HandleSelection()
    {
        Ray ray = GameCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            //the collider could be children of the unit, so we make sure to check in the parent
            // var unit = hit.collider.GetComponentInParent<Unit>();
            // m_Selected = unit;


            //check if the hit object have a IUIInfoContent to display in the UI
            //if there is none, this will be null, so this will hid the panel if it was displayed
            // var uiInfo = hit.collider.GetComponentInParent<UIMainScene.IUIInfoContent>();
            // UIMainScene.Instance.SetNewInfoContent(uiInfo);
            OpponentHand opponentHand;
            if (hit.collider.TryGetComponent(out opponentHand))
            {
                opponentHand.SetSelectedHand();
            }
            else
            {
                AllOpponentCards.Instance.UnselectAllOpponentHands();
            }
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleSelection();
        }
    }
}
