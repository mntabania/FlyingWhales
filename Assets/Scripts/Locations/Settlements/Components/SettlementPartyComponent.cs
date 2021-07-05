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

public class SettlementPartyComponent : NPCSettlementComponent {
    public GameDate scheduleDateForProcessingOfPartyQuests { get; private set; }

    public SettlementPartyComponent() {
    }
    public SettlementPartyComponent(SaveDataSettlementPartyComponent data) {
        scheduleDateForProcessingOfPartyQuests = data.scheduleDateForProcessingOfPartyQuests;
    }

    #region Party Quests
    public void InitialScheduleProcessingOfPartyQuests() {
        if (owner.locationType == LOCATION_TYPE.VILLAGE) {
            int minimumTick = GameManager.Instance.GetTicksBasedOnHour(2); //2 AM in ticks
            int maximumTick = GameManager.Instance.GetTicksBasedOnHour(5); //5 AM in ticks

            int scheduledTick = GameUtilities.RandomBetweenTwoNumbers(minimumTick, maximumTick);
            GameDate schedule = GameManager.Instance.Today().AddDays(1);
            schedule.SetTicks(scheduledTick);
            scheduleDateForProcessingOfPartyQuests = schedule;
            SchedulingManager.Instance.AddEntry(scheduleDateForProcessingOfPartyQuests, ProcessingOfPartyQuests, null);
        }
    }
    private void ProcessingOfPartyQuests() {
        if (owner.HasResidentThatIsNotDead()) {
            ProcessPartyQuests();
        }
        scheduleDateForProcessingOfPartyQuests = GameManager.Instance.Today().AddDays(1);
        SchedulingManager.Instance.AddEntry(scheduleDateForProcessingOfPartyQuests, ProcessingOfPartyQuests, null);
    }
    private void ProcessPartyQuests() {
        string log = string.Empty;

        int villagePartyCount = owner.GetPartyCount();
#if DEBUG_LOG
        log = GameManager.Instance.TodayLogString() + owner.name + " will process party quests";
        log += "\nParty Count: " + villagePartyCount;
#endif
        Faction factionOwner = owner.owner;
        if (factionOwner != null) {
            //Exploration
#if DEBUG_LOG
            log += "\nWill try Explore";
#endif
            int exploreChance = 50;
            if (villagePartyCount >= 3) {
                exploreChance = 100;
            }
            if (GameUtilities.RollChance(exploreChance, ref log)) { //50
                if (!factionOwner.partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Exploration)) {
                    factionOwner.partyQuestBoard.CreateExplorationPartyQuest(null, owner, owner.region);
                }
            }

            //Morning Patrol
#if DEBUG_LOG
            log += "\nWill try Morning Patrol";
#endif
            int morningPatrolChance = 50;
            if (villagePartyCount >= 3) {
                morningPatrolChance = 100;
            }
            if (GameUtilities.RollChance(morningPatrolChance, ref log)) { //25
                if (!factionOwner.partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Morning_Patrol)) {
                    factionOwner.partyQuestBoard.CreateMorningPatrolPartyQuest(null, owner);
                }
            }

            //Night Patrol
#if DEBUG_LOG
            log += "\nWill try Night Patrol";
#endif
            int nightPatrolChance = 50;
            if (villagePartyCount >= 2) {
                nightPatrolChance = 100;
            }
            if (GameUtilities.RollChance(nightPatrolChance, ref log)) { //50
                if (!factionOwner.partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Night_Patrol)) {
                    factionOwner.partyQuestBoard.CreateNightPatrolPartyQuest(null, owner);
                }
            }

            //Raid
#if DEBUG_LOG
            log += "\nWill try Raid";
