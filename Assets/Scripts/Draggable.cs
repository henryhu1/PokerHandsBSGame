using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    Vector3 offset;
    Plane plane;
    BoxCollider boxCollider;

    public bool isBeingDragged;
    public int Index;

    public void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    public void Update()
    {
        if (isBeingDragged)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (plane.Raycast(ray, out float distance))
            {
                // Move the object to the new position, preserving the offset
                transform.position = ray.GetPoint(distance) + offset;
                PlayerCardsInHandManager.Instance.HandleCardDrag(transform.position);
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            boxCollider.enabled = true;
            isBeingDragged = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayerCardsInHandManager.Instance.HandleCardEnter(Index);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isBeingDragged = true;
        plane = new Plane(Vector3.forward, transform.position);

        // Raycast from the camera to the mouse position
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);

        if (plane.Raycast(ray, out float distance))
        {
            // Calculate the offset between the object's position and the hit point
            offset = transform.position - ray.GetPoint(distance);
        }
        PlayerCardsInHandManager.Instance.SetCardEmptySlotPosition(this);
        boxCollider.enabled = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isBeingDragged = false;
        PlayerCardsInHandManager.Instance.HandleCardEndDrag(Index);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isBeingDragged) return;
        PlayerCardsInHandManager.Instance.HandleCardExit(Index);
    }
}
