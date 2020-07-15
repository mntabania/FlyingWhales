using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemBehaviour : CharacterBehaviourComponent {
	public GolemBehaviour() {
		priority = 8;
		// attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
	}
	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
		if (character is Summon summon) {
			log += $"\n-{summon.name} is a golem";
			if (summon.gridTileLocation != null) {
                if((summon.homeStructure == null || summon.homeStructure.hasBeenDestroyed) && !summon.HasTerritory()) {
                    log += "\n-No home structure and territory";
                    log += "\n-Trigger Set Home interrupt";
                    summon.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, null);
                    if (summon.homeStructure == null && !summon.HasTerritory()) {
                        log += "\n-Still no home structure and territory";
                        log += "\n-50% chance to Roam Around Tile";
                        int roll = UnityEngine.Random.Range(0, 100);
                        log += "\n-Roll: " + roll;
                        if (roll < 50) {
                            summon.jobComponent.TriggerRoamAroundTile(out producedJob);
                        } else {
                            log += "\n-Otherwise, Visit Different Region";
                            if (!summon.jobComponent.TriggerVisitDifferentRegion()) {
                                log += "\n-Cannot perform Visit Different Region, Roam Around Tile";
                                summon.jobComponent.TriggerRoamAroundTile(out producedJob);
                            }
                        }
                        return true;
                    }
                    return true;
                } else {
                    if (summon.isAtHomeStructure || summon.IsInTerritory()) {
                        bool hasAddedJob = false;
                        log += "\n-Inside territory or home structure";
                        int fiftyPercentOfMaxHP = Mathf.RoundToInt(summon.maxHP * 0.5f);
                        if (summon.currentHP < fiftyPercentOfMaxHP) {
                            log += "\n-Less than 50% of Max HP, Sleep";
                            hasAddedJob = summon.jobComponent.TriggerMonsterSleep(out producedJob);
                        } else {
                            log += "\n-35% chance to Roam Around Territory";
                            int roll = UnityEngine.Random.Range(0, 100);
                            log += $"\n-Roll: {roll.ToString()}";
                            if (roll < 35) {
                                hasAddedJob = summon.jobComponent.TriggerRoamAroundTerritory(out producedJob);
                            } else {
                                TIME_IN_WORDS currTime = GameManager.GetCurrentTimeInWordsOfTick();
                                if (currTime == TIME_IN_WORDS.LATE_NIGHT || currTime == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                                    log += "\n-Late Night or After Midnight, 40% chance to Sleep";
                                    int sleepRoll = UnityEngine.Random.Range(0, 100);
                                    log += $"\n-Roll: {sleepRoll.ToString()}";
                                    if (roll < 40) {
                                        hasAddedJob = summon.jobComponent.TriggerMonsterSleep(out producedJob);
                                    }
                                } else {
                                    log += "\n-5% chance to Sleep";
                                    int sleepRoll = UnityEngine.Random.Range(0, 100);
                                    log += $"\n-Roll: {sleepRoll.ToString()}";
                                    if (sleepRoll < 5) {
                                        hasAddedJob = summon.jobComponent.TriggerMonsterSleep(out producedJob);
                                    }
                                }
                            }
                        }
                        if (!hasAddedJob) {
                            log += "\n-Stand";
                            summon.jobComponent.TriggerStand(out producedJob);
                        }
                        return true;
                    } else {
                        log += "\n-Outside territory or home structure";
                        int fiftyPercentOfMaxHP = Mathf.RoundToInt(summon.maxHP * 0.5f);
                        if (summon.currentHP < fiftyPercentOfMaxHP) {
                            log += "\n-Less than 50% of Max HP, Return Territory or Home";
                            if (summon.homeStructure != null || summon.HasTerritory()) {
                                return summon.jobComponent.PlanIdleReturnHome(out producedJob);
                            } else {
                                log += "\n-No home structure or territory: THIS MUST NOT HAPPEN!";
                            }
                        } else {
                            log += "\n-50% chance to Roam Around Tile";
                            int roll = UnityEngine.Random.Range(0, 100);
                            log += $"\n-Roll: {roll.ToString()}";
                            if (roll < 50) {
                                summon.jobComponent.TriggerRoamAroundTile(out producedJob);
                                return true;
                            } else {
                                log += "\n-Return Territory or Home";
                                if (summon.homeStructure != null || summon.HasTerritory()) {
                                    return summon.jobComponent.PlanIdleReturnHome(out producedJob);
                                } else {
                                    log += "\n-No home structure or territory: THIS MUST NOT HAPPEN!";
                                }
                            }
                        }
                    }
                }
            }
		}
		return false;
	}
}
