using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]
public class PointerEffects : MonoBehaviour
{
    private EventTrigger eventTrigger;

    private EventTrigger.Entry hoverTrigger;
    private EventTrigger.Entry clickTrigger;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip clickClip;

    private void Awake()
    {
        eventTrigger = GetComponent<EventTrigger>();
    }

    private void Start()
    {
        hoverTrigger = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        hoverTrigger.callback.AddListener(OnMouseHover);

        clickTrigger = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerClick
        };
        clickTrigger.callback.AddListener(OnMouseClick);
    }

    private void OnEnable()
    {
        eventTrigger.triggers.Add(hoverTrigger);
        eventTrigger.triggers.Add(clickTrigger);
    }

    private void OnDisable()
    {
        eventTrigger.triggers.Clear();
    }

    private void OnMouseHover(BaseEventData data)
    {
        SoundFXPlayer.Instance.PlaySound(hoverClip);
    }

    private void OnMouseClick(BaseEventData data)
    {
        SoundFXPlayer.Instance.PlaySound(clickClip);
    }
}
