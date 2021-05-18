using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;
namespace Traits {
    public class Unfaithful : Trait {
        
        public Unfaithful() {
            name = "Unfaithful";
            description = "Cannot commit to a monogamous relationship.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            canBeTriggered = true;
        }

        #region Overrides
        public override string TriggerFlaw(Character character) {
            string successLogKey = base.TriggerFlaw(character);
            int loverID = character.relationshipContainer.GetFirstRelatableIDWithRelationship(RELATIONSHIP_TYPE.LOVER); 
            if (loverID != -1) {
                List<int> affairIDs = RuinarchListPool<int>.Claim();
                character.relationshipContainer.PopulateAllRelatableIDWithRelationship(affairIDs, RELATIONSHIP_TYPE.AFFAIR);
                Character aliveAffair = null;
                for (int i = 0; i < affairIDs.Count; i++) {
                    int affairID = affairIDs[i];
                    Character affair = CharacterManager.Instance.GetCharacterByID(affairID);
                    if (affair != null && !affair.isDead) {
                        aliveAffair = affair;
                        break;
                    }
                }
                RuinarchListPool<int>.Release(affairIDs);
                if (aliveAffair == null) {
                    if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW)) {
                        List<Character> choices = new List<Character>();
                        //choose from characters that owner has a relationship with, that is not their lover or affair and is still alive.
                        foreach (var relationship in character.relationshipContainer.relationships) {
                            if (relationship.Key != loverID && relationship.Value.HasRelationship(RELATIONSHIP_TYPE.AFFAIR) == false) {
                                Character otherCharacter = CharacterManager.Instance.GetCharacterByID(relationship.Key);
                                if (otherCharacter != null && otherCharacter.isDead == false) {
                                    if (RelationshipManager.IsSexuallyCompatible(character, otherCharacter) && 
                                        RelationshipManager.Instance.GetValidator(character).CanHaveRelationship(character, otherCharacter, RELATIONSHIP_TYPE.AFFAIR)) {
                                        choices.Add(otherCharacter);
                                    }
                                }
                            }
                        }

                        if (choices.Count > 0) {
                            //If no affair yet, the character will create a Have Affair Job which will attempt to have an affair with a viable target.
                            Character chosenCharacter = CollectionUtilities.GetRandomElement(choices);
                            GoapPlanJob cheatJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.HAVE_AFFAIR, chosenCharacter, character);
                            character.jobQueue.AddJobInQueue(cheatJob);
                            return successLogKey;
                        } else {
                            return "fail_no_affair";
                        }
                    }
                } else {
                    if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW)) {
                        //If already has a affair, the character will attempt to make love with one.
                        GoapPlanJob cheatJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.MAKE_LOVE, aliveAffair, character);
                        character.jobQueue.AddJobInQueue(cheatJob);
                    }
                }
                return successLogKey;
            } else {
                return "no_spouse";
            }
        }
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character targetCharacter) {
                targetCharacter.behaviourComponent.AddBehaviourComponent(typeof(UnfaithfulBehaviour));
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character targetCharacter) {
                targetCharacter.behaviourComponent.RemoveBehaviourComponent(typeof(UnfaithfulBehaviour));
            }
        }
        #endregion

        public bool IsCompatibleBasedOnSexualityAndOpinions(Character p_character1, Character p_character2) {
            if (p_character1.HasAfflictedByPlayerWith(this)) {
                if (PlayerSkillManager.Instance.HasAfflictionAddedBehaviourForSkillAtCurrentLevel(PLAYER_SKILL_TYPE.UNFAITHFULNESS, AFFLICTION_SPECIFIC_BEHAVIOUR.Wild_Multiple_Affair)) {
                    return !p_character1.relationshipContainer.IsEnemiesWith(p_character2);
                } else {
                    return RelationshipManager.IsSexuallyCompatible(p_character1, p_character2) && !p_character1.relationshipContainer.IsEnemiesWith(p_character2);
                }
            } else {
                return RelationshipManager.IsSexuallyCompatible(p_character1, p_character2) && !p_character1.relationshipContainer.IsEnemiesWith(p_character2);
            }
        }
        /// <summary>
        /// Check if a character is willing to form an intimate relationship with a specific character.
        /// This checks if character 1 is okay with having relationships with family members, animals, etc.
        /// </summary>
        /// <param name="p_character1">The character to check the personal constraints of.</param>
        /// <param name="p_character2">The target character to check against.</param>
        /// <returns>True or False</returns>
        public  bool CanBeLoverOrAffairBasedOnPersonalConstraints(Character p_character1, Character p_character2) {
            if (p_character1.HasAfflictedByPlayerWith(this)) {
                if (PlayerSkillManager.Instance.HasAfflictionAddedBehaviourForSkillAtCurrentLevel(PLAYER_SKILL_TYPE.UNFAITHFULNESS, AFFLICTION_SPECIFIC_BEHAVIOUR.Wild_Multiple_Affair)) {
                    return true; //can have affair with anyone
                } else if (PlayerSkillManager.Instance.HasAfflictionAddedBehaviourForSkillAtCurrentLevel(PLAYER_SKILL_TYPE.UNFAITHFULNESS, AFFLICTION_SPECIFIC_BEHAVIOUR.Multiple_Affair)) {
                    //can have multiple affairs but should respect familial relationships
                    return !p_character1.relationshipContainer.IsFamilyMember(p_character2);
                } else {
                    //can only have 1 affair
                    if (p_character1.relationshipContainer.GetRelatablesWithRelationshipCount(RELATIONSHIP_TYPE.AFFAIR) > 0) {
                        return false;
                    }
                    //if target is not a family member, then allow affair
                    return !p_character1.relationshipContainer.IsFamilyMember(p_character2);
                }
            } else {
                //can only have 1 affair
                if (p_character1.relationshipContainer.GetRelatablesWithRelationshipCount(RELATIONSHIP_TYPE.AFFAIR) > 0) {
                    return false;
                }
                //if target is not a family member, then allow affair
                return !p_character1.relationshipContainer.IsFamilyMember(p_character2);
            }
        }
        

    }
}

