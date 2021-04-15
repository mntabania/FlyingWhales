﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Object_Pools;

public class DragonBehaviour : BaseMonsterBehaviour {
	public DragonBehaviour() {
		priority = 8;
		// attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
	}
	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-{character.name} is a dragon";
        if(character is Dragon dragon) {
            if (dragon.willLeaveWorld) {
                log += $"\n-Will leave world";
                if (dragon.gridTileLocation.IsAtEdgeOfWalkableMap()) {
                    Region currentRegion = dragon.currentRegion;
                    dragon.SetDestroyMarkerOnDeath(true);
                    dragon.SetShowNotificationOnDeath(false);
                    Log deathLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Summon", "Dragon", "left", providedTags: LOG_TAG.Life_Changes);
                    deathLog.AddToFillers(dragon, dragon.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    deathLog.AddLogToDatabase();
                    dragon.Death(_deathLog: deathLog);
                    LogPool.Release(deathLog);
                    if(UIManager.Instance.monsterInfoUI.isShowing && UIManager.Instance.monsterInfoUI.activeMonster == dragon) {
                        UIManager.Instance.monsterInfoUI.CloseMenu();
                    }
                    Messenger.Broadcast(MonsterSignals.DRAGON_LEFT_WORLD, character, currentRegion);
                } else {
                    dragon.jobComponent.CreateGoToJob(dragon.gridTileLocation.GetNearestEdgeTileFromThis(), out producedJob);
                }
                return true;
            } else {
                if (dragon.isAttackingPlayer) {
                    log += $"\n-Attacking player";
                    if(dragon.targetStructure == null || !(dragon.targetStructure is DemonicStructure) || dragon.targetStructure.hasBeenDestroyed) {
                        log += $"\n-No target player structure or current target structure is not a demonic structure or current target structure is destroyed, will set one";
                        dragon.SetPlayerTargetStructure();
                    }
                    if (dragon.targetStructure != null) {
                        log += $"\n-Has target player structure: " + dragon.targetStructure.name;
                        if (dragon.currentStructure == dragon.targetStructure) {
                            log += $"\n-Character is already in target structure";
                            LocationStructure targetStructure = dragon.currentStructure;
                            if (targetStructure.objectsThatContributeToDamage.Count > 0) {
                                log += "\n-Has tile object in vision";
                                log += "\n-Adding tile object as hostile";
                                TileObject chosenTileObject = null;
                                IDamageable nearestDamageableObject = targetStructure.GetNearestDamageableThatContributeToHP(dragon.gridTileLocation);
                                if (nearestDamageableObject != null && nearestDamageableObject is TileObject tileObject) {
                                    chosenTileObject = tileObject;
                                }
                                if (chosenTileObject != null) {
                                    dragon.combatComponent.Fight(chosenTileObject, CombatManager.Hostility);
                                    return true;
                                } else {
                                    log += "\n-No preplaced tile object in vision, set target structure to null";
                                    dragon.ResetTargetStructure();
                                    return true;
                                }
                            } else {
                                log += "\n-No tile objects that contribute to structure hp, set target structure to null";
                                dragon.ResetTargetStructure();
                                return true;
                            }
                        } else {
                            log += $"\n-Character is not in target structure, go to it";
                            LocationGridTile targetTile = UtilityScripts.CollectionUtilities.GetRandomElement(dragon.targetStructure.passableTiles);
                            character.jobComponent.CreateGoToJob(targetTile, out producedJob);
                            return true;
                        }
                    } else {
                        log += $"\n-Still no target structure, roam";
                        character.jobComponent.TriggerRoamAroundTile(out producedJob);
                        return true;
                    }
                } else {
                    log += $"\n-Attacking village";
                    if (dragon.targetStructure == null) {
                        log += $"\n-No target village, will set one";
                        dragon.SetVillageTargetStructure();
                    }
                    if(dragon.targetStructure != null) {
                        BaseSettlement targetSettlement = dragon.targetStructure.settlementLocation;
                        if(targetSettlement != null) {
                            log += $"\n-Has target village: " + targetSettlement.name;
                            if (character.gridTileLocation != null) {
                                if (character.gridTileLocation.IsPartOfSettlement(targetSettlement)) {
                                    log += $"\n-Character is already in target settlement";
                                    Character target = targetSettlement.GetRandomResidentThatMeetCriteria(resident => character != resident && !resident.isDead && !resident.isBeingSeized && resident.gridTileLocation != null && resident.gridTileLocation.IsPartOfSettlement(targetSettlement) && !resident.traitContainer.HasTrait("Hibernating", "Indestructible"));
                                    if (target != null) {
                                        log += $"\n-Chosen target is {target.name}";
                                        character.combatComponent.Fight(target, CombatManager.Hostility);
                                        return true;
                                    } else {
                                        log += $"\n-No target character, will attack area";
                                        LocationStructure randomStructure = targetSettlement.GetRandomStructure();
                                        LocationGridTile randomTile = randomStructure.GetRandomTile();
                                        character.combatComponent.Fight(randomTile.tileObjectComponent.genericTileObject, CombatManager.Hostility);
                                        return true;
                                    }
                                } else {
                                    log += $"\n-Character is not in target settlement, go to it";
                                    LocationStructure targetStructure = targetSettlement.GetRandomStructure();
                                    if (targetStructure != null) {
                                        LocationGridTile targetTile = UtilityScripts.CollectionUtilities.GetRandomElement(targetStructure.passableTiles);
                                        if (character.jobComponent.CreateGoToJob(targetTile, out producedJob)) {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                        log += $"\n-Cannot go to target settlement, roam";
                        character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                        return true;
                    } else {
                        log += $"\n-Still no target village, roam";
                        character.jobComponent.TriggerRoamAroundTile(out producedJob);
                        return true;
                    }
                }
            }
        }
		return false;
	}
}
