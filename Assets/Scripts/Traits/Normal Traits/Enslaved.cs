using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Enslaved : Status {
        //Not singleton for responsible characters
        //public override bool isSingleton => true;

        public Enslaved() {
            name = "Enslaved";
            description = "Forced to gather food for its master.";
            thoughtText = "I miss freedom.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            moodEffect = -8;
            hindersSocials = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourcePOI) {
            base.OnAddTrait(sourcePOI);
            if (sourcePOI is Character targetCharacter) {
                if (targetCharacter.partyComponent.hasParty) {
                    targetCharacter.partyComponent.currentParty.RemoveMember(targetCharacter);
                }
                targetCharacter.traitContainer.RemoveRestrainAndImprison(targetCharacter);
                targetCharacter.behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Slave_Behaviour);
                if (targetCharacter.isNotSummonAndDemonAndZombie) {
                    targetCharacter.classComponent.AssignClass("Farmer");
                }
                if(responsibleCharacter != null) {
                    if (responsibleCharacter.faction != null) {
                        targetCharacter.ChangeFactionTo(responsibleCharacter.faction, true);
                    }
                    if (responsibleCharacter.homeStructure != null) {
                        targetCharacter.MigrateHomeStructureTo(responsibleCharacter.homeStructure);
                    } else if (responsibleCharacter.homeSettlement != null) {
                        targetCharacter.MigrateHomeTo(responsibleCharacter.homeSettlement);
                    }
                } else if (responsibleCharacters != null && responsibleCharacters.Count > 0) {
                    Character characterResponsible = responsibleCharacters[0];
                    if(characterResponsible.faction != null) {
                        targetCharacter.ChangeFactionTo(characterResponsible.faction, true);
                    }
                    if (characterResponsible.homeStructure != null) {
                        targetCharacter.MigrateHomeStructureTo(characterResponsible.homeStructure);
                    } else if (characterResponsible.homeSettlement != null) {
                        targetCharacter.MigrateHomeTo(characterResponsible.homeSettlement);
                    }
                }
                targetCharacter.jobComponent.AddAbleJob(JOB_TYPE.PRODUCE_FOOD);
                targetCharacter.jobComponent.AddAbleJob(JOB_TYPE.HAUL);
                Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, targetCharacter as IPlayerActionTarget);
            }
        }
        public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy) {
            base.OnRemoveTrait(sourcePOI, removedBy);
            if (sourcePOI is Character targetCharacter) {
                targetCharacter.ChangeToDefaultFaction();
                targetCharacter.MigrateHomeStructureTo(null);
                targetCharacter.behaviourComponent.UpdateDefaultBehaviourSet();
                targetCharacter.jobComponent.RemoveAbleJob(JOB_TYPE.PRODUCE_FOOD);
                targetCharacter.jobComponent.RemoveAbleJob(JOB_TYPE.HAUL);
                Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, targetCharacter as IPlayerActionTarget);
            }
        }
        #endregion
    }
}

