using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Characters.Behaviour;
using Inner_Maps;
using UnityEngine;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class SnatchData : PlayerAction {
    public override SPELL_TYPE type => SPELL_TYPE.SNATCH;
    public override string name => "Snatch";
    public override string description => $"This Action can be used to instruct an available Skeleton to abduct a target Villager or Monster. If successful, it will then bring it to an appropriate demonic structure - Defiler or Prison for Villagers, Kennel for Monsters.";
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
            } else if (targetCharacter.isNormalCharacter) {
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
            Character availableSkeleton = GetNearestAvailableSkeleton(targetCharacter);
            availableSkeleton.jobQueue.CancelAllJobs();
            
            List<LocationGridTile> choices;
            if (structure is Kennel) {
                HexTile hexTile = structure.occupiedHexTile.hexTileOwner;
                choices = hexTile.locationGridTiles.Where(t => t.structure is Wilderness && t.IsPassable() && !t.isOccupied).ToList();  
            } else {
                choices = structure.passableTiles.Where(t => !t.structure.IsTilePartOfARoom(t, out var room)).ToList();    
            }
            if (choices.Count > 0) {
                availableSkeleton.jobComponent.CreateSnatchJob(targetCharacter, CollectionUtilities.GetRandomElement(choices), structure);
                Log log = new Log(GameManager.Instance.Today(), "InterventionAbility", "Snatch", "instructed");
                log.AddToFillers(availableSkeleton, availableSkeleton.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                log.AddToFillers(structure, structure.nameplateName, LOG_IDENTIFIER.LANDMARK_1);
                PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
                Messenger.Broadcast(Signals.FORCE_RELOAD_PLAYER_ACTIONS);
            }
        }
    }
    public override bool CanPerformAbilityTowards(Character targetCharacter) {
        bool canPerform = CanPerformAbility(); //NOTE: Did not use base since this action can be used on blessed characters
        if (canPerform) {
            if (!PlayerManager.Instance.player.playerFaction.characters.Any(CanDoSnatch)) {
                return false;
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

    private Character GetNearestAvailableSkeleton(Character targetCharacter) {
        Character nearest = null;
        float nearestDist = 9999f;
        for (int i = 0; i < PlayerManager.Instance.player.playerFaction.characters.Count; i++) {
            Character snatcher = PlayerManager.Instance.player.playerFaction.characters[i];
            if (CanDoSnatch(snatcher)) {
                float dist = Vector2.Distance(snatcher.worldPosition, targetCharacter.worldPosition);
                if (dist < nearestDist) {
                    nearest = snatcher;
                    nearestDist = dist;
                }
            }
        }
        return nearest;
    }
    private static bool CanDoSnatch(Character character) {
        return character.behaviourComponent.HasBehaviour(typeof(SnatcherBehaviour)) && !character.behaviourComponent.isCurrentlySnatching && character.canPerform;
    }
}