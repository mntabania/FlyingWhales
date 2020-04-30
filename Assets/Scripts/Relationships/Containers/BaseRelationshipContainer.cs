using System;
using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class BaseRelationshipContainer : IRelationshipContainer {
    public const string Close_Friend = "Close Friend";
    public const string Friend = "Friend";
    public const string Acquaintance = "Acquaintance";
    public const string Enemy = "Enemy";
    public const string Rival = "Rival";
    
    private const int Friend_Requirement = 1; //opinion requirement to consider someone a friend
    private const int Enemy_Requirement = -1; //opinion requirement to consider someone an enemy
    
    public Dictionary<int, IRelationshipData> relationships { get; }
    public List<Character> charactersWithOpinion { get; }
    
    public BaseRelationshipContainer() {
        relationships = new Dictionary<int, IRelationshipData>();
        charactersWithOpinion = new List<Character>();
    }

    #region Adding
    public void AddRelationship(Relatable owner, Relatable relatable, RELATIONSHIP_TYPE relType) {
        if (HasRelationshipWith(relatable) == false) {
            CreateNewRelationship(owner, relatable);
        }
        relationships[relatable.id].AddRelationship(relType);
    }
    public IRelationshipData CreateNewRelationship(Relatable owner, Relatable relatable) {
        IRelationshipData data = new BaseRelationshipData();
        data.SetTargetName(relatable.relatableName);
        data.SetTargetGender(relatable.gender);
        relationships.Add(relatable.id, data);
        if (relatable is Character) {
            charactersWithOpinion.Add(relatable as Character);
        }
        Messenger.Broadcast(Signals.RELATIONSHIP_ADDED, owner, relatable);
        return data;
    }
    public IRelationshipData CreateNewRelationship(Relatable owner, int id, string name, GENDER gender) {
        IRelationshipData data = new BaseRelationshipData();
        data.SetTargetName(name);
        data.SetTargetGender(gender);
        relationships.Add(id, data);
        Messenger.Broadcast<Relatable, Relatable>(Signals.RELATIONSHIP_ADDED, owner, null);
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
    public bool HasRelationshipWith(Relatable relatable, RELATIONSHIP_TYPE relType) {
        if (HasRelationshipWith(relatable)) {
            IRelationshipData data = relationships[relatable.id];
            return data.relationships.Contains(relType);
        }
        return false;
    }
    public bool HasRelationshipWith(Relatable relatable, params RELATIONSHIP_TYPE[] relType) {
        if (HasRelationshipWith(relatable)) {
            IRelationshipData data = relationships[relatable.id];
            for (int i = 0; i < relType.Length; i++) {
                RELATIONSHIP_TYPE rel = relType[i];
                for (int j = 0; j < data.relationships.Count; j++) {
                    RELATIONSHIP_TYPE dataRel = data.relationships[j];
                    if(rel == dataRel) {
                        return true;
                    }
                }
            }
            return false;
        }
        return false;
    }
    public bool IsFamilyMember(Character target) {
        if (HasRelationshipWith(target)) {
            IRelationshipData data = GetRelationshipDataWith(target);
            return data.HasRelationship(RELATIONSHIP_TYPE.CHILD, RELATIONSHIP_TYPE.PARENT, RELATIONSHIP_TYPE.SIBLING);
        }
        return false;
    }
    #endregion

    #region Getting
    public int GetFirstRelatableIDWithRelationship(params RELATIONSHIP_TYPE[] type) {
        foreach (KeyValuePair<int, IRelationshipData> kvp in relationships) {
            if (kvp.Value.HasRelationship(type)) {
                return kvp.Key;
            }
        }
        return -1;
    }
    public List<int> GetAllRelatableIDWithRelationship(params RELATIONSHIP_TYPE[] type) {
        List<int> ids = new List<int>();
        foreach (KeyValuePair<int, IRelationshipData> kvp in relationships) {
            if (kvp.Value.HasRelationship(type)) {
                ids.Add(kvp.Key);
            }
        }
        return ids;
    }
    public int GetRelatablesWithRelationshipCount(params RELATIONSHIP_TYPE[] type) {
        int count = 0;
        foreach (KeyValuePair<int, IRelationshipData> kvp in relationships) {
            if (kvp.Value.HasRelationship(type)) {
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
    public RELATIONSHIP_TYPE GetRelationshipFromParametersWith(Relatable relatable, params RELATIONSHIP_TYPE[] relType) {
        if (HasRelationshipWith(relatable)) {
            IRelationshipData data = relationships[relatable.id];
            for (int i = 0; i < relType.Length; i++) {
                RELATIONSHIP_TYPE rel = relType[i];
                for (int j = 0; j < data.relationships.Count; j++) {
                    RELATIONSHIP_TYPE dataRel = data.relationships[j];
                    if (rel == dataRel) {
                        return rel;
                    }
                }
            }
            return RELATIONSHIP_TYPE.NONE;
        }
        return RELATIONSHIP_TYPE.NONE;
    }
    #endregion

    #region Opinions
    public void AdjustOpinion(Character owner, Character target, string opinionText, int opinionValue, string lastStrawReason = "") {
        if (owner.minion != null || owner is Summon) {
            //Minions or Summons cannot have opinions
            return;
        }
        IRelationshipData relationshipData = GetOrCreateRelationshipDataWith(owner, target);
        if (owner.traitContainer.HasTrait("Psychopath")) {
            Psychopath serialKiller = owner.traitContainer.GetNormalTrait<Psychopath>("Psychopath");
            serialKiller.AdjustOpinion(target, opinionText, opinionValue);
            //Psychopaths do not gain or lose Opinion towards other characters (ensure that logs related to Opinion changes also do not show up)
            owner.logComponent.PrintLogIfActive(
                $"{owner.name} wants to adjust {opinionText} opinion towards {target.name} by {opinionValue} but {owner.name} is a Serial Killer");
            opinionValue = 0;
        }
        relationshipData.opinions.AdjustOpinion(opinionText, opinionValue);
        if (opinionValue > 0) {
            OnOpinionChanged(owner, target, lastStrawReason);
            Messenger.Broadcast(Signals.OPINION_INCREASED, owner, target, lastStrawReason);
        } else if (opinionValue < 0) {
            OnOpinionChanged(owner, target, lastStrawReason);
            Messenger.Broadcast(Signals.OPINION_DECREASED, owner, target, lastStrawReason);
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
        IRelationshipData relationshipData = GetOrCreateRelationshipDataWith(owner, target);
        if (owner.traitContainer.HasTrait("Serial Killer")) {
            //Psychopaths do not gain or lose Opinion towards other characters (ensure that logs related to Opinion changes also do not show up)
            owner.logComponent.PrintLogIfActive(
                $"{owner.name} wants to adjust {opinionText} opinion towards {target.name} by {opinionValue} but {owner.name} is a Serial Killer, setting the value to zero...");
            opinionValue = 0;
        }
        relationshipData.opinions.SetOpinion(opinionText, opinionValue);
        if (opinionValue > 0) {
            OnOpinionChanged(owner, target, lastStrawReason);
            Messenger.Broadcast(Signals.OPINION_INCREASED, owner, target, lastStrawReason);
        } else if (opinionValue < 0) {
            OnOpinionChanged(owner, target, lastStrawReason);
            Messenger.Broadcast(Signals.OPINION_DECREASED, owner, target, lastStrawReason);
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
            if (owner.traitContainer.HasTrait("Serial Killer")) {
                opinionValue = 0;
            }
            relationshipData.opinions.SetOpinion(opinionText, opinionValue);
            if (!isInitial) {
                if (opinionValue > 0) {
                    OnOpinionChanged(owner, targetCharacter, lastStrawReason);
                    Messenger.Broadcast<Character, Character, string>(Signals.OPINION_INCREASED, owner, null, lastStrawReason);
                } else if (opinionValue < 0) {
                    OnOpinionChanged(owner, targetCharacter, lastStrawReason);
                    Messenger.Broadcast<Character, Character, string>(Signals.OPINION_DECREASED, owner, null, lastStrawReason);
                }
            }
        }
    }
    public void OnOpinionChanged(Character owner, Character targetCharacter, string reason) {
        if (owner.relationshipContainer.IsEnemiesWith(targetCharacter)) {
            //Character spreadRumorTarget = owner.rumorComponent.GetRandomSpreadRumorTarget(targetCharacter);
            //if (spreadRumorTarget != null) {
            //    Rumor rumor = owner.rumorComponent.GenerateNewRandomRumor(spreadRumorTarget, targetCharacter);
            //    owner.jobComponent.CreateSpreadRumorJob(spreadRumorTarget, rumor);
            //    return;
            //}

            if (UnityEngine.Random.Range(0, 100) < 30) {//30
                //Break up
                if (owner.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR)) {
                    owner.interruptComponent.TriggerInterrupt(INTERRUPT.Break_Up, targetCharacter, reason);
                    return;
                }
            }
            int chance = 1;
            int roll = UnityEngine.Random.Range(0, 100);
            MOOD_STATE ownerMood = owner.moodComponent.moodState;
            string opinionLabel = owner.relationshipContainer.GetOpinionLabel(targetCharacter);
            if (ownerMood == MOOD_STATE.LOW) {
                chance *= 10;
            } else if (ownerMood == MOOD_STATE.CRITICAL) {
                chance *= 15;
            }
            if (opinionLabel == RelationshipManager.Rival) {
                chance *= 2;
            }
            if (roll < chance) {
                if (owner.marker && owner.marker.inVisionCharacters.Contains(targetCharacter)) {
                    if (owner.traitContainer.HasTrait("Combatant")) {
                        if (UnityEngine.Random.Range(0, 100) < 50) {
                            if (owner.jobComponent.CreateBrawlJob(targetCharacter) != null) {
                                return;
                            }
                        }
                    }
                }
                if (UnityEngine.Random.Range(0, 100) < 25) {
                    if (owner.jobComponent.CreatePoisonFoodJob(targetCharacter)) {
                        return;
                    }
                }
                if (UnityEngine.Random.Range(0, 100) < 50) {
                    if (owner.jobComponent.CreatePlaceTrapJob(targetCharacter)) {
                        return;
                    }
                }
                //Spread Rumor
                Character spreadRumorTarget = owner.rumorComponent.GetRandomSpreadRumorTarget(targetCharacter);
                if (spreadRumorTarget != null) {
                    Rumor rumor = owner.rumorComponent.GenerateNewRandomRumor(spreadRumorTarget, targetCharacter);
                    owner.jobComponent.CreateSpreadRumorJob(spreadRumorTarget, rumor);
                    return;
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
    public int GetTotalOpinion(Character target) {
        return GetTotalOpinion(target.id);
    }
    public int GetTotalOpinion(int id) {
        return relationships[id].opinions.totalOpinion;
    }
    public OpinionData GetOpinionData(Character target) {
        return GetOpinionData(target.id);
    }
    public OpinionData GetOpinionData(int id) {
        return relationships[id].opinions;
    }
    public string GetOpinionLabel(Character target) {
        return GetOpinionLabel(target.id);
    }
    public string GetOpinionLabel(int id) {
        if (HasRelationshipWith(id)) {
            int totalOpinion = GetTotalOpinion(id);
            if (totalOpinion > 70) {
                return Close_Friend;
            } else if (totalOpinion > 20 && totalOpinion <= 70) {
                return Friend;
            } else if (totalOpinion > -21 && totalOpinion <= 20) {
                return Acquaintance;
            } else if (totalOpinion > -71 && totalOpinion <= -21) {
                return Enemy;
            } else if (totalOpinion <= -71) {
                return Rival;
            }
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
    public bool IsFriendsWith(Character character) {
        string opinionLabel = GetOpinionLabel(character);
        return opinionLabel == Friend || opinionLabel == Close_Friend;
    }
    public bool IsEnemiesWith(Character character) {
        string opinionLabel = GetOpinionLabel(character);
        return opinionLabel == Enemy || opinionLabel == Rival;
    }
    public List<Character> GetCharactersWithPositiveOpinion() {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < charactersWithOpinion.Count; i++) {
            Character otherCharacter = charactersWithOpinion[i];
            if (GetTotalOpinion(otherCharacter) > 0) {
                characters.Add(otherCharacter);
            }
        }
        return characters;
    }
    public List<Character> GetCharactersWithNeutralOpinion() {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < charactersWithOpinion.Count; i++) {
            Character otherCharacter = charactersWithOpinion[i];
            int opinion = GetTotalOpinion(otherCharacter); 
            if (opinion < Friend_Requirement && opinion > Enemy_Requirement) {
                characters.Add(otherCharacter);
            }
        }
        return characters;
    }
    public List<Character> GetCharactersWithNegativeOpinion() {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < charactersWithOpinion.Count; i++) {
            Character otherCharacter = charactersWithOpinion[i];
            if (GetTotalOpinion(otherCharacter) < 0) {
                characters.Add(otherCharacter);
            }
        }
        return characters;
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
    public List<Character> GetEnemyCharacters() {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < charactersWithOpinion.Count; i++) {
            Character otherCharacter = charactersWithOpinion[i];
            if (IsEnemiesWith(otherCharacter)) {
                characters.Add(otherCharacter);
            }
        }
        return characters;
    }
    public Character GetRandomEnemyCharacter() {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < charactersWithOpinion.Count; i++) {
            Character otherCharacter = charactersWithOpinion[i];
            if (IsEnemiesWith(otherCharacter)) {
                characters.Add(otherCharacter);
            }
        }
        if(characters.Count > 0) {
            return characters[UnityEngine.Random.Range(0, characters.Count)];
        }
        return null;
    }
    public List<Character> GetFriendCharacters() {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < charactersWithOpinion.Count; i++) {
            Character otherCharacter = charactersWithOpinion[i];
            if (IsFriendsWith(otherCharacter)) {
                characters.Add(otherCharacter);
            }
        }
        return characters;
    }
    public List<Character> GetCharactersWithOpinionLabel(params string[] labels) {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < charactersWithOpinion.Count; i++) {
            Character otherCharacter = charactersWithOpinion[i];
            string opinionLabel = GetOpinionLabel(otherCharacter);
            for (int j = 0; j < labels.Length; j++) {
                if (labels[j] == opinionLabel) {
                    characters.Add(otherCharacter);
                }
            }
        }
        return characters;
    }
    public bool HasCharacterWithOpinionLabel(params string[] labels) {
        for (int i = 0; i < charactersWithOpinion.Count; i++) {
            Character otherCharacter = charactersWithOpinion[i];
            string opinionLabel = GetOpinionLabel(otherCharacter);
            for (int j = 0; j < labels.Length; j++) {
                if (labels[j] == opinionLabel) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool HasOpinionLabelWithCharacter(Character character, params string[] labels) {
        if (HasRelationshipWith(character)) {
            string opinionLabel = GetOpinionLabel(character);
            for (int j = 0; j < labels.Length; j++) {
                if (labels[j] == opinionLabel) {
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
                    return string.IsNullOrEmpty(opinionLabel) == false ? opinionLabel : Acquaintance;
                default:
                    return UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetterOnly(relType.ToString());
            }
        }
        return Acquaintance;
    }
    #endregion
}
