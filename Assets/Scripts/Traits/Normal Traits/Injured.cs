using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Injured : Status {
        //private GoapPlanJob _removeTraitJob;

        //#region getters/setters
        //public override bool isRemovedOnSwitchAlterEgo {
        //    get { return true; }
        //}
        //#endregion
        public override bool isSingleton => true;

        public Injured() {
            name = "Injured";
            description = "This character is badly hurt.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(24);
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.FIRST_AID_CHARACTER, };
            moodEffect = -4;
            isStacking = true;
            stackLimit = 5;
            stackModifier = 0.5f;
            //effects = new List<TraitEffect>();
        }

        #region Overrides
        public override void OnAddTrait(ITraitable traitable) {
            base.OnAddTrait(traitable);
            if (traitable is Character character) {
                character.UpdateCanCombatState();
                character.movementComponent.AdjustSpeedModifier(-0.15f);
                //_sourceCharacter.CreateRemoveTraitJob(name);
                character.AddTraitNeededToBeRemoved(this);
                //_sourceCharacter.needsComponent.AdjustStaminaDecreaseRate(5);

                if (gainedFromDoing == null) { //TODO: || gainedFromDoing.poiTarget != _sourceCharacter
                    character.RegisterLog("NonIntel", "add_trait", null, name.ToLower());
                } else {
                    if (gainedFromDoing.goapType == INTERACTION_TYPE.ASSAULT) {
                        Log addLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "add_trait", gainedFromDoing);
                        if(gainedFromDoing != null) {
                            addLog.SetLogType(LOG_TYPE.Action);
                        }
                        addLog.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        addLog.AddToFillers(this, this.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                        //TODO: gainedFromDoing.states["Target Injured"].AddArrangedLog("injured", addLog, () => PlayerManager.Instance.player.ShowNotificationFrom(addLog, _sourceCharacter, true));
                    }
                }
                //Messenger.Broadcast(Signals.TRANSFER_ENGAGE_TO_FLEE_LIST, _sourceCharacter);
            }
        }
        public override void OnRemoveTrait(ITraitable traitable, Character removedBy) {
            if (traitable is Character character) {
                character.UpdateCanCombatState();
                character.movementComponent.AdjustSpeedModifier(0.15f);
                character.RemoveTraitNeededToBeRemoved(this);
                //_sourceCharacter.needsComponent.AdjustStaminaDecreaseRate(-5);
                character.RegisterLog("NonIntel", "remove_trait", null, name.ToLower());
            }
            base.OnRemoveTrait(traitable, removedBy);
        }
        #endregion
    }

}
