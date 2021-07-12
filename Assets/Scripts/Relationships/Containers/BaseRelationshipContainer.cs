using System;
using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
public class BaseRelationshipContainer : IRelationshipContainer {
    
    private const int Friend_Requirement = 1; //opinion requirement to consider someone a friend
    private const int Enemy_Requirement = -1; //opinion requirement to consider someone an enemy
    
    public Dictionary<int, IRelationshipData> relationships { get; }
    public List<Character> charactersWithOpinion { get; }
    
    public BaseRelationshipContainer() {
        relationships = new Dictionary<int, IRelationshipData>();
        charactersWithOpinion = new List<Character>();
        Messenger.AddListener<Character>(WorldEventSignals.NEW_VILLAGER_ARRIVED, OnNewVillagerArrived);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterChangedName);
    }
    public BaseRelationshipContainer(SaveDataBaseRelationshipContainer data) {
        relationships = new Dictionary<int, IRelationshipData>(data.relationships);
        charactersWithOpinion = SaveUtilities.ConvertIDListToCharacters(data.charactersWithOpinion);
        Messenger.AddListener<Character>(WorldEventSignals.NEW_VILLAGER_ARRIVED, OnNewVillagerArrived);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterChangedName);
    }

    #region Adding
    public void OnNewVillagerArrived(Character character) {
        if (HasRelationshipWith(character)) {
            //if this character already has a relationship with the character that just spawned, 
            //add that character to its charactersWithOpinion list since it wasn't added to begin with, since
            //that character did not have an instance yet.
            charactersWithOpinion.Add(character);
        }
    }
    public void AddRelationship(Relatable owner, Relatable relatable, RELATIONSHIP_TYPE relType) {
        if (HasRelationshipWith(relatable) == false) {
            CreateNewRelationship(owner, relatable);
        }
        relationships[relatable.id].AddRelationship(relType);
        Messenger.Broadcast(CharacterSignals.RELATIONSHIP_TYPE_ADDED, owner, relatable);
    }
    public IRelationshipData CreateNewRelationship(Relatable owner, Relatable relatable) {
        Assert.IsFalse(owner == relatable, $"{owner.relatableName} is trying to add a relationship with itself!");
        IRelationshipData data = new BaseRelationshipData();
        data.SetTargetName(relatable.relatableName);
        data.SetTargetGender(relatable.gender);
        relationships.Add(relatable.id, data);
        if (relatable is Character targetCharacter) {
            charactersWithOpinion.Add(targetCharacter);
        }
        Messenger.Broadcast(CharacterSignals.RELATIONSHIP_CREATED, owner, relatable);
        return data;
    }
    public IRelationshipData CreateNewRelationship(Relatable owner, int id, string name, GENDER gender) {
        Assert.IsFalse(owner.id == id, $"{owner.relatableName} is trying to add a relationship with itself!");
        IRelationshipData data = new BaseRelationshipData();
        data.SetTargetName(name);
        data.SetTargetGender(gender);
        relationships.Add(id, data);
        Character targetCharacter = CharacterManager.Instance.GetCharacterByID(id);
        if (targetCharacter != null) {
            charactersWithOpinion.Add(targetCharacter);
        }
        Messenger.Broadcast<Relatable, Relatable>(CharacterSignals.RELATIONSHIP_CREATED, owner, null);
        return data;
    }
    public IRelationshipData GetOrCreateRelationshipDataWith(Relatable owner, Relatable relatable) {
        if (HasRelationshipWith(relatable) == false) {
            CreateNewRelationship(owner, relatable);
        }
        return relationships[relatable.id];
    }
    public IRelationshipData GetOrCreateRelationshipDataWith(Relatable owner, int id, string name, GENDER gender) {
        if (HasRelationshipWith(id) == false) {
            CreateNewRelationship(owner, id, name, gender);
        }
        return relationships[id];
    }
    private bool TryGetRelationshipDataWith(Relatable relatable, out IRelationshipData data) {
        return TryGetRelationshipDataWith(relatable.id, out data);
    }
    private bool TryGetRelationshipDataWith(int id, out IRelationshipData data) {
        return relationships.TryGetValue(id, out data);
    }
    #endregion

    #region Removing
    public void RemoveRelationship(Relatable relatable, RELATIONSHIP_TYPE rel) {
        relationships[relatable.id].RemoveRelationship(rel);
    }
    #endregion

    #region Inquiry
    public bool HasRelationshipWith(Relatable relatable) {
        return HasRelationshipWith(relatable.id);
    }
    public bool HasRelationshipWith(int id) {
        return relationships.ContainsKey(id);
    }
    public bool HasSpecialRelationshipWith(Relatable relatable) {
        IRelationshipData relData = GetRelationshipDataWith(relatable);
        if(relData != null && relData.relationships != null && relData.relationships.Count > 0) {
            return true;
        }
        return false;
    }
    public bool HasRelationshipWith(Relatable relatable, RELATIONSHIP_TYPE relType) {
        if (HasRelationshipWith(relatable)) {
            IRelationshipData data = relationships[relatable.id];
            return data.HasRelationship(relType);
        }
        return false;
    }
    public bool HasRelationshipWith(Relatable relatable, RELATIONSHIP_TYPE relType1, RELATIONSHIP_TYPE relType2) {
        if (HasRelationshipWith(relatable)) {
            IRelationshipData data = relationships[relatable.id];
            return data.HasRelationship(relType1, relType2);
        }
        return false;
    }
    public bool HasRelationshipWith(Relatable relatable, RELATIONSHIP_TYPE relType1, RELATIONSHIP_TYPE relType2, RELATIONSHIP_TYPE relType3) {
        if (HasRelationshipWith(relatable)) {
            IRelationshipData data = relationships[relatable.id];
            return data.HasRelationship(relType1, relType2, relType3);
        }
        return false;
    }
    public bool HasRelationshipWith(Relatable relatable, RELATIONSHIP_TYPE relType1, RELATIONSHIP_TYPE relType2, RELATIONSHIP_TYPE relType3, RELATIONSHIP_TYPE relType4, RELATIONSHIP_TYPE relType5) {
        if (HasRelationshipWith(relatable)) {
            IRelationshipData data = relationships[relatable.id];
            return data.HasRelationship(relType1, relType2, relType3, relType4, relType5);
        }
        return false;
    }
    public bool IsFamilyMember(Character target) {
        if (HasRelationshipWith(target)) {
            IRelationshipData data = GetRelationshipDataWith(target);
            return data.IsFamilyMember();
        }
        return false;
    }
    public bool HasRelationship(RELATIONSHIP_TYPE type) {
        return GetRelatablesWithRelationshipCount(type) > 0;
    }
    public bool HasActiveRelationship(RELATIONSHIP_TYPE type1, RELATIONSHIP_TYPE type2) {
        Character character = GetFirstCharacterWithRelationship(type1, type2);
        if (character != null) {
            return true;
        }
        return false;
    }
    #endregion

    #region Getting
    public Character GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE type1, RELATIONSHIP_TYPE type2) {
        foreach (KeyValuePair<int, IRelationshipData> kvp in relationships) {
            if (kvp.Value.HasRelationship(type1, type2)) {
                Character character = CharacterManager.Instance.GetCharacterByID(kvp.Key);
                if (character != null) {
                    return character;
                }
            }
        }
        return null;
    }
    public Character GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE type) {
        foreach (KeyValuePair<int, IRelationshipData> kvp in relationships) {
            if (kvp.Value.HasRelationship(type)) {
                Character character = CharacterManager.Instance.GetCharacterByID(kvp.Key);
                if (character != null) {
                    return character;    
                }
            }
        }
        return null;
    }
    public bool IsRelativeLoverOrAffairAndNotRival(Character character) {
        return (IsFamilyMember(character) || HasRelationshipWith(character, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR)) && GetOpinionLabel(character) != RelationshipManager.Rival;
    }
    public int GetFirstRelatableIDWithRelationship(RELATIONSHIP_TYPE type) {
        foreach (KeyValuePair<int, IRelationshipData> kvp in relationships) {
            if (kvp.Value.HasRelationship(type)) {
                return kvp.Key;
            }
        }
        return -1;
    }
    public void PopulateAllRelatableIDWithRelationship(List<int> ids, RELATIONSHIP_TYPE type) {
        foreach (KeyValuePair<int, IRelationshipData> kvp in relationships) {
            if (kvp.Value.HasRelationship(type)) {
                ids.Add(kvp.Key);
            }
        }
    }
    public int GetRelatablesWithRelationshipCount(RELATIONSHIP_TYPE type) {
        int count = 0;
        foreach (KeyValuePair<int, IRelationshipData> kvp in relationships) {
            if (kvp.Value.HasRelationship(type)) {
                count++;
            }
        }
        return count;
    }
    public int GetRelatablesWithRelationshipCount(RELATIONSHIP_TYPE type1, RELATIONSHIP_TYPE type2) {
        int count = 0;
        foreach (KeyValuePair<int, IRelationshipData> kvp in relationships) {
            if (kvp.Value.HasRelationship(type1, type2)) {
                count++;
            }
        }
        return count;
    }
    public IRelationshipData GetRelationshipDataWith(Relatable relatable) {
        return GetRelationshipDataWith(relatable.id);
    }
    public IRelationshipData GetRelationshipDataWith(int id) {
        if (HasRelationshipWith(id)) {
            return relationships[id];
        }
        return null;
    }
    public RELATIONSHIP_TYPE GetRelationshipFromParametersWith(Relatable relatable, RELATIONSHIP_TYPE relType1, RELATIONSHIP_TYPE relType2) {
        if (HasRelationshipWith(relatable)) {
            IRelationshipData data = relationships[relatable.id];
            for (int j = 0; j < data.relationships.Count; j++) {
                RELATIONSHIP_TYPE dataRel = data.relationships[j];
                if (relType1 == dataRel || relType2 == dataRel) {
                    return dataRel;
                }
            }
            return RELATIONSHIP_TYPE.NONE;
        }
        return RELATIONSHIP_TYPE.NONE;
    }
    public Character GetRandomMissingCharacterWithOpinion(string opinionLabel) {
        Character chosenCharacter = null;
        List<Character> characters = RuinarchListPool<Character>.Claim();
        for (int i = 0; i < charactersWithOpinion.Count; i++) {
            Character target = charactersWithOpinion[i];
            if (!target.isDead && GetAwarenessState(target) == AWARENESS_STATE.Missing) {
                if (GetOpinionLabel(target) == opinionLabel) {
                    characters.Add(target);
                }
            }
        }
        if(characters.Count > 0) {
            chosenCharacter = UtilityScripts.CollectionUtilities.GetRandomElement(characters);
        }
        RuinarchListPool<Character>.Release(characters);
        return chosenCharacter;
    }
    public Character GetRandomMissingCharacterThatIsFamilyMemberOrLoverAffairOf(Character p_character) {
        Character chosenCharacter = null;
        List<Character> characters = RuinarchListPool<Character>.Claim();
        for (int i = 0; i < charactersWithOpinion.Count; i++) {
            Character target = charactersWithOpinion[i];
            if (!target.isDead && GetAwarenessState(target) == AWARENESS_STATE.Missing) {
                if (p_character.relationshipContainer.IsFamilyMember(target) 
                    || p_character.relationshipContainer.HasRelationshipWith(target, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR)) {
                    characters.Add(target);
                }
            }
        }
        if (characters.Count > 0) {
            chosenCharacter = UtilityScripts.CollectionUtilities.GetRandomElement(characters);
        }
        RuinarchListPool<Character>.Release(characters);
        return chosenCharacter;
    }
    #endregion

    #region Opinions
    public void AdjustOpinion(Character owner, Character target, string opinionText, int opinionValue, string lastStrawReason = "") {
        if (owner.minion != null || owner is Summon) {
            if (!owner.relationshipContainer.HasSpecialRelationshipWith(target)) {
                //Minions or Summons cannot have opinions on characters that they do not have relationships with
                return;    
            }
        }
        if (target.minion != null || target is Summon) {
            if (!target.relationshipContainer.HasSpecialRelationshipWith(owner)) {
                //Minions or Summons cannot have opinions on characters that they do not have relationships with
                return;    
            }
        }
        if(owner == target) {
            //Cannot adjust opinion to self
            //Therefore, must not have a relationship with self
            return;
        }
        IRelationshipData relationshipData = GetOrCreateRelationshipDataWith(owner, target);
        string opinionLabelBeforeChange = GetOpinionLabel(target);
        if (owner.traitContainer.HasTrait("Psychopath")) {
            Psychopath psychopath = owner.traitContainer.GetTraitOrStatus<Psychopath>("Psychopath");
            psychopath.AdjustOpinion(target, opinionText, opinionValue);
            //Psychopaths do not gain or lose Opinion towards other characters (ensure that logs related to Opinion changes also do not show up)
#if DEBUG_LOG
            owner.logComponent.PrintLogIfActive(
                $"{owner.name} wants to adjust {opinionText} opinion towards {target.name} by {opinionValue} but {owner.name} is a Psychopath");
#endif
            opinionValue = 0;
        }
        relationshipData.opinions.AdjustOpinion(opinionText, opinionValue);
        if (opinionValue > 0) {
            //OnOpinionReduced(owner, target, lastStrawReason);
            Messenger.Broadcast(CharacterSignals.OPINION_INCREASED, owner, target, lastStrawReason);
        } else if (opinionValue < 0) {
            if (!owner.reactionComponent.isDisguised && !target.reactionComponent.isDisguised) {
                //only create jobs if both the owner of this and the target is not disguised.
                CreateJobsOnOpinionReduced(owner, target, lastStrawReason, opinionValue);    
            }
            Messenger.Broadcast(CharacterSignals.OPINION_DECREASED, owner, target, lastStrawReason);
        }
        string opinionLabelAfterChange = GetOpinionLabel(target);
        if (opinionLabelBeforeChange != opinionLabelAfterChange && opinionValue < 0) {
            //Only broadcast this signal when an opinion label has changed because of decrease in opinion.
            //This is so that we do not catch cases when Rival Characters become enemies when we expect that
            //this signal will only broadcast when characters change from enemies to rivals
            Messenger.Broadcast(CharacterSignals.OPINION_LABEL_DECREASED, owner, target, opinionLabelAfterChange);
        }
        
        if (target.relationshipContainer.HasRelationshipWith(owner) == false) {
            target.relationshipContainer.CreateNewRelationship(target, owner);
        }
    }
    public void SetOpinion(Character owner, Character target, string opinionText, int opinionValue, string lastStrawReason = "") {
        if (owner.minion != null || owner is Summon) {
            //Minions or Summons cannot have opinions
            return;
        }
        if (target.minion != null || target is Summon) {
            //Minions or Summons cannot have opinions
            return;
        }
        IRelationshipData relationshipData = GetOrCreateRelationshipDataWith(owner, target);
        string opinionLabelBeforeChange = GetOpinionLabel(target);
        if (owner.traitContainer.HasTrait("Psychopath")) {
            //Psychopaths do not gain or lose Opinion towards other characters (ensure that logs related to Opinion changes also do not show up)
#if DEBUG_LOG
            owner.logComponent.PrintLogIfActive(
                $"{owner.name} wants to adjust {opinionText} opinion towards {target.name} by {opinionValue} but {owner.name} is a Psychopath, setting the value to zero...");
#endif
            opinionValue = 0;
        }
        relationshipData.opinions.SetOpinion(opinionText, opinionValue);
        if (opinionValue > 0) {
            //OnOpinionReduced(owner, target, lastStrawReason);
            Messenger.Broadcast(CharacterSignals.OPINION_INCREASED, owner, target, lastStrawReason);
        } else if (opinionValue < 0) {
            CreateJobsOnOpinionReduced(owner, target, lastStrawReason, opinionValue);
            Messenger.Broadcast(CharacterSignals.OPINION_DECREASED, owner, target, lastStrawReason);
        }
        string opinionLabelAfterChange = GetOpinionLabel(target);
        if (opinionLabelBeforeChange != opinionLabelAfterChange && opinionValue < 0) {
            //Only broadcast this signal when an opinion label has changed because of decrease in opinion.
            //This is so that we do not catch cases when Rival Characters become enemies when we expect that
            //this signal will only broadcast when characters change from enemies to rivals
            Messenger.Broadcast(CharacterSignals.OPINION_LABEL_DECREASED, owner, target, opinionLabelAfterChange);
        }
        if (target.relationshipContainer.HasRelationshipWith(owner) == false) {
            target.relationshipContainer.CreateNewRelationship(target, owner);
        }
    }
    public void SetOpinion(Character owner, int targetID, string targetName, GENDER gender, string opinionText,
        int opinionValue, bool isInitial, string lastStrawReason = "") {
        if (owner.minion != null || owner is Summon) {
            //Minions or Summons cannot have opinions
            return;
        }
        
        Character targetCharacter = CharacterManager.Instance.GetCharacterByID(targetID);
        if (targetCharacter != null) {
            SetOpinion(owner, targetCharacter, opinionText, opinionValue, lastStrawReason);
        } else {
            IRelationshipData relationshipData = GetOrCreateRelationshipDataWith(owner, targetID, targetName, gender);
            if (owner.traitContainer.HasTrait("Psychopath")) {
                opinionValue = 0;
            }
            relationshipData.opinions.SetOpinion(opinionText, opinionValue);
            if (!isInitial) {
                if (opinionValue > 0) {
                    //OnOpinionChanged(owner, targetCharacter, lastStrawReason);
                    Messenger.Broadcast<Character, Character, string>(CharacterSignals.OPINION_INCREASED, owner, null, lastStrawReason);
                } else if (opinionValue < 0) {
                    CreateJobsOnOpinionReduced(owner, targetCharacter, lastStrawReason, opinionValue);
                    Messenger.Broadcast<Character, Character, string>(CharacterSignals.OPINION_DECREASED, owner, null, lastStrawReason);
                }
            }
        }
    }
    private void CreateJobsOnOpinionReduced(Character owner, Character targetCharacter, string reason, int amountReduced) {
        if (owner.relationshipContainer.IsEnemiesWith(targetCharacter) && GameManager.Instance.gameHasStarted) {
            //Character spreadRumorTarget = owner.rumorComponent.GetRandomSpreadRumorTarget(targetCharacter);
            //if (spreadRumorTarget != null) {
            //    Rumor rumor = owner.rumorComponent.GenerateNewRandomRumor(spreadRumorTarget, targetCharacter);
            //    owner.jobComponent.CreateSpreadRumorJob(spreadRumorTarget, rumor);
            //    return;
            //}

            if (UnityEngine.Random.Range(0, 100) < 30) {//30
                //Break up
                BreakUp(owner, targetCharacter, reason);
            }
            int chance = 4;
            int roll = UnityEngine.Random.Range(0, 100);
            MOOD_STATE ownerMood = owner.moodComponent.moodState;
            string opinionLabel = owner.relationshipContainer.GetOpinionLabel(targetCharacter);
            if (ownerMood == MOOD_STATE.Bad) {
                chance *= 5;
            } else if (ownerMood == MOOD_STATE.Critical) {
                chance *= 10;
            }
            if (opinionLabel == RelationshipManager.Rival) {
                chance *= 2;
            }

            chance += Mathf.RoundToInt((amountReduced * -1) / 5f);
            if (roll < chance) {
                if (owner.marker && owner.marker.IsPOIInVision(targetCharacter)) {
                    if (targetCharacter.combatComponent.isInCombat) {
                        if (owner.jobComponent.CreateBrawlJob(targetCharacter)) {
                            return;
                        }
                    }
                    if (owner.traitContainer.HasTrait("Combatant")) {
                        if (UnityEngine.Random.Range(0, 100) < 50) {
                            if (owner.jobComponent.CreateBrawlJob(targetCharacter)) {
                                return;
                            }
                        }
                    }
                }
                if (UnityEngine.Random.Range(0, 100) < 25) {
                    if (!owner.partyComponent.isActiveMember) {
                        if (owner.jobComponent.CreatePoisonFoodJob(targetCharacter)) {
                            return;
                        }
                    }

                }
                if (UnityEngine.Random.Range(0, 100) < 25) {
                    if (!owner.partyComponent.isActiveMember) {
                        if (owner.jobComponent.CreatePlaceTrapJob(targetCharacter)) {
                            return;
                        }
                    }
                }
                //

                //Spread Rumor
                if (!owner.partyComponent.isActiveMember) {
                    Character spreadRumorOrNegativeInfoTarget = owner.rumorComponent.GetRandomSpreadRumorOrNegativeInfoTarget(targetCharacter);
                    if (spreadRumorOrNegativeInfoTarget != null) {
                        if (UnityEngine.Random.Range(0, 100) < 50) {
                            ActualGoapNode negativeInfo = owner.rumorComponent.GetRandomKnownNegativeInfo(spreadRumorOrNegativeInfoTarget, targetCharacter);
                            if (negativeInfo != null) {
                                if (owner.jobComponent.CreateSpreadNegativeInfoJob(spreadRumorOrNegativeInfoTarget, negativeInfo)) {
                                    return;
                                }
                            }
                        }

                        Rumor rumor = owner.rumorComponent.GenerateNewRandomRumor(spreadRumorOrNegativeInfoTarget, targetCharacter);
                        if (rumor != null) {
                            owner.jobComponent.CreateSpreadRumorJob(spreadRumorOrNegativeInfoTarget, rumor);
                        }
                    }
                }
            }
        }
        
    }
    public void RemoveOpinion(Character target, string opinionText) {
        if (TryGetRelationshipDataWith(target, out var relationshipData)) {
            relationshipData.opinions.RemoveOpinion(opinionText);
        }
    }
    public bool HasOpinion(Character target, string opinionText) {
        if (TryGetRelationshipDataWith(target, out var relationshipData)) {
            return relationshipData.opinions.HasOpinion(opinionText);
        }
        return false;
    }
    public bool HasOpinion(int id, string opinionText) {
        if (TryGetRelationshipDataWith(id, out var relationshipData)) {
            return relationshipData.opinions.HasOpinion(opinionText);
        }
        return false;
    }
    public int GetTotalOpinion(Character target) {
        return GetTotalOpinion(target.id);
    }
    public int GetTotalOpinion(int id) {
        if (HasRelationshipWith(id)) {
            return relationships[id].opinions?.totalOpinion ?? 0;    
        }
        return 0;
    }
    public OpinionData GetOpinionData(Character target) {
        return GetOpinionData(target.id);
    }
    public OpinionData GetOpinionData(int id) {
        if (HasRelationshipWith(id)) {
            return relationships[id].opinions;    
        }
        return null;
    }
    public string GetOpinionLabel(Character target) {
        return GetOpinionLabel(target.id);
    }
    public string GetOpinionLabel(int id) {
        if (HasRelationshipWith(id)) {
            return relationships[id].opinions.GetOpinionLabel();
            // int totalOpinion = GetTotalOpinion(id);
            // if (totalOpinion > 70) {
            //     return RelationshipManager.Close_Friend;
            // } else if (totalOpinion > 20 && totalOpinion <= 70) {
            //     return RelationshipManager.Friend;
            // } else if (totalOpinion > -21 && totalOpinion <= 20) {
            //     return RelationshipManager.Acquaintance;
            // } else if (totalOpinion > -71 && totalOpinion <= -21) {
            //     return RelationshipManager.Enemy;
            // } else if (totalOpinion <= -71) {
            //     return RelationshipManager.Rival;
            // }
        }
        return string.Empty;
    }
    public static string OpinionColor(int number) {
        if (number > 20) {
            return "green";    
        } else if (number > -21 && number <= 20) {
            return "#808080";
        } else {
            return "red";
        }
    }
    public static string OpinionColorNoGray(int number) {
        if (number >= 0) {
            return "green";
        } else {
            return "red";
        }
    }
    public bool IsFriendsWith(Character character) {
        string opinionLabel = GetOpinionLabel(character);
        return opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend;
    }
    public bool IsEnemiesWith(Character character) {
        string opinionLabel = GetOpinionLabel(character);
        return opinionLabel == RelationshipManager.Enemy || opinionLabel == RelationshipManager.Rival;
    }
    public Character GetFirstEnemyCharacter() {
        for (int i = 0; i < charactersWithOpinion.Count; i++) {
            Character otherCharacter = charactersWithOpinion[i];
            if (IsEnemiesWith(otherCharacter)) {
                return otherCharacter;
            }
        }
        return null;
    }
    public void PopulateEnemyCharacters(List<Character> characters) {
        for (int i = 0; i < charactersWithOpinion.Count; i++) {
            Character otherCharacter = charactersWithOpinion[i];
            if (IsEnemiesWith(otherCharacter)) {
                characters.Add(otherCharacter);
            }
        }
    }
    public Character GetRandomEnemyCharacter() {
        Character chosenCharacter = null;
        List<Character> characters = RuinarchListPool<Character>.Claim();
        for (int i = 0; i < charactersWithOpinion.Count; i++) {
            Character otherCharacter = charactersWithOpinion[i];
            if (IsEnemiesWith(otherCharacter)) {
                characters.Add(otherCharacter);
            }
        }
        if(characters.Count > 0) {
            chosenCharacter = characters[GameUtilities.RandomBetweenTwoNumbers(0, characters.Count - 1)];
        }
        RuinarchListPool<Character>.Release(characters);
        return chosenCharacter;
    }
    public void PopulateFriendCharacters(List<Character> characters) {
        for (int i = 0; i < charactersWithOpinion.Count; i++) {
            Character otherCharacter = charactersWithOpinion[i];
            if (IsFriendsWith(otherCharacter)) {
                characters.Add(otherCharacter);
            }
        }
    }
    public bool HasOpinionLabelWithCharacter(Character character, string opinion) {
        if (HasRelationshipWith(character)) {
            string opinionLabel = GetOpinionLabel(character);
            if (opinion == opinionLabel) {
                return true;
            }
        }
        return false;
    }
    public bool HasOpinionLabelWithCharacter(Character character, string opinion1, string opinion2) {
        if (HasRelationshipWith(character)) {
            string opinionLabel = GetOpinionLabel(character);
            if (opinion1 == opinionLabel || opinion2 == opinionLabel) {
                return true;
            }
        }
        return false;
    }
    public bool HasOpinionLabelWithCharacter(Character character, string opinion1, string opinion2, string opinion3) {
        if (HasRelationshipWith(character)) {
            string opinionLabel = GetOpinionLabel(character);
            if (opinion1 == opinionLabel || opinion2 == opinionLabel || opinion3 == opinionLabel) {
                return true;
            }
        }
        return false;
    }
    public bool HasOpinionLabelWithCharacter(Character character, List<OPINIONS> opinions) {
        if (HasRelationshipWith(character)) {
            string opinionLabel = GetOpinionLabel(character);
            for (int j = 0; j < opinions.Count; j++) {
                if (opinions[j].GetOpinionLabel() == opinionLabel) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasEnemyCharacter() {
        for (int i = 0; i < charactersWithOpinion.Count; i++) {
            Character otherCharacter = charactersWithOpinion[i];
            if (IsEnemiesWith(otherCharacter)) {
                return true;
            }
        }
        return false;
    }
    public int GetNumberOfFriendCharacters() {
        int count = 0;
        for (int i = 0; i < charactersWithOpinion.Count; i++) {
            Character otherCharacter = charactersWithOpinion[i];
            if (IsFriendsWith(otherCharacter)) {
                count++;
            }
        }
        return count;
    }
    public RELATIONSHIP_EFFECT GetRelationshipEffectWith(Character character) {
        if (HasRelationshipWith(character)) {
            int totalOpinion = GetTotalOpinion(character);
            if (totalOpinion > 0) {
                return RELATIONSHIP_EFFECT.POSITIVE;
            } else if (totalOpinion < 0) {
                return RELATIONSHIP_EFFECT.NEGATIVE;
            }    
        }
        return RELATIONSHIP_EFFECT.NONE;
    }
    public int GetCompatibility(Character target) {
        return GetCompatibility(target.id);
    }
    public int GetCompatibility(int targetID) {
        if (TryGetRelationshipDataWith(targetID, out var relationshipData)) {
            return relationshipData.opinions.compatibilityValue;
        }
        return -1;
    }
    public bool HasSpecialPositiveRelationshipWith(Character characterThatDied) {
        if (TryGetRelationshipDataWith(characterThatDied.id, out var data)) {
            RELATIONSHIP_TYPE relType = data.GetFirstMajorRelationship();
            switch (relType) {
                case RELATIONSHIP_TYPE.CHILD:
                case RELATIONSHIP_TYPE.LOVER:
                case RELATIONSHIP_TYPE.PARENT:
                case RELATIONSHIP_TYPE.SIBLING:
                case RELATIONSHIP_TYPE.AFFAIR:
                    return true;
                default:
                    return false;
            }
        }
        return false;
    }
    public string GetRelationshipNameWith(Character target) {
        return GetRelationshipNameWith(target.id);
    }
    public string GetRelationshipNameWith(int id) {
        if (TryGetRelationshipDataWith(id, out var data)) {
            RELATIONSHIP_TYPE relType = data.GetFirstMajorRelationship();
            switch (relType) {
                case RELATIONSHIP_TYPE.CHILD:
                    return data.targetGender == GENDER.MALE ? "Son" : "Daughter";
                case RELATIONSHIP_TYPE.PARENT:
                    return data.targetGender == GENDER.MALE ? "Father" : "Mother";
                case RELATIONSHIP_TYPE.SIBLING:
                    return data.targetGender == GENDER.MALE ? "Brother" : "Sister";
                case RELATIONSHIP_TYPE.LOVER:
                    return data.targetGender == GENDER.MALE ? "Husband" : "Wife";
                case RELATIONSHIP_TYPE.NONE:
                    string opinionLabel = GetOpinionLabel(id);
                    return string.IsNullOrEmpty(opinionLabel) == false ? opinionLabel : RelationshipManager.Acquaintance;
                default:
                    return UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetterOnly(relType.ToString());
            }
        }
        return RelationshipManager.Acquaintance;
    }
#endregion

#region Awareness
    public AWARENESS_STATE GetAwarenessState(Character character) {
        if (relationships.ContainsKey(character.id)) {
            return relationships[character.id].awareness.state;
        }
        return AWARENESS_STATE.None;
    }
    public void SetAwarenessState(Character source, Character target, AWARENESS_STATE state) {
        if (relationships.ContainsKey(target.id)) {
            AwarenessData awareness = relationships[target.id].awareness;
            if (awareness.state != state) {
                awareness.SetAwarenessState(state);
                source.stateAwarenessComponent.OnSetAwarenessState(target, state);
            }
        }
    }
#endregion

#region Utilities
    public bool BreakUp(Character owner, Character targetCharacter, string reason) {
        if (HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR)) {
            owner.interruptComponent.TriggerInterrupt(INTERRUPT.Break_Up, targetCharacter, reason);
            return true;
        }
        return false;
    }
#endregion

#region Listeners
    private void OnCharacterChangedName(Character character) {
        if (HasRelationshipWith(character)) {
            IRelationshipData relationshipData = GetRelationshipDataWith(character);
            relationshipData.SetTargetName(character.name);
        }
    }
#endregion
}

#region Save Data
public class SaveDataBaseRelationshipContainer : SaveData<BaseRelationshipContainer> {
    public Dictionary<int, IRelationshipData> relationships;
    public List<string> charactersWithOpinion;
    public override void Save(BaseRelationshipContainer data) {
        base.Save(data);
        relationships = data.relationships;
        charactersWithOpinion = SaveUtilities.ConvertSavableListToIDs(data.charactersWithOpinion);
    }
    public override BaseRelationshipContainer Load() {
        return new BaseRelationshipContainer(this);
    }
}
#endregion