using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpponentHand : MonoBehaviour
{
    [SerializeField] private MeshRenderer m_meshRenderer;
    [SerializeField] private Material blankCardTexture;
    private static List<int> s_cardRemovalOrder = new List<int> { 1, 3, 2, 0, 4 };

    public void DisplayCards(int amount)
    {
        int materialsToRemove = m_meshRenderer.materials.Length - amount;
        Debug.Log($"opponent has {amount} cards, removing {materialsToRemove} materials");
        Material[] mats = m_meshRenderer.materials;
        for (int i = 0; i < m_meshRenderer.materials.Length; i++)
        {
            if (i < materialsToRemove)
            {
                mats[s_cardRemovalOrder[i]] = null;
            }
            else
            {
                mats[s_cardRemovalOrder[i]] = blankCardTexture;
            }
        }
        m_meshRenderer.materials = mats;
    }
}
