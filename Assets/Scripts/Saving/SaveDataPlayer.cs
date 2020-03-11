using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDataPlayer {

    public int playerFactionID;
    public int playerAreaID;
    public int threat;
    public int mana;
    public int maxMana;
    public int manaRegen;

    public List<SaveDataMinion> minions;
    public List<SaveDataSummonSlot> summonSlots;
    public List<SaveDataArtifactSlot> artifactSlots;
    public List<SaveDataInterventionAbility> interventionAbilitySlots;
    //public List<SaveDataIntel> allIntel;
    public UnsummonedMinionData[] minionsToSummon;


    public int currentMinionLeaderID;

    public int maxSummonSlots;
    public int maxArtifactSlots;

    //public int invadingRegionID;

    public int currentDivineInterventionTick;
    //public int currentInterventionAbilityTimerTick;
    //public int currentNewInterventionAbilityCycleIndex;
    //public INTERVENTION_ABILITY interventionAbilityToResearch;
    //public bool isNotFirstResearch;

    public float constructionRatePercentageModifier;

    public void Save(Player player) {
        playerFactionID = player.playerFaction.id;
        playerAreaID = player.playerSettlement.id;
        mana = player.mana;
        //maxSummonSlots = player.maxSummonSlots;
        //maxArtifactSlots = player.maxArtifactSlots;
        //currentDivineInterventionTick = player.currentDivineInterventionTick;
        // minionsToSummon = player.minionsToSummon;
        constructionRatePercentageModifier = player.constructionRatePercentageModifier;
        //isNotFirstResearch = player.isNotFirstResearch;

        minions = new List<SaveDataMinion>();
        for (int i = 0; i < player.minions.Count; i++) {
            SaveDataMinion saveDataMinion = new SaveDataMinion();
            saveDataMinion.Save(player.minions[i]);
            minions.Add(saveDataMinion);
        }

        //summonSlots = new List<SaveDataSummonSlot>();
        //for (int i = 0; i < player.summons.Count; i++) {
        //    SaveDataSummonSlot data = new SaveDataSummonSlot();
        //    data.Save(player.summons[i]);
        //    summonSlots.Add(data);
        //}

        // artifactSlots = new List<SaveDataArtifactSlot>();
        // for (int i = 0; i < player.artifacts.Count; i++) {
        //     SaveDataArtifactSlot saveDataArtifact = new SaveDataArtifactSlot();
        //     saveDataArtifact.Save(player.artifacts[i]);
        //     artifactSlots.Add(saveDataArtifact);
        // }

        //currentMinionLeaderID = player.currentMinionLeader.character.id;

        interventionAbilitySlots = new List<SaveDataInterventionAbility>();
        for (int i = 0; i < player.interventionAbilitySlots.Length; i++) {
            SaveDataInterventionAbility saveDataInterventionAbility = new SaveDataInterventionAbility();
            saveDataInterventionAbility.Save(player.interventionAbilitySlots[i]);
            interventionAbilitySlots.Add(saveDataInterventionAbility);
        }

        //allIntel = new List<SaveDataIntel>();
        //for (int i = 0; i < player.allIntel.Count; i++) {
        //    Intel intel = player.allIntel[i];
        //    SaveDataIntel data = System.Activator.CreateInstance(System.Type.GetType("SaveData" + intel.GetType().ToString())) as SaveDataIntel;
        //    data.Save(intel);
        //    allIntel.Add(data);
        //}
        //if(player.isInvadingRegion) {
        //    invadingRegionID = player.invadingRegion.id;
        //} else {
        //    invadingRegionID = -1;
        //}

        //interventionAbilityToResearch = player.interventionAbilityToResearch;
        //currentInterventionAbilityTimerTick = player.currentInterventionAbilityTimerTick;
        //currentNewInterventionAbilityCycleIndex = player.currentNewInterventionAbilityCycleIndex;
    }
    public void Load() {
        PlayerManager.Instance.InitializePlayer(this);
        //PlayerManager.Instance.player.LoadDivineIntervention(this);
        PlayerManager.Instance.player.LoadIntels(this);
    }
    //public void LoadInvasion(Save save) {
    //    if (invadingRegionID != -1) {
    //        SaveDataRegion invadingRegionSave = null;
    //        for (int i = 0; i < save.regionSaves.Count; i++) {
    //            if (save.regionSaves[i].id == invadingRegionID) {
    //                invadingRegionSave = save.regionSaves[i];
    //                break;
    //            }
    //        }

    //        Region region = GridMap.Instance.GetRegionByID(invadingRegionSave.id);
    //        region.LoadInvasion(invadingRegionSave.ticksInInvasion);
    //    }
    //}
    private void SortAddSaveDataArtifact(SaveDataArtifactSlot newSaveData) {
        bool hasBeenInserted = false;
        for (int i = 0; i < artifactSlots.Count; i++) {
            SaveDataArtifactSlot currSaveData = artifactSlots[i];
            if (newSaveData.id < currSaveData.id) {
                artifactSlots.Insert(i, newSaveData);
                hasBeenInserted = true;
                break;
            }
        }
        if (!hasBeenInserted) {
            artifactSlots.Add(newSaveData);
        }
    }
}
