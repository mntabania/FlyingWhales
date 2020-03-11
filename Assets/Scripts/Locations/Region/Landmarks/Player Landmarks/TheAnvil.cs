using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Obsolete("Player landmarks should no longer be used, use the LocationStructure version instead.")]
public class TheAnvil : BaseLandmark {
    // public static readonly string Improved_Spells_1 = "Improved Spells I";
    // public static readonly string Improved_Summoning_1 = "Improved Summoning I";
    // public static readonly string Improved_Artifacts_1 = "Improved Artifacts I";
    // public static readonly string Improved_Spells_2 = "Improved Spells II";
    // public static readonly string Improved_Summoning_2 = "Improved Summoning II";
    // public static readonly string Improved_Artifacts_2 = "Improved Artifacts II";
    // //unimplemented
    // public static readonly string Faster_Invasion = "Faster Invasion";
    // public static readonly string Improved_Construction = "Improved Construction";
    // public static readonly string Increased_Mana_Capacity = "Increased Mana Capacity";
    // public static readonly string Increased_Mana_Regen = "Increased Mana Regen";
    //
    // public static readonly string Faster_Portal_Invocation = "Faster Portal Invocation";
    // public static readonly string Reduce_Spire_Cooldown = "Reduce Spire Cooldown";
    // public static readonly string Reduce_Eye_Cooldown = "Reduce Eye Cooldown";
    //
    // public string upgradeIdentifier { get; private set; }
    // public int currentUpgradeTick {
    //     get {
    //         return UtilityScripts.GameUtilities.GetTicksInBetweenDates(upgradeStartDate, GameManager.Instance.Today());
    //     }
    // }
    // private GameDate upgradeStartDate;
    // public GameDate dueDate { get; private set; }

    // public int upgradeDuration { get; private set; }
    public Dictionary<string, AnvilDynamicResearchData> dynamicResearchData { get; private set; }

