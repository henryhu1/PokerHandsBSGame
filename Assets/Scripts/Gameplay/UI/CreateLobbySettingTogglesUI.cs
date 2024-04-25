using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateLobbySettingTogglesUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_settingText;
    [SerializeField] private ToggleGroup m_toggleGroup;
    [SerializeField] private List<Toggle> m_toggles;
    private Dictionary<Toggle, TextMeshProUGUI> m_togglesLabels;

    private void Awake()
    {
        m_togglesLabels = new Dictionary<Toggle, TextMeshProUGUI>();
        foreach (Toggle toggle in m_toggles)
        {
            m_togglesLabels.Add(toggle, toggle.GetComponentInChildren<TextMeshProUGUI>());
        }
    }

    public void SetSettingText(string text)
    {
        m_settingText.text = text;
    }

    public void SetToggleLabel(int index, string label)
    {
        m_togglesLabels[m_toggles[index]].text = label;
    }

    public int GetActiveToggle()
    {
        IEnumerable<Toggle> selectedToggle = m_toggleGroup.ActiveToggles();
        return m_toggles.IndexOf(selectedToggle.First());
    }
}
