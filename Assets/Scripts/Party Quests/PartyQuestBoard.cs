using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Locations.Settlements;
using Inner_Maps.Location_Structures;

public class PartyQuestBoard {
    public Faction owner { get; private set; }
    public List<PartyQuest> availablePartyQuests { get; protected set; }

    public PartyQuestBoard(Faction owner) {
        this.owner = owner;
        availablePartyQuests = new List<PartyQuest>();
    }
    public PartyQuestBoard(SaveDataPartyQuestBoard data) {
        availablePartyQuests = new List<PartyQuest>();
    }

    #region Party Quest
    public PartyQuest GetFirstUnassignedPartyQuestFor(Party party) {
        PartyQuest chosenSecondaryQuest = null; //The quest that is not made in the location. Party must prioritize quest that is made in their settlement, if there is none, just get the first unassigned, i.e the secondary quest
        for (int i = 0; i < availablePartyQuests.Count; i++) {
            PartyQuest quest = availablePartyQuests[i];
            if (!quest.isAssigned && party.members.Count >= quest.minimumPartySize) {
                if(quest.madeInLocation != null && quest.madeInLocation == party.partySettlement) {
                    return quest;
                } else {
                    if(chosenSecondaryQuest == null) {
                        chosenSecondaryQuest = quest;
                    }
                }
            }
        }
        return chosenSecondaryQuest;
    }
    public bool HasPartyQuest(PARTY_QUEST_TYPE questType) {
        return GetPartyQuest(questType) != null;
    }
    public bool HasPartyQuestWithTarget(PARTY_QUEST_TYPE questType, IPartyQuestTarget target) {
        return GetPartyQuestWithTarget(questType, target) != null;
    }
    public PartyQuest GetPartyQuestWithTarget(PARTY_QUEST_TYPE questType, IPartyQuestTarget target) {
        for (int i = 0; i < availablePartyQuests.Count; i++) {
            PartyQuest quest = availablePartyQuests[i];
            if (quest.partyQuestType == questType && quest.target == target) {
                return quest;
            }
        }
        return null;
    }
    public PartyQuest GetPartyQuest(PARTY_QUEST_TYPE questType) {
        for (int i = 0; i < availablePartyQuests.Count; i++) {
            PartyQuest quest = availablePartyQuests[i];
            if (quest.partyQuestType == questType) {
                return quest;
            }
        }
        return null;
    }
    public void AddPartyQuest(PartyQuest quest, Character questCreator) {
        if (!availablePartyQuests.Contains(quest)) {
            availablePartyQuests.Add(quest);

            if(questCreator == null) {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Party", "Quest", "post_quest_no_creator", providedTags: LOG_TAG.Party);
                log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.FACTION_1);
                log.AddToFillers(null, quest.GetPartyQuestTextInLog(), LOG_IDENTIFIER.STRING_1);
                log.AddLogToDatabase();
                PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
            } else {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Party", "Quest", "post_quest", providedTags: LOG_TAG.Party);
                log.AddToFillers(questCreator, questCreator.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.FACTION_1);
                log.AddToFillers(null, quest.GetPartyQuestTextInLog(), LOG_IDENTIFIER.STRING_1);
                log.AddLogToDatabase();
                PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
            }
        }
    }
    public bool RemovePartyQuest(PartyQuest quest) {
        return availablePartyQuests.Remove(quest);
    }
    #endregion

    #region Party Quest Creations
    public void CreateExplorationPartyQuest(Character questCreator, BaseSettlement madeInLocation, Region region) {
        if(!owner.isMajorNonPlayer) {
            //Cannot post quests on faction that are not major
            return;
        }
        ExplorationPartyQuest quest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Exploration) as ExplorationPartyQuest;
        quest.SetMadeInLocation(madeInLocation);
        quest.SetRegionRefForGettingNewStructure(region);
        quest.ProcessSettingTargetStructure();
        AddPartyQuest(quest, questCreator);
    }
    public void CreateRescuePartyQuest(Character questCreator, BaseSettlement madeInLocation, Character targetCharacter) {
        if (!owner.isMajorNonPlayer) {
            //Cannot post quests on faction that are not major
            return;
        }
        RescuePartyQuest quest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Rescue) as RescuePartyQuest;
        quest.SetMadeInLocation(madeInLocation);
        quest.SetTargetCharacter(targetCharacter);
        AddPartyQuest(quest, questCreator);
    }
    public void CreateExterminatePartyQuest(Character questCreator, BaseSettlement madeInLocation, LocationStructure targetStructure, NPCSettlement originSettlement) {
        if (!owner.isMajorNonPlayer) {
            //Cannot post quests on faction that are not major
            return;
        }
        ExterminationPartyQuest quest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Extermination) as ExterminationPartyQuest;
        quest.SetMadeInLocation(madeInLocation);
        quest.SetOriginSettlement(originSettlement);
        quest.SetTargetStructure(targetStructure);
        AddPartyQuest(quest, questCreator);
    }
    public void CreateCounterattackPartyQuest(Character questCreator, BaseSettlement madeInLocation, LocationStructure targetStructure) {
        if (!owner.isMajorNonPlayer) {
            //Cannot post quests on faction that are not major
            return;
        }
        CounterattackPartyQuest quest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Counterattack) as CounterattackPartyQuest;
        quest.SetMadeInLocation(madeInLocation);
        quest.SetTargetStructure(targetStructure);
        AddPartyQuest(quest, questCreator);
    }
    public void CreateRaidPartyQuest(Character questCreator, BaseSettlement madeInLocation, BaseSettlement targetSettlement) {
        if (!owner.isMajorNonPlayer) {
            //Cannot post quests on faction that are not major
            return;
        }
        RaidPartyQuest quest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Raid) as RaidPartyQuest;
        quest.SetMadeInLocation(madeInLocation);
        quest.SetTargetSettlement(targetSettlement);
        AddPartyQuest(quest, questCreator);
    }
    //public void CreateMonsterInvadePartyQuest(BaseSettlement settlement, HexTile hexForJoining, LocationStructure targetStructure) {
    //    MonsterInvadePartyQuest quest = CreateNewPartyQuest(PARTY_QUEST_TYPE.Monster_Invade) as MonsterInvadePartyQuest;
    //    quest.SetHexForJoining(hexForJoining);
    //    quest.SetTargetStructure(targetStructure);
    //    settlement.AddPartyQuest(quest);
    //}
    //public void CreateMonsterInvadePartyQuest(BaseSettlement settlement, HexTile hexForJoining, HexTile targetHex) {
    //    MonsterInvadePartyQuest quest = CreateNewPartyQuest(PARTY_QUEST_TYPE.Monster_Invade) as MonsterInvadePartyQuest;
    //    quest.SetHexForJoining(hexForJoining);
    //    quest.SetTargetHex(targetHex);
    //    settlement.AddPartyQuest(quest);
    //}
    #endregion

    #region Loading
    public void LoadReferences(SaveDataPartyQuestBoard data) {
        owner = FactionManager.Instance.GetFactionByPersistentID(data.owner);

        if (data.availablePartyQuests != null) {
            for (int i = 0; i < data.availablePartyQuests.Count; i++) {
                PartyQuest quest = DatabaseManager.Instance.partyQuestDatabase.GetPartyQuestByPersistentID(data.availablePartyQuests[i]);
                availablePartyQuests.Add(quest);
            }
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataPartyQuestBoard: SaveData<PartyQuestBoard> {
    public string owner;
    public List<string> availablePartyQuests;

    #region Overrides
    public override void Save(PartyQuestBoard data) {
        owner = data.owner.persistentID;

        availablePartyQuests = new List<string>();
        for (int i = 0; i < data.availablePartyQuests.Count; i++) {
            PartyQuest quest = data.availablePartyQuests[i];
            availablePartyQuests.Add(quest.persistentID);
            SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(quest);
        }
    }

    public override PartyQuestBoard Load() {
        PartyQuestBoard data = new PartyQuestBoard(this);
        return data;
    }
    #endregion
}