    public TheAnvil(HexTile location, SaveDataLandmark data) : base(location, data) { }
    public TheAnvil(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) {
        // ConstructResearchData();
    }
//     public void LoadSavedData(SaveDataTheAnvil data) {
//         ConstructResearchData(data);
//     }
//
//     public void SetUpgradeIdentifier(string upgradeIdentifier) {
//         this.upgradeIdentifier = upgradeIdentifier;
//         upgradeDuration = GetUpgradeDuration(upgradeIdentifier);
//     }
//
//     public void StartUpgradeProcess() {
//         PlayerManager.Instance.player.AdjustMana(-LandmarkManager.Instance.anvilResearchData[upgradeIdentifier].manaCost);
//         upgradeStartDate = GameManager.Instance.Today();
//         dueDate = GameManager.Instance.Today().AddTicks(upgradeDuration);
//         int tickDiff = UtilityScripts.GameUtilities.GetTicksInBetweenDates(upgradeStartDate, dueDate);
//         Debug.Log("Tick diff between " + upgradeStartDate.ConvertToContinuousDaysWithTime() + " and " + dueDate.ConvertToContinuousDaysWithTime() + " is " + tickDiff.ToString());
//         SchedulingManager.Instance.AddEntry(dueDate, UpgradeDone, this);
//         TimerHubUI.Instance.AddItem("Research " + upgradeIdentifier, upgradeDuration, () => UIManager.Instance.ShowRegionInfo(this.tileLocation.region));
//     }
//     private void UpgradeDone() {
//         if (upgradeIdentifier == Improved_Spells_1 || upgradeIdentifier == Improved_Spells_2) {
//             for (int i = 0; i < PlayerManager.Instance.player.interventionAbilitySlots.Length; i++) {
//                 PlayerJobActionSlot slot = PlayerManager.Instance.player.interventionAbilitySlots[i];
//                 slot.SetLevel(LandmarkManager.Instance.anvilResearchData[upgradeIdentifier].effect);
//             }
//         } else if (upgradeIdentifier == Improved_Summoning_1 || upgradeIdentifier == Improved_Summoning_2) {
//             for (int i = 0; i < PlayerManager.Instance.player.summonSlots.Count; i++) {
//                 SummonSlot slot = PlayerManager.Instance.player.summonSlots[i];
//                 slot.SetLevel(LandmarkManager.Instance.anvilResearchData[upgradeIdentifier].effect);
//             }
//         } else if (upgradeIdentifier == Improved_Artifacts_1 || upgradeIdentifier == Improved_Artifacts_2) {
//             for (int i = 0; i < PlayerManager.Instance.player.artifactSlots.Count; i++) {
//                 ArtifactSlot slot = PlayerManager.Instance.player.artifactSlots[i];
//                 slot.SetLevel(LandmarkManager.Instance.anvilResearchData[upgradeIdentifier].effect);
//             }
//         } else if (upgradeIdentifier == Faster_Invasion) {
//             for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
//                 GridMap.Instance.allRegions[i].mainLandmark.SetInvasionTicks(Mathf.RoundToInt(GridMap.Instance.allRegions[i].mainLandmark.invasionTicks * 0.8f));
//             }
//         } else if (upgradeIdentifier == Improved_Construction) {
//             PlayerManager.Instance.player.SetConstructionRatePercentageModifier(-0.2f);
//         }
//         AnvilDynamicResearchData tempData = dynamicResearchData[upgradeIdentifier];
//         tempData.isResearched = true;
//         dynamicResearchData[upgradeIdentifier] = tempData;
//
//         UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), LandmarkManager.Instance.anvilResearchData[upgradeIdentifier].researchDoneNotifText, () => UIManager.Instance.ShowRegionInfo(this.tileLocation.region));
//         upgradeIdentifier = string.Empty;
//         tileLocation.region.assignedMinion.SetAssignedRegion(null);
//         tileLocation.region.SetAssignedMinion(null); //reset assigned minion
// //        UIManager.Instance.regionInfoUI.OnPlayerUpgradeDone();
//     }
//     public string GetUnavailabilityDescription(string upgrade) {
//         string text = string.Empty;
//         if (!dynamicResearchData[upgrade].isResearched) {
//             if (LandmarkManager.Instance.anvilResearchData[upgrade].preRequisiteResearch != string.Empty
//                 && !dynamicResearchData[LandmarkManager.Instance.anvilResearchData[upgrade].preRequisiteResearch].isResearched) {
//                 text = "Available only after " + LandmarkManager.Instance.anvilResearchData[upgrade].preRequisiteResearch + " has been researched.";
//             }
//         } else {
//             text = "Already researched.";
//         }
//         return text;
//     }
//
//     public override void DestroyLandmark() {
//         base.DestroyLandmark();
//         SchedulingManager.Instance.ClearAllSchedulesBy(this);
//         if (!string.IsNullOrEmpty(upgradeIdentifier)) {
//             TimerHubUI.Instance.RemoveItem("Research " + upgradeIdentifier);
//         }
//     }
//
//     #region Static
//     public static int GetUpgradeDuration(string upgrade) {
//         //if (upgrade == Improved_Spells_1) {
//         //    return GameManager.Instance.GetTicksBasedOnHour(8);
//         //} else if (upgrade == Improved_Summoning_1) {
//         //    return GameManager.Instance.GetTicksBasedOnHour(8);
//         //} else if (upgrade == Improved_Artifacts_1) {
//         //    return GameManager.Instance.GetTicksBasedOnHour(8);
//         //}
//         return GameManager.Instance.GetTicksBasedOnHour(LandmarkManager.Instance.anvilResearchData[upgrade].durationInHours);
//     }
//     public static string GetUpgradeDescription(string upgrade) {
//         //if (upgrade == Improved_Spells_1) {
//         //    return "Upgrade all your spells by 1 level.";
//         //} else if (upgrade == Improved_Summoning_1) {
//         //    return "Upgrade all your summon slots by 1 level.";
//         //} else if (upgrade == Improved_Artifacts_1) {
//         //    return "Upgrade all your artifact slots by 1 level.";
//         //}
//         return LandmarkManager.Instance.anvilResearchData[upgrade].description;
//     }
//     #endregion
//
//     #region Research Data
//     private void ConstructResearchData() {
//         dynamicResearchData = new Dictionary<string, AnvilDynamicResearchData>();
//         foreach (KeyValuePair<string, AnvilResearchData> kvp in LandmarkManager.Instance.anvilResearchData) {
//             //string initialUnavailabilityDescription = string.Empty;
//             //if(kvp.Value.preRequisiteResearch != string.Empty) {
//             //    initialUnavailabilityDescription = "Available only after " + kvp.Value.preRequisiteResearch + " has been researched.";
//             //}
//             dynamicResearchData.Add(kvp.Key, new AnvilDynamicResearchData() { isResearched = false, /*unavailabilityDescription = initialUnavailabilityDescription*/ });
//         }
//     }
//     private void ConstructResearchData(SaveDataTheAnvil data) {
//         dynamicResearchData = new Dictionary<string, AnvilDynamicResearchData>();
//         for (int i = 0; i < data.dynamicResearchDataKeys.Count; i++) {
//             dynamicResearchData.Add(data.dynamicResearchDataKeys[i], data.dynamicResearchDataValues[i]);
//         }
//     }
//     #endregion
}

public class SaveDataTheAnvil : SaveDataLandmark {
    //public int currentCooldownTick;
    public List<string> dynamicResearchDataKeys;
    public List<AnvilDynamicResearchData> dynamicResearchDataValues;


    public override void Save(BaseLandmark landmark) {
        base.Save(landmark);
        //TheAnvil anvil = landmark as TheAnvil;
        //dynamicResearchDataKeys = anvil.dynamicResearchData.Keys.ToList();
        //dynamicResearchDataValues = anvil.dynamicResearchData.Values.ToList();
        //currentCooldownTick = eye.currentCooldownTick;
    }
    public override void LoadSpecificLandmarkData(BaseLandmark landmark) {
        base.LoadSpecificLandmarkData(landmark);
        //TheAnvil anvil = landmark as TheAnvil;
        // anvil.LoadSavedData(this);
    }
}


public struct AnvilResearchData {
    //This is the value effect after research is done. Ex: in Improved Spells 1, the effect would be that the spell levels will be increased by 1, so the value of this is 1
    //This is only integer for now since all effects of research are numbers, i.e., levels, invasion rate, construction rate, max mana, mana regen rate
    //But this will be subject to change if there will be effects that are not numbers
    public int effect;
    public string description;
    public int manaCost;
    public int durationInHours;
    public string preRequisiteResearch; //The research required to be done in order for this research to be done. If empty, this there is no prerequisite
    public string researchDoneNotifText; //Text that will be shown in the notification after this research is done
}

//This data might change during run time unlike AnvilResearchData which is constant throughout
//This data must also be saved because we want to track its current state
[System.Serializable]
public struct AnvilDynamicResearchData {
    public bool isResearched; //Has this been researched already?
    //public string unavailabilityDescription; //Detailed text of why this is unavailable for research
}