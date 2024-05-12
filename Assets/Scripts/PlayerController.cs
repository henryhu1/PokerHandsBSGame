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
