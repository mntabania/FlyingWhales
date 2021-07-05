using System;
using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
using Random = UnityEngine.Random;

public class RelationshipManager : BaseMonoBehaviour {

    public static RelationshipManager Instance;

    private IRelationshipValidator _characterRelationshipValidator;
    private IRelationshipProcessor _characterRelationshipProcessor;
    
    public const string Close_Friend = "Close Friend";
    public const string Friend = "Friend";
    public const string Acquaintance = "Acquaintance";
    public const string Enemy = "Enemy";
    public const string Rival = "Rival";
    
    public const int MaxCompatibility = 5;
    public const int MinCompatibility = 0;
    

    void Awake() {
        Instance = this;
        _characterRelationshipValidator = new CharacterRelationshipValidator();
        //processors
        _characterRelationshipProcessor = new CharacterRelationshipProcessor();
    }
    protected override void OnDestroy() {
        base.OnDestroy();
        Instance = null;
    }

    #region Containers
    public IRelationshipContainer CreateRelationshipContainer(Relatable relatable) {
        if (relatable is IPointOfInterest) {
            return new BaseRelationshipContainer();
        }
        return null;
    }
    #endregion

    #region Validators
    public IRelationshipValidator GetValidator(Relatable obj) {
        if (obj is Character) {
            return _characterRelationshipValidator;
        }
        throw new Exception($"There is no relationship validator for {obj.relatableName}");
    }
    public bool CanHaveRelationship(Relatable rel1, Relatable rel2, RELATIONSHIP_TYPE rel) {
        IRelationshipValidator validator = GetValidator(rel1);
        if (validator != null) {
            return validator.CanHaveRelationship(rel1, rel2, rel);
        }
        return false; //if no validator, then do not allow
    }
    #endregion

