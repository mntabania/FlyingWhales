using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

public class ManaRegenComponent
{
    private Player m_player;

    private int m_manaPitCount = 0;
    private int m_maxMana = 0; //TODO: Move this to player pag may time na

    public ManaRegenComponent(Player p_player) {
        m_player = p_player;
        m_maxMana = EditableValuesManager.Instance.maximumMana;
        SubscribeListeners();
    }
    public ManaRegenComponent(Player p_player, SaveDataManaRegenComponent data) {
        m_player = p_player;
        m_manaPitCount = data.manaPitCount;
        m_maxMana = data.maxMana;
        EditableValuesManager.Instance.maximumMana = m_maxMana;
        SubscribeListeners();
    }
    private void SubscribeListeners() {
        Messenger.AddListener(Signals.HOUR_STARTED, OnHourStared);
        Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_OBJECT_PLACED, OnStructurePlaced);
        Messenger.AddListener<LocationStructure, Area>(StructureSignals.STRUCTURE_OBJECT_REMOVED, OnStructureDestroyed);
    }
    

    #region structure event listener(FOR MANA_PIT)
    void OnStructurePlaced(LocationStructure p_structure) {
        if (p_structure.structureType == STRUCTURE_TYPE.MANA_PIT) {
            EditableValuesManager.Instance.maximumMana += EditableValuesManager.Instance.GetAdditionalMaxManaPerManaPit();
            m_maxMana = EditableValuesManager.Instance.maximumMana;
            m_manaPitCount++;
        }
    }

    void OnStructureDestroyed(LocationStructure p_structure, Area p_area) {
        if (p_structure.structureType == STRUCTURE_TYPE.MANA_PIT) {
            EditableValuesManager.Instance.maximumMana -= EditableValuesManager.Instance.GetAdditionalMaxManaPerManaPit();
            m_maxMana = EditableValuesManager.Instance.maximumMana;
            if (m_player.mana > EditableValuesManager.Instance.maximumMana) {
                m_player.AdjustMana(EditableValuesManager.Instance.maximumMana - m_player.mana);
            }
            m_manaPitCount--;
        }
    }

    public int GetManaPitCount() {
        return m_manaPitCount;
    }
    public int GetMaxMana() {
        return m_maxMana;
    }

    void OnHourStared() {
        if (m_player.mana < EditableValuesManager.Instance.maximumMana) {
            m_player.AdjustMana(EditableValuesManager.Instance.GetManaRegenPerHour() + (EditableValuesManager.Instance.GetManaRegenPerManaPit() * m_manaPitCount));
        }
    }
    #endregion
}

#region Save Data
public class SaveDataManaRegenComponent : SaveData<ManaRegenComponent> {
    public int manaPitCount;
    public int maxMana;
    public override void Save(ManaRegenComponent data) {
        base.Save(data);
        manaPitCount = data.GetManaPitCount();
        maxMana = data.GetMaxMana();
    }
}
#endregion