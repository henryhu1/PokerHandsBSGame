using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CreateLobbySettingTogglesUI : MonoBehaviour
{
    [SerializeField] private ToggleGroup m_toggleGroup;
    [SerializeField] private List<Toggle> m_toggles;

    public int GetActiveToggle()
    {
        IEnumerable<Toggle> selectedToggle = m_toggleGroup.ActiveToggles();
        return m_toggles.IndexOf(selectedToggle.First());
    }
}
