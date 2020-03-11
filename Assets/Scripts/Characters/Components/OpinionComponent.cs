using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;

public class OpinionComponent {
    public const string Close_Friend = "Close Friend";
    public const string Friend = "Friend";
    public const string Acquaintance = "Acquaintance";
    public const string Enemy = "Enemy";
    public const string Rival = "Rival";

    private const int Friend_Requirement = 1; //opinion requirement to consider someone a friend
    private const int Enemy_Requirement = -1; //opinion requirement to consider someone an enemy

    public const int MaxCompatibility = 5;
    public const int MinCompatibility = 0;
    
    public Character owner { get; }
    public Dictionary<Character, OpinionData> opinions { get; }
    public List<Character> charactersWithOpinion { get; } //Made a list of all characters with opinion to lessen CPU load

    public OpinionComponent(Character owner) {
        this.owner = owner;
        opinions = new Dictionary<Character, OpinionData>();
        charactersWithOpinion = new List<Character>();
    }

    public void AdjustOpinion(Character target, string opinionText, int opinionValue, string lastStrawReason = "") {
        if (owner.minion != null || owner is Summon) {
            //Minions or Summons cannot have opinions
            return;
        }
        if (!HasOpinion(target)) {
            OpinionData opinionData = ObjectPoolManager.Instance.CreateNewOpinionData(); 
            opinions.Add(target, opinionData);
            // opinionData.OnInitiallyAdded(target);
            charactersWithOpinion.Add(target);

            // //Note: I did this because compatibility value between two characters must only be 1 instance, but since we have compatibilityValue variable per opinion, it now has 2 instances
            // //In order for us to be sure that only 1 compatibility value is set, we check if the newly added target to the opinion already has opinion towards the owner, then it must mean that there is already a compatibility value, that's why we set it to 0
            // if (!target.opinionComponent.HasOpinion(owner)) {
            //     opinions[target].OnInitiallyAdded();
            // } else {
            //     opinions[target].SetCompatibilityValue(0);
            // }
            Messenger.Broadcast(Signals.OPINION_ADDED, owner, target);
        }
        if (owner.traitContainer.HasTrait("Psychopath")) {
            Psychopath serialKiller = owner.traitContainer.GetNormalTrait<Psychopath>("Psychopath");
            serialKiller.AdjustOpinion(target, opinionText, opinionValue);
            //Psychopaths do not gain or lose Opinion towards other characters (ensure that logs related to Opinion changes also do not show up)
            owner.logComponent.PrintLogIfActive(
                $"{owner.name} wants to adjust {opinionText} opinion towards {target.name} by {opinionValue} but {owner.name} is a Serial Killer");
            opinionValue = 0;
        }
        opinions[target].AdjustOpinion(opinionText, opinionValue);
        if (opinionValue > 0) {
            Messenger.Broadcast(Signals.OPINION_INCREASED, owner, target, lastStrawReason);
        } else if (opinionValue < 0) {
            Messenger.Broadcast(Signals.OPINION_DECREASED, owner, target, lastStrawReason);
        }
        if (!target.relationshipContainer.HasRelationshipWith(owner)) {
            target.relationshipContainer.AdjustOpinion(target, owner, "Base", 0);
        }
    }
    public void SetOpinion(Character target, string opinionText, int opinionValue, string lastStrawReason = "") {
        if (owner.minion != null || owner is Summon) {
            //Minions or Summons cannot have opinions
            return;
        }
        if (!HasOpinion(target)) {
            OpinionData opinionData = ObjectPoolManager.Instance.CreateNewOpinionData(); 
            opinions.Add(target, opinionData);
            // opinionData.OnInitiallyAdded(target);
            charactersWithOpinion.Add(target);

            // //Note: I did this because compatibility value between two characters must only be 1 instance, but since we have compatibilityValue variable per opinion, it now has 2 instances
            // //In order for us to be sure that only 1 compatibility value is set, we check if the newly added target to the opinion already has opinion towards the owner, then it must mean that there is already a compatibility value, that's why we set it to 0
            // if (!target.opinionComponent.HasOpinion(owner)) {
            //     opinions[target].OnInitiallyAdded();
            // } else {
            //     opinions[target].SetCompatibilityValue(0);
            // }
            Messenger.Broadcast(Signals.OPINION_ADDED, owner, target);
        }
        if (owner.traitContainer.HasTrait("Psychopath")) {
            //Psychopaths do not gain or lose Opinion towards other characters (ensure that logs related to Opinion changes also do not show up)
            owner.logComponent.PrintLogIfActive(
                $"{owner.name} wants to adjust {opinionText} opinion towards {target.name} by {opinionValue} but {owner.name} is a Serial Killer, setting the value to zero...");
            opinionValue = 0;
        }
        opinions[target].SetOpinion(opinionText, opinionValue);
        if (opinionValue > 0) {
            Messenger.Broadcast(Signals.OPINION_INCREASED, owner, target, lastStrawReason);
        } else if (opinionValue < 0) {
            Messenger.Broadcast(Signals.OPINION_DECREASED, owner, target, lastStrawReason);
        }
        if (!target.relationshipContainer.HasRelationshipWith(owner)) {
            target.relationshipContainer.SetOpinion(target, owner, "Base", 0);
        }
    }
    public void RemoveOpinion(Character target, string opinionText) {
        if (HasOpinion(target)) {
            opinions[target].RemoveOpinion(opinionText);
        }
    }
    public void RemoveOpinion(Character target) {
        if (HasOpinion(target)) {
            OpinionData data = opinions[target];
            opinions.Remove(target);
            charactersWithOpinion.Remove(target);
            Messenger.Broadcast(Signals.OPINION_REMOVED, owner, target);
            ObjectPoolManager.Instance.ReturnOpinionDataToPool(data);
        }
    }
    public bool HasOpinion(Character target) {
        return opinions.ContainsKey(target);
    }
    public bool HasOpinion(Character target, string opinionText) {
        return opinions.ContainsKey(target) && opinions[target].HasOpinion(opinionText);
    }
    public int GetTotalOpinion(Character target) {
        return opinions[target].totalOpinion;
    }
    public OpinionData GetOpinionData(Character target) {
        return opinions[target];
    }
    public string GetOpinionLabel(Character target) {
        if (HasOpinion(target)) {
            int totalOpinion = GetTotalOpinion(target);
            if (totalOpinion > 70) {
                return Close_Friend;
            } else if (totalOpinion > 20 && totalOpinion <= 70) {
                return Friend;
            } else if (totalOpinion > -21 && totalOpinion <= 20) {
                return Acquaintance;
            } else if (totalOpinion > -71 && totalOpinion <= -21) {
                return Enemy;
            } else {
                return Rival;
            }
        }
        return string.Empty;
    }

