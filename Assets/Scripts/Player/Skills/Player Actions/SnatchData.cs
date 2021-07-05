using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Characters.Behaviour;
using Inner_Maps;
using UnityEngine;
using Inner_Maps.Location_Structures;
using Logs;
using UtilityScripts;

public class SnatchData : PlayerAction {
    public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.SNATCH;
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
                UIManager.Instance.ShowClickableObjectPicker(validStructures, o => OnPickTargetStructure(o, targetCharacter), validityChecker: IsStructureValid, 
                    onHoverAction: OnHoverStructure, onHoverExitAction: OnHoverExitStructure, title: "Select Structure", 
                    portraitGetter: null, showCover: true, layer: 25, shouldShowConfirmationWindowOnPick: true, asButton: true);
            } else {
                List<LocationStructure> validStructures = new List<LocationStructure>();
                //if (PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.DEFILER)) {
                //    validStructures.AddRange(PlayerManager.Instance.player.playerSettlement.GetStructuresOfType(STRUCTURE_TYPE.DEFILER));    
                //}
                if (PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.TORTURE_CHAMBERS)) {
                    validStructures.AddRange(PlayerManager.Instance.player.playerSettlement.GetStructuresOfType(STRUCTURE_TYPE.TORTURE_CHAMBERS));
                }
                
                UIManager.Instance.ShowClickableObjectPicker(validStructures, o => OnPickTargetStructure(o, targetCharacter), validityChecker: IsStructureValid, 
                    onHoverAction: OnHoverStructure, onHoverExitAction: OnHoverExitStructure, title: "Select Structure", 
                    portraitGetter: null, showCover: true, layer: 25, shouldShowConfirmationWindowOnPick: true, asButton: true);
            }
        }
    }
    private bool IsStructureValid(LocationStructure structure) {
        if (structure is Kennel kennel) {
            return !kennel.HasReachedKennelCapacity();
        } else if (structure is Defiler defiler) {
            return defiler.HasUnoccupiedRoom();
        } else if (structure is TortureChambers prison) {
            return prison.HasUnoccupiedRoom();
        }
        return false;
    }
    private void OnHoverStructure(LocationStructure structure) {
        if (!IsStructureValid(structure)) {
            UIManager.Instance.ShowSmallInfo($"{structure.name} is already at max capacity!", "Maximum Capacity Reached");
        }
    }
    private void OnHoverExitStructure(LocationStructure structure) {
        if (!IsStructureValid(structure)) {
            UIManager.Instance.HideSmallInfo();
        }
    }
    private void OnPickTargetStructure(object obj, Character targetCharacter) {
        if (obj is LocationStructure structure) {
            UIManager.Instance.HideObjectPicker();
            //get an available skeleton then add job to skeleton to drop character near chosen structure
            Character availableSkeletonOrCultist = GetNearestAvailableSkeletonOrCultist(targetCharacter);
            availableSkeletonOrCultist.jobQueue.CancelAllJobs();

            List<LocationGridTile> choices = ObjectPoolManager.Instance.CreateNewGridTileList();
            if (structure is Kennel) {
                Area area = structure.occupiedArea;
                for (int i = 0; i < area.gridTileComponent.gridTiles.Count; i++) {
                    LocationGridTile t = area.gridTileComponent.gridTiles[i];
                    if(t.structure is Wilderness && t.IsPassable() && !t.isOccupied) {
                        choices.Add(t);
                    }
                }
                //choices = hexTile.locationGridTiles.Where(t => t.structure is Wilderness && t.IsPassable() && !t.isOccupied).ToList();  
            } else {
                for (int i = 0; i < structure.passableTiles.Count; i++) {
                    LocationGridTile t = structure.passableTiles[i];
                    if (!t.structure.IsTilePartOfARoom(t, out var room)) {
                        choices.Add(t);
                    }
                }
                //choices = structure.passableTiles.Where(t => !t.structure.IsTilePartOfARoom(t, out var room)).ToList();
            }
            if (choices.Count > 0) {
                availableSkeletonOrCultist.jobComponent.CreateSnatchJob(targetCharacter, CollectionUtilities.GetRandomElement(choices), structure);
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "InterventionAbility", "Snatch", "instructed", null, LOG_TAG.Player);
                log.AddToFillers(availableSkeletonOrCultist, availableSkeletonOrCultist.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                log.AddToFillers(structure, structure.nameplateName, LOG_IDENTIFIER.LANDMARK_1);
                log.AddLogToDatabase();
                PlayerManager.Instance.player.ShowNotificationFromPlayer(log, true);
                Messenger.Broadcast(PlayerSkillSignals.FORCE_RELOAD_PLAYER_ACTIONS);
                base.ActivateAbility(targetCharacter); //this is so that mana/charges/cooldown can be activated after picking structure to bring to
            }
            ObjectPoolManager.Instance.ReturnGridTileListToPool(choices);
        }
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = CanPerformAbility(); //NOTE: Did not use base since this action can be used on blessed characters
        if (canPerform) {
            if (!CharacterManager.Instance.allCharacters.Any(CanDoSnatch)) {
                return false;
            }
            if(targetCharacter.traitContainer.HasTrait("Hibernating", "Sturdy", "Indestructible")) {
                return false;
            }
            if (targetCharacter is Summon) {
                return PlayerManager.Instance.player.playerSettlement.HasAvailableKennelForSnatch();
            } else {
                return PlayerManager.Instance.player.playerSettlement.HasAvailableDefilerForSnatch() ||
                       PlayerManager.Instance.player.playerSettlement.HasAvailablePrisonForSnatch();  
            }
        }
        return false;
    }
    public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) {
        string reasons = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
        if (!CharacterManager.Instance.allCharacters.Any(CanDoSnatch)) {
            reasons += "You have no available Snatchers \n(NOTE: Cultists in an active quest cannot be instructed),";
        }
        if (targetCharacter.traitContainer.HasTrait("Hibernating", "Sturdy", "Indestructible")) {
            reasons += "Cannot target characters that are Hibernating/Sturdy/Indestructible,";
        }
        if (targetCharacter is Summon) {
            if (!PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.KENNEL)) {
                reasons += "You have no Kennels,";    
            } else if (!PlayerManager.Instance.player.playerSettlement.HasAvailableKennelForSnatch()) {
                reasons += "You have no Kennels that can house more Monsters,";    
            }
        } else {
            if (/*!PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.DEFILER) &&*/
                !PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.TORTURE_CHAMBERS)) {
                reasons += "You have no Prison,";    
            } else if (!PlayerManager.Instance.player.playerSettlement.HasAvailableDefilerForSnatch() && 
                       !PlayerManager.Instance.player.playerSettlement.HasAvailablePrisonForSnatch()) {
                reasons += "You have no unoccupied Defiler Rooms or Prison Cells,";    
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
                if (targetCharacter.traitContainer.HasTrait("Cultist")) {
                    return false;
                }
                if (targetCharacter.isDead) {
                    return false;
                }
                if (targetCharacter.faction != null && targetCharacter.faction.isPlayerFaction) {
                    return false; //character is part of player faction.
                }
                if (!(targetCharacter is Summon)) {
                    //if character is not a monster, check if it is still considered a villager
                    //if it is not then do not allow snatch, since we do not allow non-villagers to be unseized on
                    //the defiler or prison anyway.
                    if (!targetCharacter.isNormalCharacter) {
                        return false;
                    }
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
        return nearest;
    }
    private static bool CanDoSnatch(Character character) {
        //Snatch is no longer exclusive to those characters that has SnatcherBehaviour
        if (character.isDead) {
            return false;
        }
        if (character.traitContainer.HasTrait("Cultist", "Snatcher")) {
            return !character.behaviourComponent.isCurrentlySnatching && character.limiterComponent.canPerform && !character.partyComponent.isActiveMember;
        } 
        return false;
    }
}