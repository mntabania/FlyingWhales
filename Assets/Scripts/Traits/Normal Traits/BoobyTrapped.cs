using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Traits;
using UnityEngine.Assertions;
using UtilityScripts;

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
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.REMOVE_TRAP, };
            awareCharacters = new List<Character>();
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(24);
            AddTraitOverrideFunctionIdentifier(TraitManager.Start_Perform_Trait);
        }

        #region Loading
        public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait) {
            base.LoadFirstWaveInstancedTrait(saveDataTrait);
            SaveDataBoobyTrapped saveDataBoobyTrapped = saveDataTrait as SaveDataBoobyTrapped;
            Assert.IsNotNull(saveDataBoobyTrapped);
            _element = saveDataBoobyTrapped.elementalType;
        }
        public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait) {
            base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
            SaveDataBoobyTrapped saveDataBoobyTrapped = p_saveDataTrait as SaveDataBoobyTrapped;
            Assert.IsNotNull(saveDataBoobyTrapped);
            awareCharacters.AddRange(SaveUtilities.ConvertIDListToCharacters(saveDataBoobyTrapped.awareCharacterIDs));        
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            traitable = addTo;
        }
        public override void OnCopyStatus(Status statusToCopy, ITraitable from, ITraitable to) {
            base.OnCopyStatus(statusToCopy, from, to);
            if (statusToCopy is BoobyTrapped status) {
                _element = status.element;
                awareCharacters.AddRange(status.awareCharacters);
            }
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
                    if (responsibleCharacter != null) {
                        if (character.traitContainer.HasTrait("Cultist") && responsibleCharacter.traitContainer.HasTrait("Cultist") && !tileObject.IsOwnedBy(character)) {
                            //Do not remove booby trap if both the culprit and the witness are cultists and the poisoned object is not owned by the witness
                        } else {
                            //create remove booby trap job
                            character.jobComponent.TriggerRemoveStatusTarget(tileObject, "Booby Trapped");
                        }
                    }
                }
            }
        }
        public void RemoveAwareCharacter(Character character) {
            awareCharacters.Remove(character);
        }
        #endregion

        private bool ActivateTrap(Character actor, IPointOfInterest target, ref bool willStillContinueAction) {
            if (target.gridTileLocation != null) {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Trait", this.name, "trap_activated", null, LOG_TAG.Life_Changes);
                log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                log.AddLogToDatabase(true);
                DamageTargetByTrap(actor, target);
                willStillContinueAction = false;
                return true;
            }
            return false;
        }
        public void DamageTargetByTrap(Character actor, IPointOfInterest target) {
            List<LocationGridTile> tiles = RuinarchListPool<LocationGridTile>.Claim();
            target.gridTileLocation.PopulateTilesInRadius(tiles, 1, includeCenterTile: true, includeTilesInDifferentStructure: true);
            for (int i = 0; i < tiles.Count; i++) {
                LocationGridTile currTile = tiles[i];
                List<IPointOfInterest> pois = RuinarchListPool<IPointOfInterest>.Claim();
                currTile.PopulatePOIsOnTile(pois);
                for (int j = 0; j < pois.Count; j++) {
                    IPointOfInterest currPOI = pois[j];
                    currPOI.AdjustHP(-800, element, true);
                }
                RuinarchListPool<IPointOfInterest>.Release(pois);
            }
            target.traitContainer.RemoveTrait(target, this);
            actor.traitContainer.AddTrait(actor, "Unconscious");
            RuinarchListPool<LocationGridTile>.Release(tiles);
        }

        public void SetElementType(ELEMENTAL_TYPE element) {
            _element = element;
            description = $"This object will explode with {UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(element.ToString())} damage if someone interacts with it.";
        }
    }
}

#region Save Data
public class SaveDataBoobyTrapped : SaveDataTrait {
    public List<string> awareCharacterIDs;
    public ELEMENTAL_TYPE elementalType;
    public override void Save(Trait trait) {
        base.Save(trait);
        BoobyTrapped boobyTrapped = trait as BoobyTrapped;
        Assert.IsNotNull(boobyTrapped);
        awareCharacterIDs = SaveUtilities.ConvertSavableListToIDs(boobyTrapped.awareCharacters);
        elementalType = boobyTrapped.elementalType;
    }
}
#endregion
