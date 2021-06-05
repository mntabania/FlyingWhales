using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UtilityScripts;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using JetBrains.Annotations;
using System.Linq;

public class SettlementStructureComponent : NPCSettlementComponent {
    public List<LocationStructure> linkedStructures { get; private set; }

    public SettlementStructureComponent() {
        linkedStructures = new List<LocationStructure>();
    }
    public SettlementStructureComponent(SaveDataSettlementStructureComponent data) {
        linkedStructures = new List<LocationStructure>();
    }

    #region Linked Structures
    public void AddLinkedStructure(LocationStructure p_structure) {
        if (!linkedStructures.Contains(p_structure)) {
            linkedStructures.Add(p_structure);
            p_structure.SetLinkedSettlement(owner);
        }
    }
    public void RemoveLinkedStructure(LocationStructure p_structure) {
        if (linkedStructures.Remove(p_structure)) {
            p_structure.SetLinkedSettlement(null);
        }
    }
    public void RelinkAllLinkedStructures() {
        for (int i = 0; i < linkedStructures.Count; i++) {
            LocationStructure s = linkedStructures[i];
            s.SetLinkedSettlement(null);
            s.LinkThisStructureToAVillage(owner);
        }
        linkedStructures.Clear();
    }
    public LocationStructure GetRandomLinkedStructureForExtermination() {
        LocationStructure chosenStructure = null;
        List<LocationStructure> pool = RuinarchListPool<LocationStructure>.Claim();
        for (int i = 0; i < linkedStructures.Count; i++) {
            LocationStructure s = linkedStructures[i];
            if (!s.hasBeenDestroyed && s.HasAliveMonsterRatmanOrUndeadResident()) {
                pool.Add(s);
            }
        }
        if (pool.Count > 0) {
            chosenStructure = pool[GameUtilities.RandomBetweenTwoNumbers(0, pool.Count - 1)];
        }
        RuinarchListPool<LocationStructure>.Release(pool);
        return chosenStructure;
    }
    public string GetLinkedStructuresSummary() {
        string log = string.Empty;
        for (int i = 0; i < linkedStructures.Count; i++) {
            if (i > 0) {
                log += ",";
            }
            log += linkedStructures[i].name;
        }
        return log;
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataSettlementStructureComponent data) {
        if (data.linkedStructures != null) {
            for (int i = 0; i < data.linkedStructures.Count; i++) {
                LocationStructure structure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentIDSafe(data.linkedStructures[i]);
                if (structure != null) {
                    linkedStructures.Add(structure);
                    structure.SetLinkedSettlement(owner);
                }
            }    
        }
    }
    #endregion

}

public class SaveDataSettlementStructureComponent : SaveData<SettlementStructureComponent> {
    public List<string> linkedStructures;
    #region Overrides
    public override void Save(SettlementStructureComponent data) {
        if (data.linkedStructures.Count > 0) {
            linkedStructures = new List<string>();
            for (int i = 0; i < data.linkedStructures.Count; i++) {
                linkedStructures.Add(data.linkedStructures[i].persistentID);
            }
        }
    }

    public override SettlementStructureComponent Load() {
        SettlementStructureComponent component = new SettlementStructureComponent(this);
        return component;
    }
#endregion
}
