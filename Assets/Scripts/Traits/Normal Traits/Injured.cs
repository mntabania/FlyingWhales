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
            description = "Sustained a physical trauma.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(24);
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.FIRST_AID_CHARACTER };
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
                character.AddTraitNeededToBeRemoved(this);

                if (gainedFromDoingType != INTERACTION_TYPE.ASSAULT) {
                    Log addLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "add_trait", null, LOG_TAG.Life_Changes);
                    addLog.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    addLog.AddToFillers(null, this.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    addLog.AddLogToDatabase(true);
                }
            }
        }
        public override void OnRemoveTrait(ITraitable traitable, Character removedBy) {
            if (traitable is Character character) {
                character.UpdateCanCombatState();
                character.movementComponent.AdjustSpeedModifier(0.15f);
                character.RemoveTraitNeededToBeRemoved(this);
                Log addLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "remove_trait", null, LOG_TAG.Life_Changes);
                addLog.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                addLog.AddToFillers(null, this.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                addLog.AddLogToDatabase(true);
            }
            base.OnRemoveTrait(traitable, removedBy);
        }
        #endregion
    }

}
