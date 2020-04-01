using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

namespace Traits {
    public class BoobyTrapped : Status {
        private ELEMENTAL_TYPE _element;

        public BoobyTrapped() {
            name = "Booby Trapped";
            description = "This is booby trapped.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(24);
            AddTraitOverrideFunctionIdentifier(TraitManager.Start_Perform_Trait);
        }

        #region Overrides
        //public override void OnAddTrait(ITraitable addedTo) {
        //    base.OnAddTrait(addedTo);
        //    if (addedTo is IPointOfInterest poi) {
        //        _freezingGO = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Freezing);
        //    }
        //    if (addedTo is Character) {
        //        Character character = addedTo as Character;
        //        character.needsComponent.AdjustComfortDecreaseRate(1f);
        //        character.needsComponent.AdjustTirednessDecreaseRate(1f);
        //        character.AdjustSpeedModifier(-0.15f);
        //    }
        //}
        //public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
        //    base.OnRemoveTrait(removedFrom, removedBy);
        //    if (_freezingGO) {
        //        ObjectPoolManager.Instance.DestroyObject(_freezingGO);
        //        _freezingGO = null;
        //    }
        //    if (removedFrom is Character) {
        //        Character character = removedFrom as Character;
        //        character.needsComponent.AdjustComfortDecreaseRate(-1f);
        //        character.needsComponent.AdjustTirednessDecreaseRate(-1f);
        //        character.AdjustSpeedModifier(0.15f);
        //    }
        //}
        public bool OnPerformGoapAction(ActualGoapNode node, ref bool willStillContinueAction) {
            if (node.action.actionCategory == ACTION_CATEGORY.DIRECT || node.action.actionCategory == ACTION_CATEGORY.CONSUME) {
                if (node.poiTarget.gridTileLocation != null) {
                    Log log = new Log(GameManager.Instance.Today(), "Trait", this.name, "trap_activated");
                    log.AddToFillers(node.actor, node.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(node.poiTarget, node.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    log.AddLogToInvolvedObjects();
                    List<LocationGridTile> tiles = node.poiTarget.gridTileLocation.GetTilesInRadius(1, includeCenterTile: true, includeTilesInDifferentStructure: true);
                    for (int i = 0; i < tiles.Count; i++) {
                        LocationGridTile currTile = tiles[i];
                        List<IPointOfInterest> pois = currTile.GetPOIsOnTile();
                        for (int j = 0; j < pois.Count; j++) {
                            IPointOfInterest currPOI = pois[j];
                            currPOI.AdjustHP(-500, _element, true);
                        }
                    }
                    willStillContinueAction = false;
                    node.poiTarget.traitContainer.RemoveTrait(node.poiTarget, this);
                    return true;
                }
            }
            return false;
        }
        #endregion

        public void SetElementType(ELEMENTAL_TYPE element) {
            _element = element;
        }
    }
}
