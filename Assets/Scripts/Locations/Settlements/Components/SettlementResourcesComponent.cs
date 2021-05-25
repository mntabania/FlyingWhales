using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UtilityScripts;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Character_Talents;

public class SettlementResourcesComponent : NPCSettlementComponent {

    public SettlementResourcesComponent() {

    }
    public SettlementResourcesComponent(SaveDataSettlementResourcesComponent data) {
 
    }

    #region General
    public int GetFoodSupplyCapacity() {
        int supply = 0;
        for (int i = 0; i < owner.residents.Count; i++) {
            Character c = owner.residents[i];
            if (!c.isDead && c.characterClass.IsFoodProducer()) {
                LocationGridTile gridTile = c.gridTileLocation;
                //If character is paralyzed, restrained or quarantined and is outside his home settlement, he should not be counted
                bool isAvailable = !(c.traitContainer.HasTrait("Paralyzed", "Restrained", "Quarantined") && gridTile != null && c.hasMarker && !gridTile.IsPartOfSettlement(owner));
                if (isAvailable) {
                    CharacterTalent foodTalent = c.talentComponent.GetTalent(CHARACTER_TALENT.Food);
                    switch (foodTalent.level) {
                        case 1:
                        case 2:
                            supply += 8;
                            break;
                        case 3:
                        case 4:
                            supply += 16;
                            break;
                        case 5:
                            supply += 24;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        return supply;
    }
    public int GetResourceSupplyCapacity() {
        int supply = 0;
        if (owner.owner?.factionType.type == FACTION_TYPE.Human_Empire) {
            int numOfResidents = GetNumOfResidentsThatHasClass("Miner");
        }
        return supply;
    }
    private int GetNumOfResidentsThatHasClass(string p_className) {
        int count = 0;
        for (int i = 0; i < owner.residents.Count; i++) {
            Character c = owner.residents[i];
            LocationGridTile gridTile = c.gridTileLocation;
            //If character is paralyzed, restrained or quarantined and is outside his home settlement, he should not be counted
            bool isAvailable = !(c.traitContainer.HasTrait("Paralyzed", "Restrained", "Quarantined") && gridTile != null && c.hasMarker && !gridTile.IsPartOfSettlement(owner));
            if (!c.isDead && isAvailable && c.characterClass.className == p_className) {
                count++;
            }
        }
        return count;
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataSettlementResourcesComponent saveDataNpcSettlement) {
    }
    #endregion
}

public class SaveDataSettlementResourcesComponent : SaveData<SettlementResourcesComponent> {

#region Overrides
    public override void Save(SettlementResourcesComponent data) {
    }

    public override SettlementResourcesComponent Load() {
        SettlementResourcesComponent component = new SettlementResourcesComponent(this);
        return component;
    }
#endregion
}