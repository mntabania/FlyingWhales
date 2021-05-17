using System.Collections;
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
#if DEBUG_LOG
        log += $"\n-{character.name} is a dragon";
#endif
        if (character is Dragon dragon) {
            if (dragon.willLeaveWorld) {
#if DEBUG_LOG
                log += $"\n-Will leave world";
#endif
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
                    dragon.jobComponent.CreateGoToSpecificTileJob(dragon.gridTileLocation.GetNearestEdgeTileFromThis(), out producedJob);
                }
                return true;
            } else {
                if (dragon.isAttackingPlayer) {
#if DEBUG_LOG
                    log += $"\n-Attacking player";
#endif
                    if (dragon.targetStructure == null || !(dragon.targetStructure is DemonicStructure) || dragon.targetStructure.hasBeenDestroyed) {
#if DEBUG_LOG
                        log += $"\n-No target player structure or current target structure is not a demonic structure or current target structure is destroyed, will set one";
#endif
                        dragon.SetPlayerTargetStructure();
                    }
                    if (dragon.targetStructure != null) {
#if DEBUG_LOG
                        log += $"\n-Has target player structure: " + dragon.targetStructure.name;
#endif
                        if (dragon.currentStructure == dragon.targetStructure) {
#if DEBUG_LOG
                            log += $"\n-Character is already in target structure";
#endif
                            LocationStructure targetStructure = dragon.currentStructure;
                            if (targetStructure.objectsThatContributeToDamage.Count > 0) {
#if DEBUG_LOG
                                log += "\n-Has tile object in vision";
                                log += "\n-Adding tile object as hostile";
#endif
                                TileObject chosenTileObject = null;
                                IDamageable nearestDamageableObject = targetStructure.GetNearestDamageableThatContributeToHP(dragon.gridTileLocation);
                                if (nearestDamageableObject != null && nearestDamageableObject is TileObject tileObject) {
                                    chosenTileObject = tileObject;
                                }
                                if (chosenTileObject != null) {
                                    dragon.combatComponent.Fight(chosenTileObject, CombatManager.Hostility);
                                    return true;
                                } else {
#if DEBUG_LOG
                                    log += "\n-No preplaced tile object in vision, set target structure to null";
#endif
                                    dragon.ResetTargetStructure();
                                    return true;
                                }
                            } else {
#if DEBUG_LOG
                                log += "\n-No tile objects that contribute to structure hp, set target structure to null";
#endif
                                dragon.ResetTargetStructure();
                                return true;
                            }
                        } else {
#if DEBUG_LOG
                            log += $"\n-Character is not in target structure, go to it";
#endif
                            LocationGridTile targetTile = UtilityScripts.CollectionUtilities.GetRandomElement(dragon.targetStructure.passableTiles);
                            character.jobComponent.CreateGoToJob(targetTile, out producedJob);
                            return true;
                        }
                    } else {
#if DEBUG_LOG
                        log += $"\n-Still no target structure, roam";
#endif
                        character.jobComponent.TriggerRoamAroundTile(out producedJob);
                        return true;
                    }
                } else {
#if DEBUG_LOG
                    log += $"\n-Attacking village";
#endif
                    if (dragon.targetStructure == null) {
#if DEBUG_LOG
                        log += $"\n-No target village, will set one";
#endif
                        dragon.SetVillageTargetStructure();
                    }
                    if(dragon.targetStructure != null) {
                        BaseSettlement targetSettlement = dragon.targetStructure.settlementLocation;
                        if(targetSettlement != null) {
#if DEBUG_LOG
                            log += $"\n-Has target village: " + targetSettlement.name;
#endif
                            if (character.gridTileLocation != null) {
                                if (character.gridTileLocation.IsPartOfSettlement(targetSettlement)) {
#if DEBUG_LOG
                                    log += $"\n-Character is already in target settlement";
#endif
                                    Character target = targetSettlement.GetRandomResidentForInvasionTargetThatIsInsideSettlement(targetSettlement, character);
                                    if (target != null) {
#if DEBUG_LOG
                                        log += $"\n-Chosen target is {target.name}";
#endif
                                        character.combatComponent.Fight(target, CombatManager.Hostility);
                                        return true;
                                    } else {
#if DEBUG_LOG
                                        log += $"\n-No target character, will attack area";
#endif
                                        LocationStructure randomStructure = targetSettlement.GetRandomStructure();
                                        LocationGridTile randomTile = randomStructure.GetRandomTile();
                                        character.combatComponent.Fight(randomTile.tileObjectComponent.genericTileObject, CombatManager.Hostility);
                                        return true;
                                    }
                                } else {
#if DEBUG_LOG
                                    log += $"\n-Character is not in target settlement, go to it";
#endif
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
#if DEBUG_LOG
                        log += $"\n-Cannot go to target settlement, roam";
#endif
                        character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                        return true;
                    } else {
#if DEBUG_LOG
                        log += $"\n-Still no target village, roam";
#endif
                        character.jobComponent.TriggerRoamAroundTile(out producedJob);
                        return true;
                    }
                }
            }
        }
		return false;
	}
}
