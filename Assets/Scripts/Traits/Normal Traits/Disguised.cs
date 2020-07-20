using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

namespace Traits {
    public class Disguised : Status {
        public Character owner { get; private set; }
        public Character disguisedCharacter { get; private set; }

        public Disguised() {
            name = "Disguised";
            description = "This is disguised.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                owner = character;
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                character.visuals.UpdateAllVisuals(character);
                if (!character.isDead && character.marker) {
                    for (int i = 0; i < character.marker.inVisionCharacters.Count; i++) {
                        Character inVisionCharacter = character.marker.inVisionCharacters[i];
                        if(!inVisionCharacter.isDead && inVisionCharacter.marker) {
                            inVisionCharacter.marker.AddUnprocessedPOI(character);
                        }
                    }
                }
            }
        }
        #endregion

        #region General
        public void SetDisguisedCharacter(Character character) {
            disguisedCharacter = character;
            owner.visuals.UpdateAllVisuals(owner);
        }
        #endregion
    }
}
