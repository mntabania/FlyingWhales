using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;
namespace Traits {
    public class Unfaithful : Trait {

        public float affairChanceMultiplier { get; private set; }

        public Unfaithful() {
            name = "Unfaithful";
            description = "Unfaithful characters are prone to having illicit love affairs.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            canBeTriggered = true;
        }

        #region Overrides
        protected override void OnChangeLevel() {
            base.OnChangeLevel();
            if (level == 1) {
                affairChanceMultiplier = 5f;
            } else if (level == 2) {
                affairChanceMultiplier = 10f;
            } else if (level == 3) {
                affairChanceMultiplier = 5f;
            }
        }
        public override string TriggerFlaw(Character character) {
            string successLogKey = base.TriggerFlaw(character);
            int loverID = character.relationshipContainer.GetFirstRelatableIDWithRelationship(RELATIONSHIP_TYPE.LOVER); 
            if (loverID != -1) {
                List<int> affairIDs = character.relationshipContainer
                    .GetAllRelatableIDWithRelationship(RELATIONSHIP_TYPE.AFFAIR);
                Character aliveAffair = null;
                for (int i = 0; i < affairIDs.Count; i++) {
                    int affairID = affairIDs[i];
                    aliveAffair = CharacterManager.Instance.GetCharacterByID(affairID);
                    if (aliveAffair != null && aliveAffair.isDead == false) { break; }
                }
                if (aliveAffair == null) {
                    if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW)) {
                        List<Character> choices = new List<Character>();
                        //choose from characters that owner has a relationship with, that is not their lover or affair and is still alive.
                        foreach (var relationship in character.relationshipContainer.relationships) {
                            if (relationship.Key != loverID && relationship.Value.HasRelationship(RELATIONSHIP_TYPE.AFFAIR) == false) {
                                Character otherCharacter = CharacterManager.Instance.GetCharacterByID(relationship.Key);
                                if (otherCharacter != null && otherCharacter.isDead == false) {
                                    SEXUALITY sexuality1 = character.sexuality;
                                    SEXUALITY sexuality2 = otherCharacter.sexuality;
                                    GENDER gender1 = character.gender;
                                    GENDER gender2 = otherCharacter.gender;
                                    if (RelationshipManager.IsSexuallyCompatible(sexuality1, sexuality2, gender1, gender2) 
                                        && RelationshipManager.Instance.GetValidator(character).
                                            CanHaveRelationship(character, otherCharacter, RELATIONSHIP_TYPE.AFFAIR)) {
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
                return "fail";
            }
        }
        #endregion

    }
}

