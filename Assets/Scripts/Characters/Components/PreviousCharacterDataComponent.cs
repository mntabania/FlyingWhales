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

    public /*ActualGoapNode*/ INTERACTION_TYPE previousActionNodeType { get; private set; } //Changed the previousCurrentActionNode to only the enum type so that the ActualGoapNode will be garbage collected since it no reference no longer holds it
    public JOB_TYPE previousJobType { get; private set; }

    /// <summary>
    /// The settlement that this character was part of when it died.
    /// Can be null.
    /// </summary>
    public NPCSettlement homeSettlementOnDeath => _homeSettlementOnDeath;

    public PreviousCharacterDataComponent() { }
    public PreviousCharacterDataComponent(SaveDataPreviousCharacterDataComponent data) {
        previousActionNodeType = data.previousActionNodeType;
        previousJobType = data.previousJobType;
    }

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

    #region Actions
    public void SetPreviousActionType(INTERACTION_TYPE p_type) {
        previousActionNodeType = p_type;
    }
    public void SetPreviousJobType(JOB_TYPE p_type) {
        previousJobType = p_type;
    }
    public bool IsPreviousJobOrActionReturnHome() {
        return previousActionNodeType.IsReturnHome() || previousJobType.IsReturnHome();
    }
    #endregion
}

public class SaveDataPreviousCharacterDataComponent : SaveData<PreviousCharacterDataComponent> {
    public string previousHomeStructureID;
    public string previousHomeSettlementID;
    public string previousFactionID;
    public string homeSettlementOnDeathID;
    public INTERACTION_TYPE previousActionNodeType;
    public JOB_TYPE previousJobType;

    #region Overrides
    public override void Save(PreviousCharacterDataComponent data) {
        previousHomeStructureID = data.previousHomeStructure?.persistentID ?? string.Empty;
        previousHomeSettlementID = data.previousHomeSettlement?.persistentID ?? string.Empty;
        previousFactionID = data.previousFaction?.persistentID ?? string.Empty;
        homeSettlementOnDeathID = data.homeSettlementOnDeath?.persistentID ?? string.Empty;
        previousActionNodeType = data.previousActionNodeType;
        previousJobType = data.previousJobType;
    }

    public override PreviousCharacterDataComponent Load() {
        PreviousCharacterDataComponent component = new PreviousCharacterDataComponent(this);
        return component;
    }
    #endregion
}