    /// <summary>
    /// Add a one way relationship to a character.
    /// </summary>
    /// <param name="currCharacter">The character that will gain the relationship.</param>
    /// <param name="targetCharacter">The character that the new relationship is targetting.</param>
    /// <param name="rel">The type of relationship to create.</param>
    /// <param name="triggerOnAdd">Should this trigger the trait's OnAdd Function.</param>
    /// <returns>The created relationship data.</returns>
    private RELATIONSHIP_TYPE GetPairedRelationship(RELATIONSHIP_TYPE rel) {
        switch (rel) {
            case RELATIONSHIP_TYPE.RELATIVE:
                return RELATIONSHIP_TYPE.RELATIVE;
            case RELATIONSHIP_TYPE.LOVER:
                return RELATIONSHIP_TYPE.LOVER;
            case RELATIONSHIP_TYPE.AFFAIR:
                return RELATIONSHIP_TYPE.AFFAIR;
            case RELATIONSHIP_TYPE.EX_LOVER:
                return RELATIONSHIP_TYPE.EX_LOVER;
            default:
                return RELATIONSHIP_TYPE.NONE;
        }
    }
    public bool IsCompatibleBasedOnSexualityAndOpinion(Character p_character1, Character p_character2) {
        Unfaithful unfaithful = p_character1.traitContainer.GetTraitOrStatus<Unfaithful>("Unfaithful");
        if (unfaithful == null) {
            return IsSexuallyCompatible(p_character1, p_character2);
        } else {
            return unfaithful.IsCompatibleBasedOnSexualityAndOpinions(p_character1, p_character2);
        }
    }
    public bool CanHaveRelationship(Character p_character1, Character p_character2, RELATIONSHIP_TYPE p_rel) {
        if (GetValidator(p_character1).CanHaveRelationship(p_character2, p_character1, p_rel) &&
            GetValidator(p_character2).CanHaveRelationship(p_character1, p_character2, p_rel)) {
            return true;
        }
        return false;
    }
    /// <summary>
    /// Check whether or not 2 characters are sexually compatible.
    /// NOTE: This takes into account some traits.
    /// </summary>
    /// <returns>True or false.</returns>
    public static bool IsSexuallyCompatible(Character character1, Character character2) {
        return IsSexuallyCompatibleOneSided(character1, character2) && IsSexuallyCompatibleOneSided(character2, character1);
    }
    /// <summary>
    /// Check if character1 is sexually compatible with character2. NOT Vice versa.
    /// Use <see cref="IsSexuallyCompatible(Character,Character)"/> for 2 way checking.
    /// NOTE: This takes into account some traits.
    /// </summary>
    /// <param name="character1">The side to check.</param>
    /// /<param name="character2">The side to compare to.</param>
    /// <returns>True or false.</returns>
    public static bool IsSexuallyCompatibleOneSided(Character character1, Character character2) {
        if (IsSexuallyCompatibleOneSided(character1.sexuality, character2.sexuality, character1.gender, character2.gender)) {
            //if a character is hemophobic, and the other character is a vampire, check if the hemophobic character knows that it is a vampire. If it does then the 2 are incompatible.
            if (character1.traitContainer.HasTrait("Hemophobic")) {
                Vampire vampire = character2.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
                if (vampire != null && vampire.DoesCharacterKnowThisVampire(character1)) {
                    return false;    
                }
            }
            //if a character is lycanphobic, and the other character is a lycanthrope, check if the lycanphobic character knows that it is a lycan. If it does then the 2 are incompatible.
            if (character1.traitContainer.HasTrait("Lycanphobic")) {
                if (character2.isLycanthrope && character2.lycanData.DoesCharacterKnowThisLycan(character1)) {
                    return false;    
                }
            }
            return true;
        }
        return false;
    }
    public static bool IsSexuallyCompatible(SEXUALITY sexuality1, SEXUALITY sexuality2, GENDER gender1, GENDER gender2) {
        bool sexuallyCompatible = IsSexuallyCompatibleOneSided(sexuality1, sexuality2, gender1, gender2);
        if (!sexuallyCompatible) {
            return false; //if they are already sexually incompatible in one side, return false
        }
        sexuallyCompatible = IsSexuallyCompatibleOneSided(sexuality2, sexuality1, gender1, gender2);
        return sexuallyCompatible;
    }
    private static bool IsSexuallyCompatibleOneSided(SEXUALITY sexuality1, SEXUALITY sexuality2, GENDER gender1, GENDER gender2) {
        switch (sexuality1) {
            case SEXUALITY.STRAIGHT:
                return gender1 != gender2;
            case SEXUALITY.BISEXUAL:
                return true; //because bisexuals are attracted to both genders.
            case SEXUALITY.GAY:
                return gender1 == gender2;
            default:
                return false;
        }
    }
    public void ApplyPreGeneratedRelationships(MapGenerationData data, PreCharacterData characterData, Character character) {
        Assert.IsTrue(characterData.id == character.id, $"Provided character data and character are inconsistent {character.name}");
        foreach (var kvp in characterData.relationships) {
            PreCharacterData targetCharacterData = DatabaseManager.Instance.familyTreeDatabase.GetCharacterWithID(kvp.Key);
            IRelationshipData relationshipData = character.relationshipContainer.GetOrCreateRelationshipDataWith(character, targetCharacterData.id, 
                targetCharacterData.firstName, targetCharacterData.gender);

            character.relationshipContainer.SetOpinion(character, targetCharacterData.id, targetCharacterData.firstName, targetCharacterData.gender, 
                "Base", kvp.Value.baseOpinion, true);

            relationshipData.opinions.SetCompatibilityValue(kvp.Value.compatibility);

            for (int k = 0; k < kvp.Value.relationships.Count; k++) {
                RELATIONSHIP_TYPE relationshipType = kvp.Value.relationships[k];
                relationshipData.AddRelationship(relationshipType);
            }
        }
    }

