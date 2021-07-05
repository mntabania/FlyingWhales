using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Locations.Settlements;
using UtilityScripts;

public class InvadeBehaviour : CharacterBehaviourComponent {
    public InvadeBehaviour() {
        priority = 10;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
#if DEBUG_LOG
        log += $"\n{character.name} is an Invader";
#endif
        if (character.behaviourComponent.invadeVillageTarget.Count <= 0) {
#if DEBUG_LOG
            log += $"\n-No invade target yet, setting one...";
#endif
            character.behaviourComponent.ResetInvadeVillageTarget();
            PopulateVillageTargetsByPriority(character.behaviourComponent.invadeVillageTarget, character);
            //character.behaviourComponent.SetInvadeVillageTarget(PopulateVillageTargetsByPriority(character));
            if (character.behaviourComponent.invadeVillageTarget.Count <= 0) {
#if DEBUG_LOG
                log += $"\n-Still no invade target, roam around.";
#endif
                return character.jobComponent.TriggerRoamAroundTile(JOB_TYPE.ROAM_AROUND_TILE, out producedJob);
            }
            producedJob = null;
            return true;
        } else {
#if DEBUG_LOG
            log += $"\n-Already has village target";
#endif
            Area areaLocation = character.areaLocation;
            if (areaLocation != null && character.behaviourComponent.invadeVillageTarget.Contains(areaLocation)) {
#if DEBUG_LOG
                log += $"\n-Already att village target, will find character to attack";
#endif
                //character is already at target village
                List<Character> targets = ObjectPoolManager.Instance.CreateNewCharactersList();
                PopulateTargetChoicesFor(targets, character, character.behaviourComponent.invadeVillageTarget);
                if (targets.Count > 0) {
                    //Fight a random target
                    Character chosenTarget = CollectionUtilities.GetRandomElement(targets);
#if DEBUG_LOG
                    log += $"\n-Chosen target is {chosenTarget.name}";
#endif
                    character.combatComponent.Fight(chosenTarget, CombatManager.Hostility);
                } else {
#if DEBUG_LOG
                    log += $"\n-No more valid targets, clearing target village data...";
#endif
                    //No more valid targets exist, clearing village target. 
                    character.behaviourComponent.ResetInvadeVillageTarget();
                }
                ObjectPoolManager.Instance.ReturnCharactersListToPool(targets);
                producedJob = null;
                return true;
            } else {
#if DEBUG_LOG
                log += $"\n-character is not yet at village target, will go there now...";
#endif
                //character is not yet at target village
                Area targetArea =
                    CollectionUtilities.GetRandomElement(character.behaviourComponent.invadeVillageTarget);
                LocationGridTile targetTile = CollectionUtilities.GetRandomElement(targetArea.gridTileComponent.gridTiles);
                return character.jobComponent.CreateGoToSpecificTileJob(targetTile, out producedJob);
            }
        }
    }
    private void PopulateTargetChoicesFor(List<Character> p_targetChoices, Character source, List<Area> p_areas) {
        for (int i = 0; i < p_areas.Count; i++) {
            Area area = p_areas[i];
            area.locationCharacterTracker.PopulateCharacterListInsideHexForInvadeBehaviour(p_targetChoices, source);
        }
    }
    //private bool IsCharacterValidForInvade(Character character) {
    //    return character.isNormalCharacter && character.isDead == false && character.isAlliedWithPlayer == false && !character.traitContainer.HasTrait("Hibernating", "Indestructible")
    //        && !character.isInLimbo && !character.isBeingSeized && character.carryComponent.IsNotBeingCarried();
    //}
    private void PopulateVillageTargetsByPriority(List<Area> areas, Character owner) {
        //get settlements in region that have normal characters living there.
        List<BaseSettlement> settlementsInRegion = RuinarchListPool<BaseSettlement>.Claim();
        owner.currentRegion?.PopulateSettlementsInRegionForInvadeBehaviour(settlementsInRegion);
        if (settlementsInRegion.Count > 0) {
            //Do not attack villages who are neutral/friendly with player faction
            List<BaseSettlement> villageChoices = RuinarchListPool<BaseSettlement>.Claim();
            for (int i = 0; i < settlementsInRegion.Count; i++) {
                BaseSettlement s = settlementsInRegion[i];
                if (s.locationType == LOCATION_TYPE.VILLAGE && (s.owner == null || s.owner.IsHostileWith(owner.faction))) {
                    villageChoices.Add(s);
                }
            }
            if (villageChoices.Count > 0) {
                //a random village occupied by Villagers within current region
                BaseSettlement chosenVillage = CollectionUtilities.GetRandomElement(villageChoices);
                areas.AddRange(chosenVillage.areas);
            } else {
                //a random special structure occupied by Villagers within current region
                List<BaseSettlement> specialStructureChoices = RuinarchListPool<BaseSettlement>.Claim();
                for (int i = 0; i < settlementsInRegion.Count; i++) {
                    BaseSettlement s = settlementsInRegion[i];
                    if (s.locationType == LOCATION_TYPE.DUNGEON) {
                        specialStructureChoices.Add(s);
                    }
                }
                if (specialStructureChoices.Count > 0) {
                    BaseSettlement chosenSpecialStructure = CollectionUtilities.GetRandomElement(specialStructureChoices);
                    areas.AddRange(chosenSpecialStructure.areas);
                }
                RuinarchListPool<BaseSettlement>.Release(specialStructureChoices);
            }
            RuinarchListPool<BaseSettlement>.Release(villageChoices);
        }
        RuinarchListPool<BaseSettlement>.Release(settlementsInRegion);
        // //no settlements in region.
        // //a random area occupied by Villagers within current region
        // List<HexTile> occupiedAreas = owner.currentRegion?.GetAreasOccupiedByVillagers();
        // if (occupiedAreas != null) {
        //     HexTile randomArea = CollectionUtilities.GetRandomElement(occupiedAreas);
        //     return new List<HexTile>() { randomArea };
        // }
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
