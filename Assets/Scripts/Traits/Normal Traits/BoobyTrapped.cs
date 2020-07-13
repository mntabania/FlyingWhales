using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

namespace Traits {
    public class BoobyTrapped : Status {
        private ELEMENTAL_TYPE _element;
        
        public List<Character> awareCharacters { get; } //characters that know about this trait
        private ITraitable traitable { get; set; } //poi that has the trait
        
        public BoobyTrapped() {
            name = "Booby Trapped";
            description = "This is booby trapped.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            awareCharacters = new List<Character>();
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(24);
            AddTraitOverrideFunctionIdentifier(TraitManager.Start_Perform_Trait);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            traitable = addedTo;
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            traitable = null;
        }
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
        public override string GetTestingData(ITraitable traitable = null) {
            string data = base.GetTestingData(traitable);
            data += $"\n\tAware Characters: ";
            for (int i = 0; i < awareCharacters.Count; i++) {
                Character character = awareCharacters[i];
                data += $"{character.name},";    
            }
            return data;
        }
        #endregion
        
        #region Aware Characters
        public void AddAwareCharacter(Character character) {
            if (awareCharacters.Contains(character) == false) {
                awareCharacters.Add(character);
                if (traitable is TileObject tileObject) {
                    //create remove poison job
                    character.jobComponent.TriggerRemoveStatusTarget(tileObject, "Poisoned");
                }
            }
        }
        public void RemoveAwareCharacter(Character character) {
            awareCharacters.Remove(character);
        }
        #endregion

        public void SetElementType(ELEMENTAL_TYPE element) {
            _element = element;
        }
    }
}