    #region Inquiry
    public bool IsFriendsWith(Character character) {
        string opinionLabel = GetOpinionLabel(character);
        return opinionLabel == Friend || opinionLabel == Close_Friend;
        //if (HasOpinion(character)) {
        //    return GetTotalOpinion(character) >= Friend_Requirement;
        //}
        //return false;
    }
    public bool IsEnemiesWith(Character character) {
        string opinionLabel = GetOpinionLabel(character);
        return opinionLabel == Enemy || opinionLabel == Rival;
        //if (HasOpinion(character)) {
        //    return GetTotalOpinion(character) <= Enemy_Requirement;
        //}
        //return false;
    }
    #endregion

    #region Data Getting
    public List<Character> GetCharactersWithPositiveOpinion() {
        List<Character> characters = new List<Character>();
        //List<Character> charactersWithOpinion = opinions.Keys.ToList();
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
        //List<Character> charactersWithOpinion = opinions.Keys.ToList();
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
        //List<Character> charactersWithOpinion = opinions.Keys.ToList();
        for (int i = 0; i < charactersWithOpinion.Count; i++) {
            Character otherCharacter = charactersWithOpinion[i];
            if (GetTotalOpinion(otherCharacter) < 0) {
                characters.Add(otherCharacter);
            }
        }
        return characters;
    }
    public List<Character> GetEnemyCharacters() {
        List<Character> characters = new List<Character>();
        //List<Character> charactersWithOpinion = opinions.Keys.ToList();
        for (int i = 0; i < charactersWithOpinion.Count; i++) {
            Character otherCharacter = charactersWithOpinion[i];
            if (IsEnemiesWith(otherCharacter)) {
                characters.Add(otherCharacter);
            }
        }
        return characters;
    }
    public List<Character> GetFriendCharacters() {
        List<Character> characters = new List<Character>();
        //List<Character> charactersWithOpinion = opinions.Keys.ToList();
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
        if (HasOpinion(character)) {
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
        if (HasOpinion(character)) {
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
        if (HasOpinion(target)) {
            return opinions[target].compatibilityValue;
        }
        return -1;
    }
    public string GetRelationshipNameWith(Character target) {
        if (owner.relationshipContainer.HasRelationshipWith(target)) {
            IRelationshipData data = owner.relationshipContainer.GetRelationshipDataWith(target);
            RELATIONSHIP_TYPE relType = data.GetFirstMajorRelationship();
            if (relType == RELATIONSHIP_TYPE.CHILD) {
                if (target.gender == GENDER.MALE) {
                    return "Son";
                } else {
                    return "Daughter";
                }
            } else if (relType == RELATIONSHIP_TYPE.PARENT) {
                if (target.gender == GENDER.MALE) {
                    return "Father";
                } else {
                    return "Mother";
                }
            } else if (relType == RELATIONSHIP_TYPE.SIBLING) {
                if (target.gender == GENDER.MALE) {
                    return "Brother";
                } else {
                    return "Sister";
                }
            } else if (relType == RELATIONSHIP_TYPE.LOVER) {
                if (target.gender == GENDER.MALE) {
                    return "Husband";
                } else {
                    return "Wife";
                }
            }
            return UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetterOnly(relType.ToString());    
        } else if (HasOpinion(target)) {
            return GetOpinionLabel(target);
        }
        return Acquaintance;
    }
    #endregion
}