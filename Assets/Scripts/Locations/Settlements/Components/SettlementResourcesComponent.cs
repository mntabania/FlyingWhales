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
            supply += c.classComponent.GetFoodSupplyCapacityValue();
        }
        return supply;
    }
    public int GetResourceSupplyCapacity() {
        int supply = 0;
        if (owner.owner?.factionType.type == FACTION_TYPE.Human_Empire) {
            for (int i = 0; i < owner.residents.Count; i++) {
                Character c = owner.residents[i];
                supply += c.classComponent.GetResourceSupplyCapacityValue("Miner");
            }
            //int numOfMiners = GetNumOfResidentsThatHasClass("Miner");
            //int minerCapacity = owner.occupiedVillageSpot.minerCapacity;
            //int minerMultiplier = Mathf.Min(numOfMiners, minerCapacity);

            //supply += minerMultiplier * 8;
        } else if (owner.owner?.factionType.type == FACTION_TYPE.Elven_Kingdom) {
            for (int i = 0; i < owner.residents.Count; i++) {
                Character c = owner.residents[i];
                supply += c.classComponent.GetResourceSupplyCapacityValue("Logger");
            }
            //int numOfLoggers = GetNumOfResidentsThatHasClass("Logger");
            //int loggerCapacity = owner.occupiedVillageSpot.loggerCapacity;
            //int loggerMultiplier = Mathf.Min(numOfLoggers, loggerCapacity);

            //supply += loggerMultiplier * 8;
        } else {
            for (int i = 0; i < owner.residents.Count; i++) {
                Character c = owner.residents[i];
                supply += c.classComponent.GetResourceSupplyCapacityValue("Miner");
                supply += c.classComponent.GetResourceSupplyCapacityValue("Logger");
            }
            //int numOfMiners = GetNumOfResidentsThatHasClass("Miner");
            //int minerCapacity = owner.occupiedVillageSpot.minerCapacity;
            //int numOfLoggers = GetNumOfResidentsThatHasClass("Logger");
            //int loggerCapacity = owner.occupiedVillageSpot.loggerCapacity;

            //int minerMultiplier = Mathf.Min(numOfMiners, minerCapacity);
            //int loggerMultiplier = Mathf.Min(numOfLoggers, loggerCapacity);
            //supply += ((minerMultiplier + loggerMultiplier) * 8);
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
    private int GetNumOfResidentsThatHasClass(string p_className1, string p_className2) {
        int count = 0;
        for (int i = 0; i < owner.residents.Count; i++) {
            Character c = owner.residents[i];
            LocationGridTile gridTile = c.gridTileLocation;
            //If character is paralyzed, restrained or quarantined and is outside his home settlement, he should not be counted
            bool isAvailable = !(c.traitContainer.HasTrait("Paralyzed", "Restrained", "Quarantined") && gridTile != null && c.hasMarker && !gridTile.IsPartOfSettlement(owner));
            if (!c.isDead && isAvailable && (c.characterClass.className == p_className1 || c.characterClass.className == p_className2)) {
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