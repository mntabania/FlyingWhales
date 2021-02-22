﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Lustful : Trait {
        public override bool isSingleton => true;

        public Lustful() {
            name = "Lustful";
            description = "Enjoys frequent lovemaking.";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            mutuallyExclusive = new string[] { "Chaste" };
        }

        #region Overrides
        public override void ExecuteCostModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, OtherData[] otherData, ref int cost) {
            base.ExecuteCostModification(action, actor, poiTarget, otherData, ref cost);
            if (action == INTERACTION_TYPE.MAKE_LOVE) {
                TIME_IN_WORDS currentTime = GameManager.Instance.GetCurrentTimeInWordsOfTick(actor);
                if (currentTime == TIME_IN_WORDS.EARLY_NIGHT || currentTime == TIME_IN_WORDS.LATE_NIGHT) {
                    if (poiTarget is Character) {
                        Character targetCharacter = poiTarget as Character;
                        Unfaithful unfaithful = actor.traitContainer.GetTraitOrStatus<Unfaithful>("Unfaithful");
                        if (unfaithful != null && actor.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.AFFAIR)) {
                            cost = UtilityScripts.Utilities.Rng.Next(15, 37);
                        }
                    }
                    //Lustful(Early Night or Late Night 5 - 25)
                    cost = UtilityScripts.Utilities.Rng.Next(5, 26);
                }
                cost = UtilityScripts.Utilities.Rng.Next(15, 26);
            }
        }
        #endregion

    }

}
