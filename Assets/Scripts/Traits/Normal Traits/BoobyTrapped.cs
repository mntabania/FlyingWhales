using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Traits;
using UnityEngine.Assertions;
namespace Traits {
    public class BoobyTrapped : Status {
        private ELEMENTAL_TYPE _element;
        
        public List<Character> awareCharacters { get; } //characters that know about this trait
        private ITraitable traitable { get; set; } //poi that has the trait

        #region getters
        public ELEMENTAL_TYPE element => _element;
        public override Type serializedData => typeof(SaveDataBoobyTrapped);
        #endregion

        public BoobyTrapped() {
            name = "Booby Trapped";
            description = "This object will explode with [Element] damage if someone interacts with it.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            awareCharacters = new List<Character>();
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(24);
            AddTraitOverrideFunctionIdentifier(TraitManager.Start_Perform_Trait);
        }

        #region Loading
        public override void LoadInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadInstancedTrait(saveDataTrait);
            SaveDataBoobyTrapped saveDataBoobyTrapped = saveDataTrait as SaveDataBoobyTrapped;
            Assert.IsNotNull(saveDataBoobyTrapped);
            _element = saveDataBoobyTrapped.elementalType;
        }
        public override void LoadSecondWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadSecondWaveInstancedTrait(saveDataTrait);
            //TODO: Load aware characters
            throw new NotImplementedException();
        }
        #endregion
        
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
                if(ActivateTrap(node.actor, node.target, ref willStillContinueAction)) {
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

        private bool ActivateTrap(Character actor, IPointOfInterest target, ref bool willStillContinueAction) {
            if (target.gridTileLocation != null) {
                Log log = new Log(GameManager.Instance.Today(), "Trait", this.name, "trap_activated");
                log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                log.AddLogToInvolvedObjects();
                DamageTargetByTrap(target);
                willStillContinueAction = false;
                return true;
            }
            return false;
        }
        public void DamageTargetByTrap(IPointOfInterest target) {
            List<LocationGridTile> tiles = target.gridTileLocation.GetTilesInRadius(1, includeCenterTile: true, includeTilesInDifferentStructure: true);
            for (int i = 0; i < tiles.Count; i++) {
                LocationGridTile currTile = tiles[i];
                List<IPointOfInterest> pois = currTile.GetPOIsOnTile();
                for (int j = 0; j < pois.Count; j++) {
                    IPointOfInterest currPOI = pois[j];
                    currPOI.AdjustHP(-800, element, true);
                }
            }
            target.traitContainer.RemoveTrait(target, this);
        }

        public void SetElementType(ELEMENTAL_TYPE element) {
            _element = element;
            description = $"This object will explode with {UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(element.ToString())} damage if someone interacts with it.";
        }
    }
}

#region Save Data
public class SaveDataBoobyTrapped : SaveDataTrait {
    public List<string> awareCharactersIDs;
    public ELEMENTAL_TYPE elementalType;
    public override void Save(Trait trait) {
        base.Save(trait);
        BoobyTrapped boobyTrapped = trait as BoobyTrapped;
        Assert.IsNotNull(boobyTrapped);
        awareCharactersIDs = SaveUtilities.ConvertSavableListToIDs(boobyTrapped.awareCharacters);
        elementalType = boobyTrapped.elementalType;
    }
}
#endregion
