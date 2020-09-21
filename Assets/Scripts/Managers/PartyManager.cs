using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UtilityScripts;
using Locations.Settlements;
using Inner_Maps.Location_Structures;
using Inner_Maps;

public class PartyManager : MonoBehaviour {
    public static PartyManager Instance;

    public const int MAX_MEMBER_CAPACITY = 5;

    public PartyNameLayouts partyNameLayouts;
    public PartyNameNouns partyNameNouns;
    public PartyNameAdjectives partyNameAdjectives;
    public PartyNameDeclarations partyNameDeclarations;
    public PartyNameTargets partyNameTargets;

    void Awake() {
        Instance = this;
    }

    #region General
    public Party CreateNewParty(Character partyCreator) {
        Party newParty = ObjectPoolManager.Instance.CreateNewParty();
        newParty.Initialize(partyCreator);
        return newParty;
    }
    public Party CreateNewParty(SaveDataParty data) {
        Party newParty = ObjectPoolManager.Instance.CreateNewParty();
        newParty.Initialize(data);
        return newParty;
    }
    private PartyQuest CreateNewPartyQuest(PARTY_QUEST_TYPE type) {
        var typeName = $"{UtilityScripts.Utilities.NotNormalizedConversionEnumToStringNoSpaces(type.ToString())}PartyQuest, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        return Activator.CreateInstance(Type.GetType(typeName) ?? throw new Exception($"provided party quest type was invalid! {typeName}")) as PartyQuest ?? throw new Exception($"provided type not a party quest! {typeName}");
    }
    private SaveDataPartyQuest CreateNewSaveDataPartyQuest(PartyQuest party) {
        SaveDataPartyQuest saveParty = Activator.CreateInstance(party.serializedData) as SaveDataPartyQuest;
        saveParty.Save(party);
        return saveParty;
    }
    public PartyQuest CreateNewPartyQuest(SaveDataPartyQuest data) {
        var typeName = $"{UtilityScripts.Utilities.NotNormalizedConversionEnumToStringNoSpaces(data.partyQuestType.ToString())}PartyQuest, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        return Activator.CreateInstance(Type.GetType(typeName), data) as PartyQuest;
    }
    //public Party CreateNewParty(PARTY_QUEST_TYPE type) {
    //    Party newParty = CreateNewParty(type);
    //    newParty.SetLeader(leader);
    //    return newParty;
    //}
    #endregion

    #region Party Name Generator
    public string GetNewPartyName(Character partyCreator) {
        string layout = CollectionUtilities.GetRandomElement(partyNameLayouts.layouts);
        string newPartyName = PartyNameLayoutReplacer(layout, partyCreator);
        return newPartyName;
    }

    private string PartyNameLayoutReplacer(string layout, Character partyCreator = null) {
        string[] words = UtilityScripts.Utilities.SplitAndKeepDelimiters(layout, UtilityScripts.Utilities.delimiters);
        for (int i = 0; i < words.Length; i++) {
            string replacedWord = string.Empty;
            string word = words[i];
            if (word.StartsWith("[") && word.EndsWith("]")) {
                replacedWord = PartyNameFillerReplacer(word, partyCreator);
            }
            if (!string.IsNullOrEmpty(replacedWord)) {
                words[i] = replacedWord;
            }
        }

        string newText = string.Empty;
        //Rebuild text
        for (int i = 0; i < words.Length; i++) {
            newText += $"{words[i]}";
        }
        newText = newText.Trim(' ');
        return newText;
    }

    private string PartyNameFillerReplacer(string filler, Character partyCreator) {
        string newWord = filler;
        List<string> poolToChoose = null;
        if(filler == "[Noun]") {
            poolToChoose = partyNameNouns.nouns;
        } else if (filler == "[Adjective]") {
            poolToChoose = partyNameAdjectives.adjectives;
        } else if (filler == "[Target]") {
            poolToChoose = partyNameTargets.targets;
        } else if (filler == "[Declaration]") {
            poolToChoose = partyNameDeclarations.declarations;
        } else if (filler == "[Region]") {
            if(partyCreator.currentRegion != null) {
                newWord = partyCreator.currentRegion.name;
            }
        }
        if (poolToChoose != null) {
            int index = CollectionUtilities.GetRandomIndexInList(poolToChoose);
            newWord = poolToChoose[index];
            if (filler == "[Noun]") {
                poolToChoose.RemoveAt(index);
            }
        }
        return newWord;
    }
    #endregion

    #region Party Quests
    public void CreateExplorationPartyQuest(BaseSettlement settlement, Region region) {
        ExplorationPartyQuest quest = CreateNewPartyQuest(PARTY_QUEST_TYPE.Exploration) as ExplorationPartyQuest;
        quest.SetRegionRefForGettingNewStructure(region);
        quest.ProcessSettingTargetStructure();
        settlement.AddPartyQuest(quest);
    }
    public void CreateRescuePartyQuest(BaseSettlement settlement, Character targetCharacter) {
        RescuePartyQuest quest = CreateNewPartyQuest(PARTY_QUEST_TYPE.Rescue) as RescuePartyQuest;
        quest.SetTargetCharacter(targetCharacter);
        settlement.AddPartyQuest(quest);
    }
    public void CreateExterminatePartyQuest(BaseSettlement settlement, LocationStructure targetStructure, NPCSettlement originSettlement) {
        ExterminationPartyQuest quest = CreateNewPartyQuest(PARTY_QUEST_TYPE.Extermination) as ExterminationPartyQuest;
        quest.SetOriginSettlement(originSettlement);
        quest.SetTargetStructure(targetStructure);
        settlement.AddPartyQuest(quest);
    }
    public void CreateCounterattackPartyQuest(BaseSettlement settlement, LocationStructure targetStructure) {
        CounterattackPartyQuest quest = CreateNewPartyQuest(PARTY_QUEST_TYPE.Counterattack) as CounterattackPartyQuest;
        quest.SetTargetStructure(targetStructure);
        settlement.AddPartyQuest(quest);
    }
    public void CreateRaidPartyQuest(BaseSettlement settlement, BaseSettlement targetSettlement) {
        RaidPartyQuest quest = CreateNewPartyQuest(PARTY_QUEST_TYPE.Raid) as RaidPartyQuest;
        quest.SetTargetSettlement(targetSettlement);
        settlement.AddPartyQuest(quest);
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
}
