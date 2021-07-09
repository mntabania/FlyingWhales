using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;
using UtilityScripts;

public class CharacterStructureComponent : CharacterComponent {
    public ManMadeStructure workPlaceStructure { get; private set; } //Do not save this because the loading of this is handled in ManMadeStructure - LoadReferences

    public CharacterStructureComponent() {
    }
    public CharacterStructureComponent(SaveDataCharacterStructureComponent data) {
    }


    #region General
    public void SetWorkPlaceStructure(ManMadeStructure p_structure) {
        if (workPlaceStructure != p_structure) {
            workPlaceStructure = p_structure;
            if (HasWorkPlaceStructure()) {
                if (owner.partyComponent.hasParty) {
                    owner.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Party, owner, "Became a worker");
                }
            }
        }
    }
    public bool HasWorkPlaceStructure() {
        return workPlaceStructure != null;
    }
    public bool TryUnassignFromCurrentWorkStructureOnClassChange(Character p_character, string p_newClassName) {
        if (workPlaceStructure != null) {
            //if actor can no longer work at its current work structure given new class, unassign them
            CharacterClassData classData = CharacterManager.Instance.GetOrCreateCharacterClassData(p_newClassName);
            if (classData.workStructureType != workPlaceStructure.structureType) {
                return workPlaceStructure.RemoveAssignedWorker(p_character);
            }
        }
        return false;
    }
    #endregion

    #region Purchasing
    public LocationStructure GetPreferredBasicResourceStructure(Character p_character) {
        if (p_character.homeSettlement == null) { return null; }
        if (p_character.faction == null) { return null; }
        if (!p_character.homeSettlement.HasBasicResourceProducingStructure()) { return null; }
        
        if (HasWorkPlaceStructure() && workPlaceStructure.structureType.IsBasicResourceProducingStructureForFaction(p_character.faction.factionType.type)) {
            bool hasNeededResource = false;
            if (p_character.faction.factionType.type == FACTION_TYPE.Human_Empire) {
                hasNeededResource = p_character.structureComponent.workPlaceStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.STONE_PILE);
            } else if (p_character.faction.factionType.type == FACTION_TYPE.Elven_Kingdom) {
                hasNeededResource = p_character.structureComponent.workPlaceStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.WOOD_PILE);
            } else if (p_character.faction.factionType.type == FACTION_TYPE.Demon_Cult ||
                       p_character.faction.factionType.type == FACTION_TYPE.Lycan_Clan || p_character.faction.factionType.type == FACTION_TYPE.Vampire_Clan) {
                hasNeededResource = p_character.structureComponent.workPlaceStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.STONE_PILE) || p_character.structureComponent.workPlaceStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.WOOD_PILE);
            }
            if (hasNeededResource) {
                return p_character.structureComponent.workPlaceStructure;    
            }
        }
        
        List<LocationStructure> basicResourceProducingStructures = RuinarchListPool<LocationStructure>.Claim();
        if (p_character.faction.factionType.type == FACTION_TYPE.Human_Empire || p_character.faction.factionType.type == FACTION_TYPE.Demon_Cult ||
            p_character.faction.factionType.type == FACTION_TYPE.Lycan_Clan || p_character.faction.factionType.type == FACTION_TYPE.Vampire_Clan) {
            List<LocationStructure> mines = p_character.homeSettlement.GetStructuresOfType(STRUCTURE_TYPE.MINE);
            if (mines != null) { basicResourceProducingStructures.AddRange(mines); }    
        }
            
        if (p_character.faction.factionType.type == FACTION_TYPE.Elven_Kingdom || p_character.faction.factionType.type == FACTION_TYPE.Demon_Cult ||
            p_character.faction.factionType.type == FACTION_TYPE.Lycan_Clan || p_character.faction.factionType.type == FACTION_TYPE.Vampire_Clan) {
            List<LocationStructure> lumberyards = p_character.homeSettlement.GetStructuresOfType(STRUCTURE_TYPE.LUMBERYARD);
            if (lumberyards != null) { basicResourceProducingStructures.AddRange(lumberyards); }    
        }
        
        if (basicResourceProducingStructures.Count > 0) {
            //villagers can now buy from any basic resource producing structure and are required to pay regardless of situation.
            basicResourceProducingStructures.Shuffle();
            for (int i = 0; i < basicResourceProducingStructures.Count; i++) {
                LocationStructure structure = basicResourceProducingStructures[i];
                ManMadeStructure manMadeStructure = structure as ManMadeStructure;
                Assert.IsNotNull(manMadeStructure, $"Food producing structure is not Man made! {structure?.name}");
                bool hasNeededResource = false;
                if (p_character.faction.factionType.type == FACTION_TYPE.Human_Empire) {
                    hasNeededResource = manMadeStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.STONE_PILE);
                } else if (p_character.faction.factionType.type == FACTION_TYPE.Elven_Kingdom) {
                    hasNeededResource = manMadeStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.WOOD_PILE);
                } else if (p_character.faction.factionType.type == FACTION_TYPE.Demon_Cult ||
                           p_character.faction.factionType.type == FACTION_TYPE.Lycan_Clan || p_character.faction.factionType.type == FACTION_TYPE.Vampire_Clan) {
                    hasNeededResource = manMadeStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.STONE_PILE) || manMadeStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.WOOD_PILE);
                }
                if (hasNeededResource) {
                    RuinarchListPool<LocationStructure>.Release(basicResourceProducingStructures);
                    return manMadeStructure;
                }
            }
        }
        RuinarchListPool<LocationStructure>.Release(basicResourceProducingStructures);
        return null;
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataCharacterStructureComponent data) {
        //Currently N/A
    }
    #endregion
}

[System.Serializable]
public class SaveDataCharacterStructureComponent : SaveData<CharacterStructureComponent> {

    #region Overrides
    public override void Save(CharacterStructureComponent data) {

    }

    public override CharacterStructureComponent Load() {
        CharacterStructureComponent component = new CharacterStructureComponent(this);
        return component;
    }
    #endregion
}