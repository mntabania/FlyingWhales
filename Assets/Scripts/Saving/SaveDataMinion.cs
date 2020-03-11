using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDataMinion {
    public int characterID;
    public int exp;
    public int indexDefaultSort;
    //public int unlockedInterventionSlots;
    //public List<SaveDataInterventionAbility> interventionAbilities;
    public SaveDataCombatAbility combatAbility;
    public int assignedRegionID;

    public List<string> traitsToAdd;
    public List<SPELL_TYPE> interventionAbilitiesToResearch;
    public int spellExtractionCount;

    public void Save(Minion minion) {
        characterID = minion.character.id;
        exp = minion.exp;
        indexDefaultSort = minion.indexDefaultSort;
        if (minion.isAssigned) {
            assignedRegionID = minion.assignedRegion.id;
        } else {
            assignedRegionID = -1;
        }
        spellExtractionCount = minion.spellExtractionCount;
        //unlockedInterventionSlots = minion.unlockedInterventionSlots;

        //interventionAbilities = new List<SaveDataInterventionAbility>();
        //for (int i = 0; i < minion.interventionAbilities.Length; i++) {
        //    if(minion.interventionAbilities[i] != null) {
        //        SaveDataInterventionAbility saveDataInterventionAbility = new SaveDataInterventionAbility();
        //        saveDataInterventionAbility.Save(minion.interventionAbilities[i]);
        //        interventionAbilities.Add(saveDataInterventionAbility);
        //    }
        //}

        combatAbility = new SaveDataCombatAbility();
        combatAbility.Save(minion.combatAbility);

        traitsToAdd = minion.traitsToAdd;
    }

    public void Load(Player player) {
        Minion minion = player.CreateNewMinion(this);
        if(assignedRegionID != -1) {
            Region region = GridMap.Instance.GetRegionByID(assignedRegionID);
            minion.SetAssignedRegion(region);
            region.SetAssignedMinion(minion);
        }
        player.AddMinion(minion);
    }
}
