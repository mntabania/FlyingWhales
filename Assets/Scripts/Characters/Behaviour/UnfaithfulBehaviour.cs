using System;
using System.Collections.Generic;
using Traits;
using UtilityScripts;

public class UnfaithfulBehaviour  : CharacterBehaviourComponent {

    public UnfaithfulBehaviour() {
        priority = 20;
    }

    #region Overrides
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        int loverID = character.relationshipContainer.GetFirstRelatableIDWithRelationship(RELATIONSHIP_TYPE.LOVER);
        if (loverID != -1 && ChanceData.RollChance(CHANCE_TYPE.Unfaithful_Active_Search_Affair)) {
            //character has a lover
            AFFLICTION_SPECIFIC_BEHAVIOUR behaviourToUse = AFFLICTION_SPECIFIC_BEHAVIOUR.None;
            if (PlayerSkillManager.Instance.HasAfflictionAddedBehaviourForSkillAtCurrentLevel(PLAYER_SKILL_TYPE.UNFAITHFULNESS, AFFLICTION_SPECIFIC_BEHAVIOUR.Wild_Multiple_Affair)) {
                behaviourToUse = AFFLICTION_SPECIFIC_BEHAVIOUR.Wild_Multiple_Affair;
            } else if (PlayerSkillManager.Instance.HasAfflictionAddedBehaviourForSkillAtCurrentLevel(PLAYER_SKILL_TYPE.UNFAITHFULNESS, AFFLICTION_SPECIFIC_BEHAVIOUR.Multiple_Affair)) {
                behaviourToUse = AFFLICTION_SPECIFIC_BEHAVIOUR.Multiple_Affair;
            } else if (PlayerSkillManager.Instance.HasAfflictionAddedBehaviourForSkillAtCurrentLevel(PLAYER_SKILL_TYPE.UNFAITHFULNESS, AFFLICTION_SPECIFIC_BEHAVIOUR.Active_Search_Affair)) {
                behaviourToUse = AFFLICTION_SPECIFIC_BEHAVIOUR.Active_Search_Affair;   
            }
            List<Character> choices = GetAffairChoices(behaviourToUse, character, loverID);
            if (choices.Count > 0) {
                //If no affair yet, the character will create a Have Affair Job which will attempt to have an affair with a viable target.
                Character chosenCharacter = CollectionUtilities.GetRandomElement(choices);
                GoapPlanJob cheatJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.FIND_AFFAIR, INTERACTION_TYPE.HAVE_AFFAIR, chosenCharacter, character);
                producedJob = cheatJob;
                RuinarchListPool<Character>.Release(choices);
                return true;
            }
            RuinarchListPool<Character>.Release(choices);
        }
        producedJob = null;
        return false;
    }
    #endregion

    private List<Character> GetAffairChoices(AFFLICTION_SPECIFIC_BEHAVIOUR p_afflictionSpecificBehaviour, Character character, int loverID) {
        List<Character> choices = RuinarchListPool<Character>.Claim();
        Unfaithful unfaithful = character.traitContainer.GetTraitOrStatus<Unfaithful>("Unfaithful");
        switch (p_afflictionSpecificBehaviour) {
            case AFFLICTION_SPECIFIC_BEHAVIOUR.Active_Search_Affair:
            case AFFLICTION_SPECIFIC_BEHAVIOUR.Multiple_Affair:
                foreach (var relationship in character.relationshipContainer.relationships) {
                    if (relationship.Key != loverID && !relationship.Value.HasRelationship(RELATIONSHIP_TYPE.AFFAIR)) {
                        Character otherCharacter = CharacterManager.Instance.GetCharacterByID(relationship.Key);
                        if (otherCharacter != null && !otherCharacter.isDead) {
                            bool isCompatible = unfaithful.IsCompatibleBasedOnSexualityAndOpinions(character, otherCharacter);
                            if (isCompatible &&
                                !character.IsHostileWith(otherCharacter) &&
                                (otherCharacter.race.IsSapient() || otherCharacter.race == RACE.RATMAN) &&
                                RelationshipManager.Instance.GetValidator(character).CanHaveRelationship(otherCharacter, character, RELATIONSHIP_TYPE.AFFAIR) &&
                                RelationshipManager.Instance.GetValidator(character).CanHaveRelationship(character, otherCharacter, RELATIONSHIP_TYPE.AFFAIR)) {
                                choices.Add(otherCharacter);
                            }
                        }
                    }
                }
                break;
            case AFFLICTION_SPECIFIC_BEHAVIOUR.Wild_Multiple_Affair:
                foreach (var relationship in character.relationshipContainer.relationships) {
                    if (relationship.Key != loverID && !relationship.Value.HasRelationship(RELATIONSHIP_TYPE.AFFAIR)) {
                        Character otherCharacter = CharacterManager.Instance.GetCharacterByID(relationship.Key);
                        if (otherCharacter != null && !otherCharacter.isDead) {
                            bool isCompatible = unfaithful.IsCompatibleBasedOnSexualityAndOpinions(character, otherCharacter);
                            if (isCompatible &&
                                (!character.IsHostileWith(otherCharacter) || otherCharacter.combatComponent.combatMode == COMBAT_MODE.Passive) &&
                                RelationshipManager.Instance.GetValidator(character).CanHaveRelationship(otherCharacter, character, RELATIONSHIP_TYPE.AFFAIR) &&
                                RelationshipManager.Instance.GetValidator(character).CanHaveRelationship(character, otherCharacter, RELATIONSHIP_TYPE.AFFAIR)) {
                                choices.Add(otherCharacter);
                            }
                        }
                    }
                }
                if (character.currentSettlement != null) {
                    for (int i = 0; i < character.currentSettlement.areas.Count; i++) {
                        Area area = character.currentSettlement.areas[i];
                        for (int j = 0; j < area.locationCharacterTracker.charactersAtLocation.Count; j++) {
                            Character otherCharacter = area.locationCharacterTracker.charactersAtLocation[j];
                            if (choices.Contains(otherCharacter)) { continue; }
                            if (otherCharacter != character && otherCharacter.id != loverID && !character.relationshipContainer.HasRelationshipWith(otherCharacter, RELATIONSHIP_TYPE.AFFAIR)) {
                                if (!otherCharacter.isDead) {
                                    bool isCompatible = unfaithful.IsCompatibleBasedOnSexualityAndOpinions(character, otherCharacter);
                                    if (isCompatible &&
                                        (!character.IsHostileWith(otherCharacter) || otherCharacter.combatComponent.combatMode == COMBAT_MODE.Passive) &&
                                        RelationshipManager.Instance.GetValidator(character).CanHaveRelationship(otherCharacter, character, RELATIONSHIP_TYPE.AFFAIR) &&
                                        RelationshipManager.Instance.GetValidator(character).CanHaveRelationship(character, otherCharacter, RELATIONSHIP_TYPE.AFFAIR)) {
                                        choices.Add(otherCharacter);
                                    }
                                }
                            }
                        }
                    }
                }
                break;
        }
        return choices;
    }
}
