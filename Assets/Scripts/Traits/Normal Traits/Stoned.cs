using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using Inner_Maps.Location_Structures;

namespace Traits {
    public class Stoned : Status {

        public override bool isSingleton => true;

        public Stoned() {
            name = "Stoned";
            description = "This has been turned into a stone.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            hindersMovement = true;
            hindersPerform = true;
            hindersWitness = true;
            hindersAttackTarget = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if(addedTo is Character character && character.marker) {
                character.marker.PauseAnimation();
                Log log = new Log(GameManager.Instance.Today(), "Trait", name, "effect");
                log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddLogToInvolvedObjects();
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character && character.marker) {
                character.marker.UnpauseAnimation();
            }
        }
        #endregion
    }
}
