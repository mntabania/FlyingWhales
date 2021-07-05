using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Locations.Settlements;
using Inner_Maps.Location_Structures;
using UtilityScripts;
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
            if (!quest.isAssigned) {
                if((party.members.Count >= quest.minimumPartySize && quest.madeInLocation != null && quest.madeInLocation == party.partySettlement) || party.isPlayerParty) {
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
    public PartyQuest GetRandomUnassignedPartyQuestFor(Party party) {
        PartyQuest chosenQuest = null;
        List<int> indexPool = RuinarchListPool<int>.Claim();
        for (int i = 0; i < availablePartyQuests.Count; i++) {
            PartyQuest quest = availablePartyQuests[i];
            if (!quest.isAssigned) {
                if ((party.members.Count >= quest.minimumPartySize && quest.madeInLocation != null && quest.madeInLocation == party.partySettlement) || party.isPlayerParty) {
                    indexPool.Add(i);
                }
            }
        }
        if (indexPool.Count > 0) {
            int chosenIndex = indexPool[GameUtilities.RandomBetweenTwoNumbers(0, indexPool.Count - 1)];
            chosenQuest = availablePartyQuests[chosenIndex];
        }
        RuinarchListPool<int>.Release(indexPool);
        return chosenQuest;
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
#if DEBUG_LOG
            if (!owner.isPlayerFaction) {
                if (questCreator == null) {
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
#endif
        }
    }
    public bool RemovePartyQuest(PartyQuest quest) {
        return availablePartyQuests.Remove(quest);
    }
    #endregion

    #region Party Quest Creations
    public void CreateExplorationPartyQuest(Character questCreator, BaseSettlement madeInLocation, Region region) {
        if(!owner.isMajorFaction) {
            //Cannot post quests on faction that are not major
            return;
        }
        LocationStructure targetStructure = region.GetRandomSpecialStructure();
        if (targetStructure != null) {
            ExplorationPartyQuest quest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Exploration) as ExplorationPartyQuest;
            quest.SetMadeInLocation(madeInLocation);
            quest.SetTargetStructure(targetStructure);
            AddPartyQuest(quest, questCreator);
        }
    }
    public void CreateRescuePartyQuest(Character questCreator, BaseSettlement madeInLocation, Character targetCharacter) {
        if (!owner.isMajorFaction) {
            //Cannot post quests on faction that are not major
            return;
        }
        LocationStructure targetStructure = targetCharacter.currentStructure;
        if (targetStructure != null && targetStructure.structureType.IsPlayerStructure()) {
            //Should not create rescue if the faction is a demon/demon cult and the target is in demonic structure
            if (owner.factionType.type != FACTION_TYPE.Demons && owner.factionType.type != FACTION_TYPE.Demon_Cult) {
                CreateDemonRescuePartyQuest(questCreator, madeInLocation, targetCharacter, targetStructure as DemonicStructure);
            }
        } else {
            RescuePartyQuest quest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Rescue) as RescuePartyQuest;
            quest.SetMadeInLocation(madeInLocation);
            quest.SetTargetCharacter(targetCharacter);
            AddPartyQuest(quest, questCreator);
        }
    }
    public void CreateDemonRescuePartyQuest(Character questCreator, BaseSettlement madeInLocation, Character targetCharacter, DemonicStructure targetDemonicStructure) {
        if (!owner.isMajorFaction) {
            //Cannot post quests on faction that are not major
            return;
        }
        DemonRescuePartyQuest quest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Demon_Rescue) as DemonRescuePartyQuest;
        quest.SetMadeInLocation(madeInLocation);
        quest.SetTargetCharacter(targetCharacter);
        quest.SetTargetDemonicStructure(targetDemonicStructure);
        AddPartyQuest(quest, questCreator);
    }
    public void CreateExterminatePartyQuest(Character questCreator, BaseSettlement madeInLocation, LocationStructure targetStructure, NPCSettlement originSettlement) {
        if (!owner.isMajorFaction) {
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
        if (!owner.isMajorFaction) {
            //Cannot post quests on faction that are not major
            return;
        }
        CounterattackPartyQuest quest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Counterattack) as CounterattackPartyQuest;
        quest.SetMadeInLocation(madeInLocation);
        quest.SetTargetStructure(targetStructure);
        AddPartyQuest(quest, questCreator);
    }
    public void CreateRaidPartyQuest(Character questCreator, BaseSettlement madeInLocation, BaseSettlement targetSettlement) {
        if (!owner.isMajorFaction) {
            //Cannot post quests on faction that are not major
            return;
        }
        RaidPartyQuest quest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Raid) as RaidPartyQuest;
        quest.SetMadeInLocation(madeInLocation);
        quest.SetTargetSettlement(targetSettlement);
        AddPartyQuest(quest, questCreator);
    }
    public void CreateDemonRaidPartyQuest(Character questCreator, BaseSettlement madeInLocation, BaseSettlement targetSettlement) {
        if (!owner.isMajorFaction) {
            //Cannot post quests on faction that are not major
            return;
        }
        DemonRaidPartyQuest quest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Demon_Raid) as DemonRaidPartyQuest;
        quest.SetMadeInLocation(madeInLocation);
        quest.SetTargetSettlement(targetSettlement);
        AddPartyQuest(quest, questCreator);
    }
    public void CreateDemonDefendPartyQuest(Character questCreator, BaseSettlement madeInLocation, LocationStructure targetStructure) {
        if (!owner.isMajorFaction) {
            //Cannot post quests on faction that are not major
            return;
        }
        DemonDefendPartyQuest quest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Demon_Defend) as DemonDefendPartyQuest;
        quest.SetMadeInLocation(madeInLocation);
        quest.SetTargetStructure(targetStructure);
        AddPartyQuest(quest, questCreator);
    }
    public void CreateDemonSnatchPartyQuest(Character questCreator, BaseSettlement madeInLocation, Character targetCharacter, DemonicStructure dropStructure) {
        if (!owner.isMajorFaction) {
            //Cannot post quests on faction that are not major
            return;
        }
        DemonSnatchPartyQuest quest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Demon_Snatch) as DemonSnatchPartyQuest;
        quest.SetMadeInLocation(madeInLocation);
        quest.SetTargetCharacter(targetCharacter);
        quest.SetDropStructure(dropStructure);
        AddPartyQuest(quest, questCreator);
    }
    public void CreateMorningPatrolPartyQuest(Character questCreator, BaseSettlement madeInLocation) {
        if (!owner.isMajorFaction) {
            //Cannot post quests on faction that are not major
            return;
        }
        MorningPatrolPartyQuest quest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Morning_Patrol) as MorningPatrolPartyQuest;
        quest.SetMadeInLocation(madeInLocation);
        AddPartyQuest(quest, questCreator);
    }
    public void CreateNightPatrolPartyQuest(Character questCreator, BaseSettlement madeInLocation) {
        if (!owner.isMajorFaction) {
            //Cannot post quests on faction that are not major
            return;
        }
        NightPatrolPartyQuest quest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Night_Patrol) as NightPatrolPartyQuest;
        quest.SetMadeInLocation(madeInLocation);
        AddPartyQuest(quest, questCreator);
    }
    public void CreateHuntBeastPartyQuest(Character questCreator, BaseSettlement madeInLocation, LocationStructure targetStructure) {
        if (!owner.isMajorFaction) {
            //Cannot post quests on faction that are not major
            return;
        }
        HuntBeastPartyQuest quest = PartyManager.Instance.CreateNewPartyQuest(PARTY_QUEST_TYPE.Hunt_Beast) as HuntBeastPartyQuest;
        quest.SetMadeInLocation(madeInLocation);
        quest.SetTargetStructure(targetStructure);
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