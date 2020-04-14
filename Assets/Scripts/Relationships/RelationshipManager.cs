using System;
using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using Random = UnityEngine.Random;

public class RelationshipManager : MonoBehaviour {

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
            case RELATIONSHIP_TYPE.MASTER:
                return RELATIONSHIP_TYPE.SERVANT;
            case RELATIONSHIP_TYPE.SERVANT:
                return RELATIONSHIP_TYPE.MASTER;
            case RELATIONSHIP_TYPE.SAVER:
                return RELATIONSHIP_TYPE.SAVE_TARGET;
            case RELATIONSHIP_TYPE.SAVE_TARGET:
                return RELATIONSHIP_TYPE.SAVER;
            case RELATIONSHIP_TYPE.EX_LOVER:
                return RELATIONSHIP_TYPE.EX_LOVER;
            default:
                return RELATIONSHIP_TYPE.NONE;
        }
    }
    public void GenerateRelationships(List<Character> characters) {
        int maxInitialRels = 4;
        RELATIONSHIP_TYPE[] relsInOrder = new RELATIONSHIP_TYPE[] { RELATIONSHIP_TYPE.RELATIVE, RELATIONSHIP_TYPE.LOVER }; //RELATIONSHIP_TYPE.ENEMY, RELATIONSHIP_TYPE.FRIEND 

        // Loop through all characters in the world
        for (int i = 0; i < characters.Count; i++) {
            Character currCharacter = characters[i];
            if (currCharacter.isFactionless) {
                continue; //skip factionless characters
            }
            int currentRelCount = currCharacter.relationshipContainer.relationships.Count;
            if (currentRelCount >= maxInitialRels) {
                continue; //skip
            }
            int totalCreatedRels = currentRelCount;
            string summary = $"{currCharacter.name}({currCharacter.sexuality}) relationship generation summary:";

            //  Loop through all relationship types
            for (int k = 0; k < relsInOrder.Length; k++) {
                RELATIONSHIP_TYPE currRel = relsInOrder[k];
                if (totalCreatedRels >= maxInitialRels) {
                    summary +=
                        $"\nMax Initial Relationships reached, stopping relationship generation for {currCharacter.name}";
                    break; //stop generating more relationships for this character
                }
                int relsToCreate = 0;
                int chance = Random.Range(0, 100);

                // Compute the number of relations to create per relationship type
                switch (currRel) {
                    case RELATIONSHIP_TYPE.RELATIVE:
                        if (UtilityScripts.GameUtilities.IsRaceBeast(currCharacter.race)) { continue; } //a beast character has no relatives
                        // if (currCharacter.role.roleType == CHARACTER_ROLE.BEAST) { continue; } //a beast character has no relatives
                        else {
                            //- a non-beast character may have either zero (75%), one (20%) or two (5%) relatives from characters of the same race
                            if (chance < 75) relsToCreate = 0;
                            else if (chance >= 75 && chance < 95) relsToCreate = 1;
                            else relsToCreate = 2;
                        }
                        break;
                    case RELATIONSHIP_TYPE.LOVER:
                        //- a character has a 20% chance to have a lover
                        if (chance < 20) relsToCreate = 1;
                        //relsToCreate = 1;
                        break;
                }
                summary += $"\n===========Creating {relsToCreate} {currRel} Relationships...==========";


                if (relsToCreate > 0) {
                    WeightedFloatDictionary<Character> relWeights = new WeightedFloatDictionary<Character>();
                    // Loop through all characters that are in the same faction as the current character
                    for (int l = 0; l < currCharacter.faction.characters.Count; l++) {
                        Character otherCharacter = currCharacter.faction.characters[l];
                        if (currCharacter.id != otherCharacter.id) { //&& currCharacter.faction == otherCharacter.faction
                            List<RELATIONSHIP_TYPE> existingRelsOfCurrentCharacter = currCharacter.relationshipContainer.GetRelationshipDataWith(otherCharacter)?.relationships ?? null;
                            List<RELATIONSHIP_TYPE> existingRelsOfOtherCharacter = otherCharacter.relationshipContainer.GetRelationshipDataWith(currCharacter)?.relationships ?? null;
                            //if the current character already has a relationship of the same type with the other character, skip
                            if (existingRelsOfCurrentCharacter != null && existingRelsOfCurrentCharacter.Contains(currRel)) {
                                continue; //skip
                            }
                            float weight = 0;

                            // Compute the weight that determines how likely this character will have the current relationship type with current character
                            switch (currRel) {
                                case RELATIONSHIP_TYPE.RELATIVE:
                                    if (UtilityScripts.GameUtilities.IsRaceBeast(otherCharacter.race)) { continue; } //a beast character has no relatives
                                    else {
                                        if (otherCharacter.currentRegion == currCharacter.currentRegion) {
                                            // character is in same location: +50 Weight
                                            weight += 50;
                                        } else {
                                            //character is in different location: +10 Weight
                                            weight += 10;
                                        }

                                        if (currCharacter.race != otherCharacter.race) weight *= 0; //character is a different race: Weight x0
                                        if (currCharacter.faction != otherCharacter.faction) {
                                            weight *= 0; //disabled different faction positive relationships
                                        }
                                    }
                                    break;
                                case RELATIONSHIP_TYPE.LOVER:
                                    if (GetValidator(currCharacter).CanHaveRelationship(currCharacter, otherCharacter, currRel) && GetValidator(otherCharacter).CanHaveRelationship(otherCharacter, currCharacter, currRel)) {
                                        if (!UtilityScripts.GameUtilities.IsRaceBeast(currCharacter.race)) {
                                            //- if non beast, from valid characters, choose based on these weights
                                            if (otherCharacter.currentRegion == currCharacter.currentRegion) {
                                                //- character is in same location: +500 Weight
                                                weight += 500;
                                            } else {
                                                //- character is in different location: +5 Weight
                                                weight += 5;
                                            }
                                            if (currCharacter.race == otherCharacter.race) {
                                                //- character is the same race: Weight x5
                                                weight *= 5;
                                            }
                                            if (!IsSexuallyCompatible(currCharacter.sexuality, otherCharacter.sexuality, currCharacter.gender, otherCharacter.gender)) {
                                                //- character is sexually incompatible: Weight x0.1
                                                weight *= 0.05f;
                                            }
                                            if (UtilityScripts.GameUtilities.IsRaceBeast(otherCharacter.race)) {
                                                //- character is a beast: Weight x0
                                                weight *= 0;
                                            }
                                            if (existingRelsOfCurrentCharacter != null && existingRelsOfCurrentCharacter.Contains(RELATIONSHIP_TYPE.RELATIVE)) {
                                                //- character is a relative: Weight x0.1    
                                                weight *= 0.1f;
                                            }
                                            if (currCharacter.faction != otherCharacter.faction) {
                                                weight *= 0; //disabled different faction positive relationships
                                            }
                                        } else {
                                            //- if beast, from valid characters, choose based on these weights
                                            if (otherCharacter.currentRegion == currCharacter.currentRegion) {
                                                //- character is in same location: +50 Weight
                                                weight += 50;
                                            } else {
                                                // - character is in different location: +5 Weight
                                                weight += 5;
                                            }
                                            if (currCharacter.race != otherCharacter.race) {
                                                //- character is a different race: Weight x0
                                                weight *= 0;
                                            }
                                            if (currCharacter.gender != otherCharacter.gender) {
                                                //- character is the opposite gender: Weight x6
                                                weight *= 6;
                                            }
                                        }
                                    }
                                    break;
                            }
                            if (weight > 0f) {
                                relWeights.AddElement(otherCharacter, weight);
                            }
                        }
                    }
                    if (relWeights.GetTotalOfWeights() > 0) {
                        summary += $"\n{relWeights.GetWeightsSummary("Weights are: ")}";
                    } else {
                        summary += "\nThere are no valid characters to have a relationship with.";
                    }


                    for (int j = 0; j < relsToCreate; j++) {
                        if (relWeights.GetTotalOfWeights() > 0) {
                            Character chosenCharacter = relWeights.PickRandomElementGivenWeights();
                            CreateNewRelationshipBetween(currCharacter, chosenCharacter, currRel);
                            totalCreatedRels++;
                            summary +=
                                $"\nCreated new relationship {currRel} between {currCharacter.name} and {chosenCharacter.name}. Total relationships created for {currCharacter.name} are {totalCreatedRels}";
                            relWeights.RemoveElement(chosenCharacter);
                        } else {
                            break;
                        }
                        if (totalCreatedRels >= maxInitialRels) {
                            //summary += "\nMax Initial Relationships reached, stopping relationship generation for " + currCharacter.name;
                            break; //stop generating more relationships for this character
                        }
                    }
                }
            }
            Debug.Log(summary);
        }
    }
    public static bool IsSexuallyCompatible(SEXUALITY sexuality1, SEXUALITY sexuality2, GENDER gender1, GENDER gender2) {
        bool sexuallyCompatible = IsSexuallyCompatibleOneSided(sexuality1, sexuality2, gender1, gender2);
        if (!sexuallyCompatible) {
            return false; //if they are already sexually incompatible in one side, return false
        }
        sexuallyCompatible = IsSexuallyCompatibleOneSided(sexuality2, sexuality1, gender1, gender2);
        return sexuallyCompatible;
    }
    public static bool IsSexuallyCompatibleOneSided(SEXUALITY sexuality1, SEXUALITY sexuality2, GENDER gender1, GENDER gender2) {
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
            Log log = new Log(GameManager.Instance.Today(), "Character", "Generic", "Affair");
            log.AddToFillers(rel1 as Character, rel1.relatableName, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(rel2 as Character, rel2.relatableName, LOG_IDENTIFIER.TARGET_CHARACTER);
            log.AddLogToInvolvedObjects();
        }
        return rel1.relationshipContainer.GetRelationshipDataWith(rel2);
    }
    public void CreateNewRelationshipDataBetween(Relatable rel1, Relatable rel2) {
        IRelationshipData relationshipData1 = rel1.relationshipContainer.GetOrCreateRelationshipDataWith(rel1, rel2);
        IRelationshipData relationshipData2 = rel2.relationshipContainer.GetOrCreateRelationshipDataWith(rel2, rel1);

        int randomCompatibility = UnityEngine.Random.Range(RelationshipManager.MinCompatibility,
            RelationshipManager.MaxCompatibility);
                        
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
            Messenger.Broadcast(Signals.RELATIONSHIP_REMOVED, rel1, rel, rel2);
        }
    }
    #endregion

    #region Relationship Improvement
    public bool RelationshipImprovement(Character actor, Character target, GoapAction cause = null) {
        if (actor.race == RACE.DEMON || target.race == RACE.DEMON || actor is Summon || target is Summon) {
            return false; //do not let demons and summons have relationships
        }
        if (actor.returnedToLife || target.returnedToLife) {
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
        //             log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "enemy_now_friend");
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
        //         log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "now_friend");
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
        if (actor.returnedToLife || target.returnedToLife) {
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
        
        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "rel_degrade");
        log.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        log.AddLogToInvolvedObjects();
        // PlayerManager.Instance.player.ShowNotificationFrom(log, target, actor);
        hasDegraded = true;
        
        // string summary = "Relationship degradation between " + actorAlterEgo.owner.name + " and " + target.name;
        //TODO:
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

        //        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "break_up");
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

        //        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "break_up");
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
        //         Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "friend_now_enemy");
        //         log.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //         log.AddToFillers(actorAlterEgo.owner, actorAlterEgo.owner.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        //         PlayerManager.Instance.player.ShowNotificationFrom(log, target, actorAlterEgo.owner);
        //         hasDegraded = true;
        //     } else {
        //         Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "no_longer_friend");
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
        //     Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "now_enemy");
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