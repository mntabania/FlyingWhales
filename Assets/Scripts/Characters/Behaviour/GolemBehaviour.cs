using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemBehaviour : BaseMonsterBehaviour {
	public GolemBehaviour() {
		priority = 8;
	}
	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
		if (character is Summon summon) {
#if DEBUG_LOG
			log += $"\n-{summon.name} is a golem";
#endif
			if (summon.gridTileLocation != null) {
                if((summon.homeStructure == null || summon.homeStructure.hasBeenDestroyed) && !summon.HasTerritory()) {
#if DEBUG_LOG
                    log += "\n-No home structure and territory";
                    log += "\n-Trigger Set Home interrupt";
#endif
                    summon.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, null);
                    if (summon.homeStructure == null && !summon.HasTerritory()) {
#if DEBUG_LOG
                        log += "\n-Still no home structure and territory";
                        //log += "\n-50% chance to Roam Around Tile";
                        log += "\n-Roam Around Tile";
#endif
                        return summon.jobComponent.TriggerRoamAroundTile(out producedJob);

                        //int roll = UnityEngine.Random.Range(0, 100);
                        //log += "\n-Roll: " + roll;
                        //if (roll < 50) {
                        //    summon.jobComponent.TriggerRoamAroundTile(out producedJob);
                        //} else {
                        //    log += "\n-Otherwise, Visit Different Region";
                        //    if (!summon.jobComponent.TriggerVisitDifferentRegion()) {
                        //        log += "\n-Cannot perform Visit Different Region, Roam Around Tile";
                        //        summon.jobComponent.TriggerRoamAroundTile(out producedJob);
                        //    }
                        //}
                        //return true;
                    }
                    return true;
                } else {
                    if (summon.isAtHomeStructure || summon.IsInTerritory()) {
                        bool hasAddedJob = false;
#if DEBUG_LOG
                        log += "\n-Inside territory or home structure";
#endif
                        int fiftyPercentOfMaxHP = Mathf.RoundToInt(summon.maxHP * 0.5f);
                        if (summon.currentHP < fiftyPercentOfMaxHP) {
#if DEBUG_LOG
                            log += "\n-Less than 50% of Max HP, Sleep";
#endif
                            hasAddedJob = summon.jobComponent.TriggerMonsterSleep(out producedJob);
                        } else {
                            int roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                            log += "\n-35% chance to Roam Around Territory";
                            log += $"\n-Roll: {roll.ToString()}";
#endif
                            if (roll < 35) {
                                hasAddedJob = summon.jobComponent.TriggerRoamAroundTerritory(out producedJob);
                            } else {
                                TIME_IN_WORDS currTime = GameManager.Instance.GetCurrentTimeInWordsOfTick();
                                if (currTime == TIME_IN_WORDS.LATE_NIGHT || currTime == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                                    int sleepRoll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                                    log += "\n-Late Night or After Midnight, 40% chance to Sleep";
                                    log += $"\n-Roll: {sleepRoll.ToString()}";
#endif
                                    if (roll < 40) {
                                        hasAddedJob = summon.jobComponent.TriggerMonsterSleep(out producedJob);
                                    }
                                } else {
                                    int sleepRoll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                                    log += "\n-5% chance to Sleep";
                                    log += $"\n-Roll: {sleepRoll.ToString()}";
#endif
                                    if (sleepRoll < 5) {
                                        hasAddedJob = summon.jobComponent.TriggerMonsterSleep(out producedJob);
                                    }
                                }
                            }
                        }
                        if (!hasAddedJob) {
#if DEBUG_LOG
                            log += "\n-Stand";
#endif
                            summon.jobComponent.TriggerStand(out producedJob);
                        }
                        return true;
                    } else {
#if DEBUG_LOG
                        log += "\n-Outside territory or home structure";
#endif
                        int fiftyPercentOfMaxHP = Mathf.RoundToInt(summon.maxHP * 0.5f);
                        if (summon.currentHP < fiftyPercentOfMaxHP) {
#if DEBUG_LOG
                            log += "\n-Less than 50% of Max HP, Return Territory or Home";
#endif
                            if (summon.homeStructure != null || summon.HasTerritory()) {
                                return summon.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
                            } else {
#if DEBUG_LOG
                                log += "\n-No home structure or territory: THIS MUST NOT HAPPEN!";
#endif
                            }
                        } else {
                            int roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                            log += "\n-50% chance to Roam Around Tile";
                            log += $"\n-Roll: {roll.ToString()}";
#endif
                            if (roll < 50) {
                                summon.jobComponent.TriggerRoamAroundTile(out producedJob);
                                return true;
                            } else {
#if DEBUG_LOG
                                log += "\n-Return Territory or Home";
#endif
                                if (summon.homeStructure != null || summon.HasTerritory()) {
                                    return summon.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
                                } else {
#if DEBUG_LOG
                                    log += "\n-No home structure or territory: THIS MUST NOT HAPPEN!";
#endif
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
