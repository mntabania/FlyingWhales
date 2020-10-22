﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Characters.Behaviour;
using Inner_Maps;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Logs;
using UtilityScripts;

public class SnatchData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.SNATCH;
    public override string name => "Snatch";
    public override bool canBeCastOnBlessed => true;
    public override string description => $"This Action can be used to instruct an available Skeleton or Cultist to abduct a target Villager or Monster. If successful, it will then bring it to an appropriate demonic structure - Defiler or Prison for Villagers, Kennel for Monsters.";
    public SnatchData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }

    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        if (targetPOI is Character targetCharacter) {
            if (targetPOI is Summon) {
                List<LocationStructure> validStructures = PlayerManager.Instance.player.playerSettlement.GetStructuresOfType(STRUCTURE_TYPE.KENNEL);
                UIManager.Instance.ShowClickableObjectPicker(validStructures, o => OnPickTargetStructure(o, targetCharacter) , title: "Select Affliction", 
                    portraitGetter: null, showCover: true, layer: 25, shouldShowConfirmationWindowOnPick: true, asButton: true);
            } else {
                List<LocationStructure> validStructures = new List<LocationStructure>();
                if (PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.DEFILER)) {
                    validStructures.AddRange(PlayerManager.Instance.player.playerSettlement.GetStructuresOfType(STRUCTURE_TYPE.DEFILER));    
                }
                if (PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.TORTURE_CHAMBERS)) {
                    validStructures.AddRange(PlayerManager.Instance.player.playerSettlement.GetStructuresOfType(STRUCTURE_TYPE.TORTURE_CHAMBERS));
                }
                
                UIManager.Instance.ShowClickableObjectPicker(validStructures, o => OnPickTargetStructure(o, targetCharacter) , title: "Select Affliction", 
                    portraitGetter: null, showCover: true, layer: 25, shouldShowConfirmationWindowOnPick: true, asButton: true);
            }
        }
    }
    private void OnPickTargetStructure(object obj, Character targetCharacter) {
        if (obj is LocationStructure structure) {
            UIManager.Instance.HideObjectPicker();
            //get an available skeleton then add job to skeleton to drop character near chosen structure
            Character availableSkeletonOrCultist = GetNearestAvailableSkeletonOrCultist(targetCharacter);
            availableSkeletonOrCultist.jobQueue.CancelAllJobs();
            
            List<LocationGridTile> choices;
            if (structure is Kennel) {
                HexTile hexTile = structure.occupiedHexTile.hexTileOwner;
                choices = hexTile.locationGridTiles.Where(t => t.structure is Wilderness && t.IsPassable() && !t.isOccupied).ToList();  
            } else {
                choices = structure.passableTiles.Where(t => !t.structure.IsTilePartOfARoom(t, out var room)).ToList();    
            }
            if (choices.Count > 0) {
                availableSkeletonOrCultist.jobComponent.CreateSnatchJob(targetCharacter, CollectionUtilities.GetRandomElement(choices), structure);
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "InterventionAbility", "Snatch", "instructed", null, LOG_TAG.Player);
                log.AddToFillers(availableSkeletonOrCultist, availableSkeletonOrCultist.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                log.AddToFillers(structure, structure.nameplateName, LOG_IDENTIFIER.LANDMARK_1);
                log.AddLogToDatabase();
                PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
                Messenger.Broadcast(Signals.FORCE_RELOAD_PLAYER_ACTIONS);
                base.ActivateAbility(targetCharacter); //this is so that mana/charges/cooldown can be activated after picking structure to bring to
            }
        }
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = CanPerformAbility(); //NOTE: Did not use base since this action can be used on blessed characters
        if (canPerform) {

            if(PlayerSkillManager.Instance.selectedArchetype == PLAYER_ARCHETYPE.Lich) {
                if (!CharacterManager.Instance.allCharacters.Any(CanDoSnatch)) {
                    return false;
                }
            } else {
                if (!PlayerManager.Instance.player.playerFaction.characters.Any(CanDoSnatch)) {
                    return false;
                }
            }
            if (targetCharacter is Summon) {
                return PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.KENNEL);
            } else {
                return PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.DEFILER) ||
                       PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.TORTURE_CHAMBERS);  
            }
        }
        return false;
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (!PlayerManager.Instance.player.playerFaction.characters.Any(CanDoSnatch)) {
            reasons += $"You have no available Snatchers,";
        }
        if (targetCharacter is Summon) {
            if (!PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.KENNEL)) {
                reasons += $"You have no Kennel,";    
            }
        } else {
            if (!PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.DEFILER) &&
                !PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.TORTURE_CHAMBERS)) {
                reasons += $"You have no Defiler or Prison,";    
            }
        }
        return reasons;
    }
    public override bool IsValid(IPlayerActionTarget target) {
        bool isValid = base.IsValid(target);
        if (isValid) {
            if (target is Character targetCharacter) {
                if (targetCharacter.currentStructure is DemonicStructure) {
                    return false;
                }
                if (targetCharacter.isAlliedWithPlayer) {
                    return false;
                }
                if (targetCharacter.isDead) {
                    return false;
                }
            }
            return true;
        }
        return false;
    }
    #endregion

    private Character GetNearestAvailableSkeletonOrCultist(Character targetCharacter) {
        Character nearest = null;
        float nearestDist = 0f;
        if(PlayerSkillManager.Instance.selectedArchetype == PLAYER_ARCHETYPE.Lich) {
            for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
                Character snatcher = CharacterManager.Instance.allCharacters[i];
                if (CanDoSnatch(snatcher)) {
                    float dist = Vector2.Distance(snatcher.worldPosition, targetCharacter.worldPosition);
                    if (nearest == null || dist < nearestDist) {
                        nearest = snatcher;
                        nearestDist = dist;
                    }
                }
            }
        } else {
            for (int i = 0; i < PlayerManager.Instance.player.playerFaction.characters.Count; i++) {
                Character snatcher = PlayerManager.Instance.player.playerFaction.characters[i];
                if (CanDoSnatch(snatcher)) {
                    float dist = Vector2.Distance(snatcher.worldPosition, targetCharacter.worldPosition);
                    if (nearest == null || dist < nearestDist) {
                        nearest = snatcher;
                        nearestDist = dist;
                    }
                }
            }
        }
        return nearest;
    }
    private static bool CanDoSnatch(Character character) {
        //Snatch is no longer exclusive to those characters that has SnatcherBehaviour
        //Cultists can no snatch if archetype is Lich
        if (character.isDead) {
            return false;
        }
        if (character.traitContainer.HasTrait("Cultist", "Snatcher")) {
            return !character.behaviourComponent.isCurrentlySnatching && character.canPerform;
        } 
        //else if (character.behaviourComponent.HasBehaviour(typeof(SnatcherBehaviour))) {
        //    return !character.behaviourComponent.isCurrentlySnatching && character.canPerform;
        //}
        return false;
    }
}