    #region Adding
    public IRelationshipData CreateNewRelationshipBetween(Relatable rel1, Relatable rel2, RELATIONSHIP_TYPE rel) {
        RELATIONSHIP_TYPE pair = GetPairedRelationship(rel);
        if (CanHaveRelationship(rel1, rel2, rel)) {
            rel1.relationshipContainer.AddRelationship(rel1,rel2, rel);
            rel1.relationshipProcessor?.OnRelationshipAdded(rel1, rel2, rel);
        }
        if (CanHaveRelationship(rel2, rel1, rel)) {
            rel2.relationshipContainer.AddRelationship(rel2, rel1, pair);
            rel2.relationshipProcessor?.OnRelationshipAdded(rel2, rel1, pair);
        }
        if (rel == RELATIONSHIP_TYPE.AFFAIR) {
            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Generic", "Affair", null, LogUtilities.Social_Life_Changes_Tags);
            log.AddToFillers(rel1 as Character, rel1.relatableName, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(rel2 as Character, rel2.relatableName, LOG_IDENTIFIER.TARGET_CHARACTER);
            log.AddLogToDatabase(true);
        }
        return rel1.relationshipContainer.GetRelationshipDataWith(rel2);
    }
    public void CreateNewRelationshipDataBetween(Relatable rel1, Relatable rel2) {
        IRelationshipData relationshipData1 = rel1.relationshipContainer.GetOrCreateRelationshipDataWith(rel1, rel2);
        IRelationshipData relationshipData2 = rel2.relationshipContainer.GetOrCreateRelationshipDataWith(rel2, rel1);

        int randomCompatibility = UnityEngine.Random.Range(MinCompatibility, MaxCompatibility);
                        
        relationshipData1.opinions.SetCompatibilityValue(randomCompatibility);
        relationshipData2.opinions.SetCompatibilityValue(randomCompatibility);

        relationshipData1.opinions.RandomizeBaseOpinionBasedOnCompatibility();
        relationshipData2.opinions.RandomizeBaseOpinionBasedOnCompatibility();
    }
    #endregion

    #region Removing
    public void RemoveRelationshipBetween(Relatable rel1, Relatable rel2, RELATIONSHIP_TYPE rel) {
        if (!rel1.relationshipContainer.relationships.ContainsKey(rel2.id)
            || !rel2.relationshipContainer.relationships.ContainsKey(rel1.id)) {
            return;
        }
        RELATIONSHIP_TYPE pair = GetPairedRelationship(rel);
        if (rel1.relationshipContainer.relationships[rel2.id].HasRelationship(rel)
            && rel2.relationshipContainer.relationships[rel1.id].HasRelationship(pair)) {

            rel1.relationshipContainer.RemoveRelationship(rel2, rel);
            rel1.relationshipProcessor?.OnRelationshipRemoved(rel1, rel2, rel);
            rel2.relationshipContainer.RemoveRelationship(rel1, pair);
            rel2.relationshipProcessor?.OnRelationshipRemoved(rel2, rel1, pair);
            Messenger.Broadcast(CharacterSignals.RELATIONSHIP_REMOVED, rel1, rel, rel2);
        }
    }
    #endregion

    #region Relationship Improvement
    public bool RelationshipImprovement(Character actor, Character target, GoapAction cause = null) {
        if (actor.race == RACE.DEMON || target.race == RACE.DEMON || actor is Summon || target is Summon) {
            return false; //do not let demons and summons have relationships
        }
        if (actor.hasBeenRaisedFromDead || target.hasBeenRaisedFromDead) {
            return false; //do not let zombies or skeletons develop other relationships
        }
        string summary = $"Relationship improvement between {actor.name} and {target.name}";
        bool hasImproved = false;
        // Log log = null;
        // if (target.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TYPE.ENEMY)) {
        //     //If Actor and Target are Enemies, 25% chance to remove Enemy relationship. If so, Target now considers Actor a Friend.
        //     summary += "\n" + target.name + " considers " + actor.name + " an enemy. Rolling for chance to consider as a friend...";
        //     int roll = UnityEngine.Random.Range(0, 100);
        //     summary += "\nRoll is " + roll.ToString();
        //     if (roll < 25) {
        //         if (target.traitContainer.GetNormalTrait<Trait>("Psychopath") == null) {
        //             log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "enemy_now_friend");
        //             summary += target.name + " now considers " + actor.name + " an enemy.";
        //             RemoveOneWayRelationship(target, actor, RELATIONSHIP_TYPE.ENEMY);
        //             CreateNewOneWayRelationship(target, actor, RELATIONSHIP_TYPE.FRIEND);
        //             hasImproved = true;
        //         }
        //     }
        // }
        // //If character is already a Friend, will not change actual relationship but will consider it improved
        // else if (target.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TYPE.FRIEND)) {
        //     hasImproved = true;
        // } else if (!target.relationshipContainer.HasRelationshipWith(actor)) {
        //     if (target.traitContainer.GetNormalTrait<Trait>("Psychopath") == null) {
        //         log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "now_friend");
        //         summary += "\n" + target.name + " has no relationship with " + actor.name + ". " + target.name + " now considers " + actor.name + " a friend.";
        //         //If Target has no relationship with Actor, Target now considers Actor a Friend.
        //         CreateNewOneWayRelationship(target, actor, RELATIONSHIP_TYPE.FRIEND);
        //         hasImproved = true;
        //     }
        // }
        // Debug.Log(summary);
        // if (log != null) {
        //     log.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //     log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        //     PlayerManager.Instance.player.ShowNotificationFrom(log, target, actor);
        // }
        return hasImproved;
    }
    #endregion

    #region Relationship Degradation
    /// <summary>
    /// Unified way of degrading a relationship of a character with a target character.
    /// </summary>
    /// <param name="actor">The character that did something to degrade the relationship.</param>
    /// <param name="target">The character that will change their relationship with the actor.</param>
    //public bool RelationshipDegradation(Character actor, Character target, ActualGoapNode cause = null) {
    //    return RelationshipDegradation(actor.currentAlterEgo, target, cause);
    //}
    public bool RelationshipDegradation(Character actor, Character target, ActualGoapNode cause = null) {
        if (actor.race == RACE.DEMON || target.race == RACE.DEMON || actor is Summon || target is Summon) {
            return false; //do not let demons and summons have relationships
        }
        if (actor.hasBeenRaisedFromDead || target.hasBeenRaisedFromDead) {
            return false; //do not let zombies or skeletons develop other relationships
        }

        bool hasDegraded = false;
        if (actor.isFactionless || target.isFactionless) {
            Debug.LogWarning("Relationship degredation was called and one or both of those characters is factionless");
            return hasDegraded;
        }
        if (actor == target) {
            Debug.LogWarning($"Relationship degredation was called and provided same characters {target.name}");
            return hasDegraded;
        }
        if (target.traitContainer.HasTrait("Diplomatic")) {
            Debug.LogWarning($"Relationship degredation was called but {target.name} is Diplomatic");
            hasDegraded = true;
            return hasDegraded;
        }

        string opinionText = "Relationship Degradation";
        if (cause != null) {
            opinionText = cause.goapName;
        }
        
        actor.relationshipContainer.AdjustOpinion(actor, target, opinionText, -10);
        target.relationshipContainer.AdjustOpinion(target, actor, opinionText, -10);
        
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "rel_degrade", null, LOG_TAG.Social);
        log.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        log.AddLogToDatabase();
        // PlayerManager.Instance.player.ShowNotificationFrom(log, target, actor);
        hasDegraded = true;
        
        // string summary = "Relationship degradation between " + actorAlterEgo.owner.name + " and " + target.name;
        //if (cause != null && cause.IsFromApprehendJob()) {
        //    //If this has been triggered by an Action's End Result that is part of an Apprehend Job, skip processing.
        //    summary += "Relationship degradation was caused by an action in an apprehend job. Skipping degredation...";
        //    Debug.Log(summary);
        //    return hasDegraded;
        //}
        //If Actor and Target are Lovers, 25% chance to create a Break Up Job with the Lover.
        //if (target.relationshipContainer.HasRelationshipWith(actorAlterEgo, RELATIONSHIP_TRAIT.LOVER)) {
        //    summary += "\n" + actorAlterEgo.owner.name + " and " + target.name + " are  lovers. Rolling for chance to create break up job...";
        //    int roll = UnityEngine.Random.Range(0, 100);
        //    summary += "\nRoll is " + roll.ToString();
        //    if (roll < 25) {
        //        summary += "\n" + target.name + " created break up job targetting " + actorAlterEgo.owner.name;
        //        target.CreateBreakupJob(actorAlterEgo.owner);

        //        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "break_up");
        //        log.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //        log.AddToFillers(actorAlterEgo.owner, actorAlterEgo.owner.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        //        PlayerManager.Instance.player.ShowNotificationFrom(log, target, actorAlterEgo.owner);
        //        hasDegraded = true;
        //    }
        //}
        ////If Actor and Target are Affairs, 25% chance to create a Break Up Job with the Paramour.
        //else if (target.relationshipContainer.HasRelationshipWith(actorAlterEgo, RELATIONSHIP_TRAIT.AFFAIR)) {
        //    summary += "\n" + actorAlterEgo.owner.name + " and " + target.name + " are  affairs. Rolling for chance to create break up job...";
        //    int roll = UnityEngine.Random.Range(0, 100);
        //    summary += "\nRoll is " + roll.ToString();
        //    if (roll < 25) {
        //        summary += "\n" + target.name + " created break up job targetting " + actorAlterEgo.owner.name;
        //        target.CreateBreakupJob(actorAlterEgo.owner);

        //        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "break_up");
        //        log.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //        log.AddToFillers(actorAlterEgo.owner, actorAlterEgo.owner.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        //        PlayerManager.Instance.player.ShowNotificationFrom(log, target, actorAlterEgo.owner);
        //        hasDegraded = true;
        //    }
        //}

        // //If Target considers Actor a Friend, remove that. If Target is in Bad or Dark Mood, Target now considers Actor an Enemy. Otherwise, they are just no longer friends.
        // if (target.relationshipContainer.HasRelationshipWith(actorAlterEgo, RELATIONSHIP_TYPE.FRIEND)) {
        //     summary += "\n" + target.name + " considers " + actorAlterEgo.name + " as a friend. Removing friend and replacing with enemy";
        //     RemoveOneWayRelationship(target, actorAlterEgo, RELATIONSHIP_TYPE.FRIEND);
        //     if (target.currentMoodType == CHARACTER_MOOD.BAD || target.currentMoodType == CHARACTER_MOOD.DARK) {
        //         CreateNewOneWayRelationship(target, actorAlterEgo, RELATIONSHIP_TYPE.ENEMY);
        //         Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "friend_now_enemy");
        //         log.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //         log.AddToFillers(actorAlterEgo.owner, actorAlterEgo.owner.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        //         PlayerManager.Instance.player.ShowNotificationFrom(log, target, actorAlterEgo.owner);
        //         hasDegraded = true;
        //     } else {
        //         Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "no_longer_friend");
        //         log.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //         log.AddToFillers(actorAlterEgo.owner, actorAlterEgo.owner.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        //         PlayerManager.Instance.player.ShowNotificationFrom(log, target, actorAlterEgo.owner);
        //         hasDegraded = true;
        //     }
        // }
        // //If character is already an Enemy, will not change actual relationship but will consider it degraded
        // else if (target.relationshipContainer.HasRelationshipWith(actorAlterEgo, RELATIONSHIP_TYPE.ENEMY)) {
        //     hasDegraded = true;
        // }
        // //If Target is only Relative of Actor(no other relationship) or has no relationship with Actor, Target now considers Actor an Enemy.
        // else if (!target.relationshipContainer.HasRelationshipWith(actorAlterEgo) || (target.relationshipContainer.HasRelationshipWith(actorAlterEgo, RELATIONSHIP_TYPE.RELATIVE) && target.relationshipContainer.GetRelationshipDataWith(actorAlterEgo).relationships.Count == 1)) {
        //     summary += "\n" + target.name + " and " + actorAlterEgo.owner.name + " has no relationship or only has relative relationship. " + target.name + " now considers " + actorAlterEgo.owner.name + " an enemy.";
        //     CreateNewOneWayRelationship(target, actorAlterEgo, RELATIONSHIP_TYPE.ENEMY);
        //
        //     Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "now_enemy");
        //     log.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //     log.AddToFillers(actorAlterEgo.owner, actorAlterEgo.owner.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        //     PlayerManager.Instance.player.ShowNotificationFrom(log, target, actorAlterEgo.owner);
        //     hasDegraded = true;
        // }


        // Debug.Log(summary);
        return hasDegraded;
    }
    #endregion

    #region Processors
    public IRelationshipProcessor GetProcessor(Relatable relatable) {
        if (relatable is Character) {
            return _characterRelationshipProcessor;
        }
        return null;
    }
    #endregion

    #region Compatibility
    public int GetCompatibilityBetween(Character character1, Character character2) {
        // int char1Compatibility = character1.relationshipContainer.GetCompatibility(character2);
        // int char2Compatibility = character2.relationshipContainer.GetCompatibility(character1);
        // if (char1Compatibility != -1 && char2Compatibility != -1) {
        //     return char1Compatibility + char2Compatibility;
        // }
        // return -1;
        return character1.relationshipContainer.GetCompatibility(character2); //since it is expected that both characters have the same compatibility values
    }
    public int GetCompatibilityBetween(Character character1, int target) {
        // int char1Compatibility = character1.relationshipContainer.GetCompatibility(character2);
        // int char2Compatibility = character2.relationshipContainer.GetCompatibility(character1);
        // if (char1Compatibility != -1 && char2Compatibility != -1) {
        //     return char1Compatibility + char2Compatibility;
        // }
        // return -1;
        return character1.relationshipContainer.GetCompatibility(target); //since it is expected that both characters have the same compatibility values
    }
    #endregion
}