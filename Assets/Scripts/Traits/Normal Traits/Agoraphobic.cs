﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Agoraphobic : Trait {

        public Agoraphobic() {
            name = "Agoraphobic";
            description = "Agoraphobics avoid crowds.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            
            
            daysDuration = 0;
            canBeTriggered = true;
            //effects = new List<TraitEffect>();
        }

        #region Overrides
        //protected override void OnChangeLevel() {
        //    base.OnChangeLevel();
        //if(level == 1) {
        //    daysDuration = 50;
        //} else if (level == 2) {
        //    daysDuration = 70;
        //} else if (level == 3) {
        //    daysDuration = 90;
        //}
        //}
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character) {
                Character character = addedTo as Character;
                if (character.marker.inVisionCharacters.Count >= 3) {
                    ApplyAgoraphobicEffect(character, true);
                }
            }
        }
        public override void OnSeePOI(IPointOfInterest targetPOI, Character character) {
            base.OnSeePOI(targetPOI, character);
            if (targetPOI.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                //Character targetCharacter = targetPOI as Character;
                if (character.traitContainer.GetNormalTrait("Berserked") != null) {
                    return;
                }
                if (character.stateComponent.currentState == null || character.stateComponent.currentState.characterState != CHARACTER_STATE.COMBAT) {
                    if (character.marker.inVisionCharacters.Count >= 3) {
                        ApplyAgoraphobicEffect(character, true);
                    }
                } else if (character.stateComponent.currentState != null && character.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT) {
                    CombatState combatState = character.stateComponent.currentState as CombatState;
                    if (combatState.isAttacking) {
                        if (character.marker.inVisionCharacters.Count >= 3) {
                            ApplyAgoraphobicEffect(character, false);
                            Messenger.Broadcast(Signals.TRANSFER_ENGAGE_TO_FLEE_LIST, character, "agoraphobia");
                        }
                    }
                }
            }
        }
        public override string TriggerFlaw(Character character) {
            //If outside and the character lives in a house, the character will flee and go back home.
            string successLogKey = base.TriggerFlaw(character);
            if (character.homeStructure != null) {
                if (character.currentStructure != character.homeStructure) {
                    if (character.currentActionNode.action != null) {
                        character.StopCurrentActionNode(false);
                    }
                    if (character.stateComponent.currentState != null) {
                        character.stateComponent.currentState.OnExitThisState();
                    } 
                    //else if (character.stateComponent.stateToDo != null) {
                    //    character.stateComponent.SetStateToDo(null, false, false);
                    //}

                    LocationGridTile tile = character.homeStructure.tiles[Random.Range(0, character.homeStructure.tiles.Count)];
                    character.marker.GoTo(tile);
                    return successLogKey;
                } else {
                    return "fail_at_home";
                }
            } else {
                return "fail_no_home";
            }
        }
        #endregion

        private void ApplyAgoraphobicEffect(Character character, bool processCombat) {
            character.marker.AddAvoidsInRange(character.marker.inVisionCharacters, processCombat, "agoraphobia");
            character.AdjustHappiness(-50);
            character.AdjustTiredness(-150);
        }
    }
}

