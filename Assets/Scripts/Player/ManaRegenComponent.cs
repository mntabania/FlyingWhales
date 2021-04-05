using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

public class ManaRegenComponent
{
    private Player m_player;

    private int m_manaPitCount = 0;

    public ManaRegenComponent(Player p_player) {
        m_player = p_player;
        Messenger.AddListener(Signals.HOUR_STARTED, OnHourStared);
        Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_OBJECT_PLACED, OnStructurePlaced);
        Messenger.AddListener<LocationStructure, Area>(StructureSignals.STRUCTURE_OBJECT_REMOVED, OnStructureDestroyed);
    }

    #region structure event listener(FOR MANA_PIT)
    void OnStructurePlaced(LocationStructure p_structure) {
        if (p_structure.structureType == STRUCTURE_TYPE.MANA_PIT) {
            EditableValuesManager.Instance.maximumMana += 250;
            m_manaPitCount++;
        }
    }

    void OnStructureDestroyed(LocationStructure p_structure, Area p_area) {
        if (p_structure.structureType == STRUCTURE_TYPE.MANA_PIT) {
            EditableValuesManager.Instance.maximumMana -= 250;
            if (m_player.mana > EditableValuesManager.Instance.maximumMana) {
                m_player.AdjustMana(EditableValuesManager.Instance.maximumMana - m_player.mana);
            }
            m_manaPitCount--;
        }
    }

    void OnHourStared() {
        if (m_player.mana < EditableValuesManager.Instance.maximumMana) {
            m_player.AdjustMana(40 + (10 * m_manaPitCount));
        }
    }
    #endregion
}
