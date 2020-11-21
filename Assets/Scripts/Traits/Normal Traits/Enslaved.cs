using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Enslaved : Status {
        public override bool isSingleton => true;

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
                targetCharacter.traitContainer.RemoveTrait(targetCharacter, "Restrained");
                targetCharacter.behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Slave_Behaviour);
                if (targetCharacter.isNotSummonAndDemon) {
                    targetCharacter.AssignClass("Peasant");
                }
                if(responsibleCharacter != null) {
                    if (responsibleCharacter.faction != null) {
                        targetCharacter.ChangeFactionTo(responsibleCharacter.faction);
                    }
                    if (responsibleCharacter.homeStructure != null) {
                        targetCharacter.MigrateHomeStructureTo(responsibleCharacter.homeStructure);
                    } else if (responsibleCharacter.homeSettlement != null) {
                        targetCharacter.MigrateHomeTo(responsibleCharacter.homeSettlement);
                    }
                } else if (responsibleCharacters != null && responsibleCharacters.Count > 0) {
                    Character characterResponsible = responsibleCharacters[0];
                    if(characterResponsible.faction != null) {
                        targetCharacter.ChangeFactionTo(characterResponsible.faction);
                    }
                    if (characterResponsible.faction != null) {
                        targetCharacter.ChangeFactionTo(characterResponsible.faction);
                    }
                    if (characterResponsible.homeStructure != null) {
                        targetCharacter.MigrateHomeStructureTo(characterResponsible.homeStructure);
                    } else if (characterResponsible.homeSettlement != null) {
                        targetCharacter.MigrateHomeTo(characterResponsible.homeSettlement);
                    }
                }
            }
        }
        public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy) {
            base.OnRemoveTrait(sourcePOI, removedBy);
            if (sourcePOI is Character targetCharacter) {
                if (targetCharacter.minion != null) {
                    targetCharacter.ChangeFactionTo(PlayerManager.Instance.player.playerFaction);
                } else if (targetCharacter is Summon) {
                    targetCharacter.ChangeFactionTo(FactionManager.Instance.neutralFaction);
                } else {
                    targetCharacter.ChangeFactionTo(FactionManager.Instance.vagrantFaction);
                }
                targetCharacter.MigrateHomeStructureTo(null);
                targetCharacter.behaviourComponent.UpdateDefaultBehaviourSet();
            }
        }
        #endregion
    }
}

