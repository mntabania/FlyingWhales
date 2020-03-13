using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Lustful : Trait {

        public Lustful() {
            name = "Lustful";
            description = "Lustful characters enjoy frequent lovemaking.";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            mutuallyExclusive = new string[] { "Chaste" };
        }

        #region Overrides
        public override void ExecuteCostModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, object[] otherData, ref int cost) {
            base.ExecuteCostModification(action, actor, poiTarget, otherData, ref cost);
            if (action == INTERACTION_TYPE.MAKE_LOVE) {
                TIME_IN_WORDS currentTime = GameManager.GetCurrentTimeInWordsOfTick(actor);
                if (currentTime == TIME_IN_WORDS.EARLY_NIGHT || currentTime == TIME_IN_WORDS.LATE_NIGHT) {
                    if (poiTarget is Character) {
                        //If unfaithful and target is Paramour (15 - 36)/(8 - 20)/(5-15) per level, affects Early Night and Late Night only).
                        Character targetCharacter = poiTarget as Character;
                        Unfaithful unfaithful = actor.traitContainer.GetNormalTrait<Unfaithful>("Unfaithful");
                        if (unfaithful != null && actor.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.AFFAIR)) {
                            if (unfaithful.level == 1) {
                                cost = UtilityScripts.Utilities.Rng.Next(15, 37);
                            } else if (unfaithful.level == 2) {
                                cost = UtilityScripts.Utilities.Rng.Next(8, 21);
                            } else if (unfaithful.level == 3) {
                                cost = UtilityScripts.Utilities.Rng.Next(5, 16);
                            }
                        }
                    }
                    //Lustful(Early Night or Late Night 5 - 25)
                    cost = UtilityScripts.Utilities.Rng.Next(5, 26);
                }
                cost = UtilityScripts.Utilities.Rng.Next(15, 26);
            }
        }
        //public override void ExecuteActionPerTickEffects(INTERACTION_TYPE action, ActualGoapNode goapNode) {
        //    base.ExecuteActionPerTickEffects(action, goapNode);
        //    if (action == INTERACTION_TYPE.MAKE_LOVE) {
        //        goapNode.actor.needsComponent.AdjustHappiness(100);
        //    }
        //}
        #endregion

    }

}