#endif
            if (GameUtilities.RollChance(ChanceData.GetChance(CHANCE_TYPE.Raid_Chance), ref log)) { //25
                //Only warmonger factions should raid
                if (factionOwner.factionType.HasIdeology(FACTION_IDEOLOGY.Warmonger) && factionOwner.IsAtWar()) {
#if DEBUG_LOG
                    log += "\nFaction owner is at war";
#endif
                    Faction enemyFaction = factionOwner.GetRandomAtWarFaction();
                    if (enemyFaction != null) {
#if DEBUG_LOG
                        log += "\nChosen enemy at war faction: " + enemyFaction.name;
#endif
                        BaseSettlement targetSettlement = enemyFaction.GetRandomOwnedVillage();
                        if (targetSettlement != null) {
#if DEBUG_LOG
                            log += "\nChosen target settlement: " + targetSettlement.name;
#endif
                            if (!factionOwner.partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Raid)) {
                                factionOwner.partyQuestBoard.CreateRaidPartyQuest(null, owner, targetSettlement);
                            }
                        }
                    }
                }
            }

            //Rescue
#if DEBUG_LOG
            log += "\nWill try Rescue";
#endif
            int rescueChance = 50;
            if (villagePartyCount >= 2) {
                rescueChance = 100;
            }
            if (GameUtilities.RollChance(rescueChance, ref log)) { //50
                if (!factionOwner.partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Rescue) && !factionOwner.partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Demon_Rescue)) {
                    Character characterToRescue = owner.GetRandomResidentForRescue();
                    if (characterToRescue != null) {
#if DEBUG_LOG
                        log += "\nChosen character to rescue: " + characterToRescue.name;
#endif
                        factionOwner.partyQuestBoard.CreateRescuePartyQuest(null, owner, characterToRescue);
                    }
                }
            }

            //Exterminate
#if DEBUG_LOG
            log += "\nWill try Exterminate";
#endif
            if (GameUtilities.RollChance(50, ref log)) { //50
                if (!factionOwner.partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Extermination)) {
                    LocationStructure targetStructure = owner.structureComponent.GetRandomLinkedStructureForExtermination();
                    if (targetStructure != null) {
#if DEBUG_LOG
                        log += "\nChosen linked target structure: " + targetStructure.name;
#endif
                        factionOwner.partyQuestBoard.CreateExterminatePartyQuest(null, owner, targetStructure, owner);
                    }
                }
            }


            //Hunt Beast
#if DEBUG_LOG
            log += "\nWill try Hunt Beast";
#endif
            if (owner.HasStructure(STRUCTURE_TYPE.BUTCHERS_SHOP) || owner.HasBlueprintOnTileForStructure(STRUCTURE_TYPE.BUTCHERS_SHOP)
                || owner.HasStructure(STRUCTURE_TYPE.HUNTER_LODGE) || owner.HasBlueprintOnTileForStructure(STRUCTURE_TYPE.HUNTER_LODGE)) {
                if (owner.occupiedVillageSpot != null) {
                    if (!factionOwner.partyQuestBoard.HasPartyQuest(PARTY_QUEST_TYPE.Hunt_Beast)) {
                        LocationStructure targetStructure = owner.occupiedVillageSpot.GetRandomLinkedAliveBeastDen();
                        if (targetStructure != null) {
#if DEBUG_LOG
                            log += "\nChosen linked target beast den: " + targetStructure.name;
#endif
                            factionOwner.partyQuestBoard.CreateHuntBeastPartyQuest(null, owner, targetStructure);
                        }
                    }
                } else {
#if DEBUG_LOG
                    log += "\nNo occupied village spot, will not hunt";
#endif
                }
            } else {
#if DEBUG_LOG
                log += "\nNo Butcher's Shop or Skinner's Lodge";
#endif
            }
        }
#if DEBUG_LOG
        Debug.Log(log);
#endif
    }
#endregion

#region Loading
    public void LoadReferences(SaveDataSettlementPartyComponent data) {
        if (owner.locationType == LOCATION_TYPE.VILLAGE) {
            SchedulingManager.Instance.AddEntry(scheduleDateForProcessingOfPartyQuests, ProcessingOfPartyQuests, null);
        }
    }
#endregion

}

public class SaveDataSettlementPartyComponent : SaveData<SettlementPartyComponent> {
    public GameDate scheduleDateForProcessingOfPartyQuests;

#region Overrides
    public override void Save(SettlementPartyComponent data) {
        scheduleDateForProcessingOfPartyQuests = data.scheduleDateForProcessingOfPartyQuests;
    }

    public override SettlementPartyComponent Load() {
        SettlementPartyComponent component = new SettlementPartyComponent(this);
        return component;
    }
#endregion
}
