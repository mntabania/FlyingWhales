using Inner_Maps.Location_Structures;
using UnityEngine;

public class PreviousCharacterDataComponent : CharacterComponent {

    private LocationStructure _previousHomeStructure;
    private NPCSettlement _previousHomeSettlement;
    private Faction _previousFaction;
    private NPCSettlement _homeSettlementOnDeath;
    
    public LocationStructure previousHomeStructure => _previousHomeStructure;
    public NPCSettlement previousHomeSettlement => _previousHomeSettlement;
    public Faction previousFaction => _previousFaction;
    /// <summary>
    /// The settlement that this character was part of when it died.
    /// Can be null.
    /// </summary>
    public NPCSettlement homeSettlementOnDeath => _homeSettlementOnDeath;

    public PreviousCharacterDataComponent() { }
    public PreviousCharacterDataComponent(SaveDataPreviousCharacterDataComponent data) { }

    #region Loading
    public void LoadReferences(SaveDataPreviousCharacterDataComponent data) {
        if (!string.IsNullOrEmpty(data.previousHomeStructureID)) {
            _previousHomeStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(data.previousHomeStructureID);
        }
        if (!string.IsNullOrEmpty(data.previousHomeSettlementID)) {
            _previousHomeSettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(data.previousHomeSettlementID) as NPCSettlement;
        }
        if (!string.IsNullOrEmpty(data.previousFactionID)) {
            _previousFaction = DatabaseManager.Instance.factionDatabase.GetFactionBasedOnPersistentID(data.previousFactionID);
        }
        if (!string.IsNullOrEmpty(data.homeSettlementOnDeathID)) {
            _homeSettlementOnDeath = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(data.homeSettlementOnDeathID) as NPCSettlement;
        }
    }
    #endregion
    
    #region Home Structure
    public void SetPreviousHomeStructure(LocationStructure p_structure) {
#if DEBUG_LOG
        Debug.Log($"Set Previous Home of {owner.name} to {p_structure?.name}");
#endif
        _previousHomeStructure = p_structure;
    }
    #endregion
    
    #region Home Settlement
    public void SetPreviousHomeSettlement(NPCSettlement p_settlement) {
        _previousHomeSettlement = p_settlement;
    }
    public void SetHomeSettlementOnDeath(NPCSettlement p_settlement) {
        _homeSettlementOnDeath = p_settlement;
    }
    #endregion

    #region Faction
    public void SetPreviousFaction(Faction p_faction) {
        _previousFaction = p_faction;
    }
    #endregion
}

public class SaveDataPreviousCharacterDataComponent : SaveData<PreviousCharacterDataComponent> {
    public string previousHomeStructureID;
    public string previousHomeSettlementID;
    public string previousFactionID;
    public string homeSettlementOnDeathID;

    #region Overrides
    public override void Save(PreviousCharacterDataComponent data) {
        previousHomeStructureID = data.previousHomeStructure?.persistentID ?? string.Empty;
        previousHomeSettlementID = data.previousHomeSettlement?.persistentID ?? string.Empty;
        previousFactionID = data.previousFaction?.persistentID ?? string.Empty;
        homeSettlementOnDeathID = data.homeSettlementOnDeath?.persistentID ?? string.Empty;
    }

    public override PreviousCharacterDataComponent Load() {
        PreviousCharacterDataComponent component = new PreviousCharacterDataComponent(this);
        return component;
    }
    #endregion
}
