﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Suspicious : Trait {
        public override bool isSingleton => true;

        public Suspicious() {
            name = "Suspicious";
            description = "Thinks danger is lurking at every corner. Might destroy things the player touches.";
            type = TRAIT_TYPE.BUFF;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            AddTraitOverrideFunctionIdentifier(TraitManager.See_Poi_Trait);
        }

        #region Overrides
        public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            if (characterThatWillDoJob.limiterComponent.canPerform && characterThatWillDoJob.limiterComponent.canMove && !characterThatWillDoJob.isDead && !characterThatWillDoJob.isAlliedWithPlayer && targetPOI is TileObject objectToBeInspected
                && objectToBeInspected.tileObjectType != TILE_OBJECT_TYPE.STRUCTURE_TILE_OBJECT && objectToBeInspected.tileObjectType != TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT) {
                if (objectToBeInspected.lastManipulatedBy is Player) {
                    //Must not destroy even if suspicious if the tile object is edible and character is starving
                    if ((!objectToBeInspected.traitContainer.HasTrait("Edible") || !characterThatWillDoJob.needsComponent.isStarving) && !(objectToBeInspected is Heirloom)) {
                        if (targetPOI.IsOwnedBy(characterThatWillDoJob)) {
                            characterThatWillDoJob.jobComponent.CreateDropItemJob(JOB_TYPE.RETURN_STOLEN_THING, targetPOI as TileObject, characterThatWillDoJob.homeStructure);
                        } else {
                            characterThatWillDoJob.jobComponent.TriggerDestroy(objectToBeInspected);
                        }
                    }
                }
            }
            return base.OnSeePOI(targetPOI, characterThatWillDoJob);
        }
        #endregion
    }

}
