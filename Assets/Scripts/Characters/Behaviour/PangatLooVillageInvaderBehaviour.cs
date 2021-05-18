using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UtilityScripts;

public class PangatLooVillageInvaderBehaviour : CharacterBehaviourComponent {
    
    public PangatLooVillageInvaderBehaviour() {
        priority = 450;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
#if DEBUG_LOG
        log += $"\n{character.name} is a Pangat Loo Invader";
#endif
        NPCSettlement targetSettlement = GetMainVillageSettlement(); //this is guaranteed to be the main village in Pangat Loo map
        if (targetSettlement != null) {
            if (character.currentSettlement == targetSettlement) {
#if DEBUG_LOG
                log += $"\n-Already at village target, will find character to attack";
#endif
                //character is already at target village
                List<Character> targets = ObjectPoolManager.Instance.CreateNewCharactersList();
                PopulateTargetChoices(targets, targetSettlement.areas);
                if (targets.Count > 0) {
                    //Fight a random target
                    Character chosenTarget = CollectionUtilities.GetRandomElement(targets);
#if DEBUG_LOG
                    log += $"\n-Chosen target is {chosenTarget.name}";
#endif
                    character.combatComponent.Fight(chosenTarget, CombatManager.Hostility);
                    ObjectPoolManager.Instance.ReturnCharactersListToPool(targets);
                    producedJob = null;
                    return true;
                } else {
#if DEBUG_LOG
                    log += $"\n-No more valid targets, roam";
#endif
                    ObjectPoolManager.Instance.ReturnCharactersListToPool(targets);
                    return character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                }
            } else {
#if DEBUG_LOG
                log += $"\n-character is not yet at village target, will go there now...";
#endif
                //character is not yet at target village
                Area targetArea = CollectionUtilities.GetRandomElement(targetSettlement.areas);
                LocationGridTile targetTile = CollectionUtilities.GetRandomElement(targetArea.gridTileComponent.gridTiles);
                return character.jobComponent.CreateGoToJob(targetTile, out producedJob);
            }    
        } else {
#if DEBUG_LOG
            log += $"\n-character does not have an invade village target, roam";
#endif
            //character could not find a valid target settlement
            return character.jobComponent.TriggerRoamAroundStructure(out producedJob);
        }
    }
    private void PopulateTargetChoices(List<Character> p_targetChoices, List<Area> o_area) {
        for (int i = 0; i < o_area.Count; i++) {
            Area area = o_area[i];
            area.locationCharacterTracker.PopulateCharacterListInsideHexForPangatLooTargetForInvasion(p_targetChoices); //Removed checking for allied with player because undead should attack all villagers in pangat loo
        }
    }
    public NPCSettlement GetMainVillageSettlement() {
        for (int i = 0; i < LandmarkManager.Instance.allNonPlayerSettlements.Count; i++) {
            NPCSettlement settlement = LandmarkManager.Instance.allNonPlayerSettlements[i];
            if(settlement.locationType == LOCATION_TYPE.VILLAGE) {
                return settlement;
            }
        }
        return null;
    }
    // public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
    //     producedJob = null;
    //     log += $"\n-{character.name} will invade";
    //     if (character.gridTileLocation.collectionOwner.isPartOfParentRegionMap
    //         && character.gridTileLocation.hexTileOwner 
    //         && character.gridTileLocation.hexTileOwner.settlementOnTile == character.behaviourComponent.assignedTargetSettlement) {
    //         log += "\n-Already in the target npcSettlement, will try to combat residents";
    //         //It will only go here if the invader is not combat anymore, meaning there are no more hostiles in his vision, so we must make sure that he attacks a resident in the settlement even though he can't see it
    //         character.behaviourComponent.invadeCombatantTargetList.Clear();
    //         character.behaviourComponent.invadeNonCombatantTargetList.Clear();
    //         BaseSettlement settlement = character.behaviourComponent.assignedTargetSettlement;
    //         for (int i = 0; i < settlement.residents.Count; i++) {
    //             Character resident = settlement.residents[i];
    //             if (!resident.isDead && resident.gridTileLocation != null && resident.gridTileLocation.IsPartOfSettlement(settlement) 
    //                 && resident.isAlliedWithPlayer == false) {
    //                 if (resident.traitContainer.HasTrait("Combatant")) {
    //                     character.behaviourComponent.invadeCombatantTargetList.Add(resident);
    //                 } else {
    //                     character.behaviourComponent.invadeNonCombatantTargetList.Add(resident);
    //                 }
    //             }
    //         }
    //         if(character.behaviourComponent.invadeCombatantTargetList.Count > 0) {
    //             Character chosenTarget = character.behaviourComponent.invadeCombatantTargetList[UnityEngine.Random.Range(0, character.behaviourComponent.invadeCombatantTargetList.Count)];
    //             log += "\n-Will attack combatant resident: " + chosenTarget.name;
    //             character.combatComponent.Fight(chosenTarget, CombatManager.Hostility);
    //         } else if (character.behaviourComponent.invadeNonCombatantTargetList.Count > 0) {
    //             Character chosenTarget = character.behaviourComponent.invadeNonCombatantTargetList[UnityEngine.Random.Range(0, character.behaviourComponent.invadeNonCombatantTargetList.Count)];
    //             log += "\n-Will attack non-combatant resident: " + chosenTarget.name;
    //             character.combatComponent.Fight(chosenTarget, CombatManager.Hostility);
    //             //character.Death();
    //         } else {
    //             log += "\n-No resident found in settlement, dissipate";
    //             //character.jobComponent.TriggerRoamAroundTile();
    //             character.Death();
    //         }
    //     } else {
    //         log += "\n-Is not in the target npcSettlement";
    //         log += "\n-Roam there";
    //         HexTile targetHex = character.behaviourComponent.assignedTargetSettlement.tiles[UnityEngine.Random.Range(0, character.behaviourComponent.assignedTargetSettlement.tiles.Count)];
    //         LocationGridTile targetTile = targetHex.locationGridTiles[UnityEngine.Random.Range(0, targetHex.locationGridTiles.Count)];
    //         character.jobComponent.TriggerRoamAroundTile(out producedJob, targetTile);
    //     }
    //     return true;
    // }
}
