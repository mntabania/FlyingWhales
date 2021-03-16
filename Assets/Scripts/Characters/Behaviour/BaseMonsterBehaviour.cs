﻿using UnityEngine;
using UtilityScripts;
using System.Linq;

public abstract class BaseMonsterBehaviour : CharacterBehaviourComponent {
    public sealed override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        if (character is Summon summon && summon.isTamed) {
            return TamedBehaviour(character, ref log, out producedJob);
        } else if (character.faction != null && character.faction.isPlayerFaction) {
            return MonsterUnderlingBehaviour(character, ref log, out producedJob);
        } else {
            return WildBehaviour(character, ref log, out producedJob);
        }
    }

    protected virtual bool TamedBehaviour(Character p_character, ref string p_log, out JobQueueItem p_producedJob) {
        if (TryTakeSettlementJob(p_character, ref p_log, out p_producedJob)) {
            return true;
        } else {
            if (TryTakePersonalPatrolJob(p_character, 15, ref p_log, out p_producedJob)) {
                return true;
            }
            return TriggerRoamAroundTerritory(p_character, ref p_log, out p_producedJob);
        }
    }
    protected abstract bool WildBehaviour(Character p_character, ref string p_log, out JobQueueItem p_producedJob);
    protected virtual bool MonsterUnderlingBehaviour(Character p_character, ref string p_log, out JobQueueItem p_producedJob) {
        return WildBehaviour(p_character, ref p_log, out p_producedJob);
    }

    #region Utilities
    protected bool TryTakeSettlementJob(Character p_character, ref string p_log, out JobQueueItem p_producedJob) {
        p_log = $"{p_log}\n-{p_character.name} will try to take a settlement job";
        if (p_character.behaviourComponent.PlanWorkActions(out p_producedJob)) {
            p_log = $"{p_log}\n-{p_character.name} found a valid settlement job: {p_producedJob}.";
            return true;
        }
        p_log = $"{p_log}\n-{p_character.name} could not find a valid settlement job that it could take.";
        p_producedJob = null;
        return false;
    }
    protected bool TryTakePersonalPatrolJob(Character p_character, int chance, ref string p_log, out JobQueueItem p_producedJob) {
        p_log = $"{p_log}\n-{p_character.name} will try to create a personal patrol job.";
        if (GameUtilities.RollChance(chance, ref p_log)) {
            if (p_character.jobComponent.TriggerPersonalPatrol(out p_producedJob)) {
                p_log = $"{p_log}\n-{p_character.name} created personal patrol job.";
                return true;
            }
            p_log = $"{p_log}\n-{p_character.name} did not create personal patrol job";
        } else {
            p_log = $"{p_log}\n-{p_character.name} did personal patrol job chance not met";  
        }
        p_producedJob = null;
        return false;
    }
    protected bool TriggerRoamAroundTerritory(Character p_character, ref string p_log, out JobQueueItem p_producedJob) {
        p_log = $"{p_log}\n-{p_character.name} will roam around territory (TAMED)";    
        return p_character.jobComponent.TriggerRoamAroundTerritory(out p_producedJob);
    }
    protected bool DefaultWildMonsterBehaviour(Character p_character, ref string p_log, out JobQueueItem p_producedJob) {
        p_producedJob = null;
		if (p_character is Summon summon) {
			p_log += $"\n-{summon.name} is monster";
			if (summon.gridTileLocation != null) {
                if((summon.homeStructure == null || summon.homeStructure.hasBeenDestroyed) && !summon.HasTerritory() && (summon.faction == null || !summon.faction.isPlayerFaction)) {
                    p_log += "\n-No home structure and territory";
                    p_log += "\n-Trigger Set Home interrupt";
                    summon.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, null);
                    if (summon.homeStructure == null && !summon.HasTerritory()) {
                        p_log += "\n-Still no home structure and territory";
                        //p_log += "\n-50% chance to Roam Around Tile";
                        p_log += "\n-Roam Around Tile";
                        return summon.jobComponent.TriggerRoamAroundTile(out p_producedJob);
                        //int roll = UnityEngine.Random.Range(0, 100);
                        //p_log += "\n-Roll: " + roll;
                        //if (roll < 50) {
                        //    summon.jobComponent.TriggerRoamAroundTile(out p_producedJob);
                        //} else {
                        //    p_log += "\n-Otherwise, Visit Different Region";
                        //    if (!summon.jobComponent.TriggerVisitDifferentRegion()) {
                        //        p_log += "\n-Cannot perform Visit Different Region, Roam Around Tile";
                        //        summon.jobComponent.TriggerRoamAroundTile(out p_producedJob);
                        //    }
                        //}
                        //return true;
                    }
                    return true;
                } else {
                    if (summon.isAtHomeStructure || summon.IsInTerritory()) {
                        bool hasAddedJob = false;
                        p_log += "\n-Inside territory or home structure";
                        int fiftyPercentOfMaxHP = Mathf.RoundToInt(summon.maxHP * 0.5f);
                        if (summon.currentHP < fiftyPercentOfMaxHP && !summon.traitContainer.HasTrait("Poisoned") && !summon.traitContainer.HasTrait("Burning")) {
                            p_log += "\n-Less than 50% of Max HP, Sleep";
                            hasAddedJob = summon.jobComponent.TriggerMonsterSleep(out p_producedJob);
                        } else {
                            p_log += "\n-35% chance to Roam Around Territory";
                            int roll = UnityEngine.Random.Range(0, 100);
                            p_log += $"\n-Roll: {roll.ToString()}";
                            if (roll < 35) {
                                hasAddedJob = summon.jobComponent.TriggerRoamAroundTerritory(out p_producedJob);
                            } else {
                                TIME_IN_WORDS currTime = GameManager.Instance.GetCurrentTimeInWordsOfTick();
                                if (currTime == TIME_IN_WORDS.LATE_NIGHT || currTime == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                                    p_log += "\n-Late Night or After Midnight, 40% chance to Sleep";
                                    int sleepRoll = UnityEngine.Random.Range(0, 100);
                                    p_log += $"\n-Roll: {sleepRoll.ToString()}";
                                    if (sleepRoll < 40) {
                                        hasAddedJob = summon.jobComponent.TriggerMonsterSleep(out p_producedJob);
                                    }
                                } else {
                                    p_log += "\n-5% chance to Sleep";
                                    int sleepRoll = UnityEngine.Random.Range(0, 100);
                                    p_log += $"\n-Roll: {sleepRoll.ToString()}";
                                    if (sleepRoll < 5) {
                                        hasAddedJob = summon.jobComponent.TriggerMonsterSleep(out p_producedJob);
                                    }
                                }
                            }
                        }
                        if (!hasAddedJob) {
                            //Livestocks should not move around its hextile because it will be weird if they keep going outside its hextile even when there is a hunter lodge structure already built
                            if(summon is Animal) {
                                p_log += "\n-Roam around tile";
                                summon.jobComponent.TriggerRoamAroundTile(out p_producedJob);
                            } else {
                                p_log += "\n-Stand";
                                summon.jobComponent.TriggerStand(out p_producedJob);
                            }
                        }
                        return true;
                    } else {
                        p_log += "\n-Outside territory or home structure";
                        int fiftyPercentOfMaxHP = Mathf.RoundToInt(summon.maxHP * 0.5f);
                        if (summon.currentHP < fiftyPercentOfMaxHP) {
                            p_log += "\n-Less than 50% of Max HP, Return Territory or Home";
                            if (summon.homeStructure != null || summon.HasTerritory()) {
                                return summon.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out p_producedJob);
                            } else {
                                p_log += "\n-No home structure or territory: THIS MUST NOT HAPPEN!";
                            }
                        } else {
                            p_log += "\n-50% chance to Roam Around Tile";
                            int roll = UnityEngine.Random.Range(0, 100);
                            p_log += $"\n-Roll: {roll.ToString()}";
                            if (roll < 50) {
                                summon.jobComponent.TriggerRoamAroundTile(out p_producedJob);
                                return true;
                            } else {
                                p_log += "\n-Return Territory or Home";
                                if (summon.homeStructure != null || summon.HasTerritory()) {
                                    return summon.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out p_producedJob);
                                } else {
                                    p_log += "\n-No home structure or territory: THIS MUST NOT HAPPEN!";
                                }
                            }
                        }
                    }
                }
            }
		}
		return false;
    }
    protected bool TryTriggerLayEgg(Character character, int maxResidentCount, TILE_OBJECT_TYPE eggType, out JobQueueItem producedJob) {
        int residentCount = 0;
        int eggCount = 0;
        if (character.homeSettlement != null) {
            residentCount = character.homeSettlement.residents.Count(x => x.isDead == false && x.race == character.race); //&& (x is GiantSpider || x is SmallSpider))
            eggCount = character.homeSettlement.GetNumberOfTileObjectsThatMeetCriteria(eggType, null);
        } else if (character.homeStructure != null) {
            residentCount = character.homeStructure.residents.Count(x => x.isDead == false);
            eggCount = character.homeStructure.GetNumberOfTileObjectsThatMeetCriteria(eggType, null);
        } else if (character.HasTerritory()) {
            residentCount = character.homeRegion.GetCountOfAliveCharacterWithSameTerritory(character);
            eggCount = character.territory.tileObjectComponent.GetNumberOfTileObjectsInHexTile(eggType);
        }
        if (residentCount < maxResidentCount && eggCount < 2) {
            return character.jobComponent.TriggerLayEgg(out producedJob);
        }
        producedJob = null;
        return false;
    }
    #endregion
}
