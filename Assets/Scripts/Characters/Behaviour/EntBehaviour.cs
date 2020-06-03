using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntBehaviour : CharacterBehaviourComponent {
	public EntBehaviour() {
		priority = 9;
		// attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
	}
	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        log += $"\n-{character.name} is Ent";
        if (character.faction.isPlayerFaction || character.combatComponent.combatMode != COMBAT_MODE.Passive) {
            NormalMonsterBehaviour(character as Ent, ref log, out producedJob);
        } else {
            character.jobComponent.TriggerStandStill(out producedJob);
        }
		return true;
	}

    private void NormalMonsterBehaviour(Ent ent, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        if (ent.gridTileLocation != null) {
            if (ent.IsInTerritory()) {
                bool hasAddedJob = false;
                log += "\n-Inside territory";
                int fiftyPercentOfMaxHP = Mathf.RoundToInt(ent.maxHP * 0.5f);
                if (ent.currentHP < fiftyPercentOfMaxHP) {
                    log += "\n-Less than 50% of Max HP, Sleep";
                    hasAddedJob = ent.jobComponent.TriggerMonsterSleep(out producedJob);
                } else {
                    log += "\n-35% chance to Roam Around Territory";
                    int roll = UnityEngine.Random.Range(0, 100);
                    log += $"\n-Roll: {roll}";
                    if (roll < 35) {
                        hasAddedJob = ent.jobComponent.TriggerRoamAroundTerritory(out producedJob);
                    } else {
                        TIME_IN_WORDS currTime = GameManager.GetCurrentTimeInWordsOfTick();
                        if (currTime == TIME_IN_WORDS.LATE_NIGHT || currTime == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                            log += "\n-Late Night or After Midnight, 40% chance to Sleep";
                            int sleepRoll = UnityEngine.Random.Range(0, 100);
                            log += $"\n-Roll: {sleepRoll}";
                            if (roll < 40) {
                                hasAddedJob = ent.jobComponent.TriggerMonsterSleep(out producedJob);
                            }
                        } else {
                            log += "\n-5% chance to Sleep";
                            int sleepRoll = UnityEngine.Random.Range(0, 100);
                            log += $"\n-Roll: {sleepRoll}";
                            if (sleepRoll < 5) {
                                hasAddedJob = ent.jobComponent.TriggerMonsterSleep(out producedJob);
                            }
                        }
                    }
                }
                if (!hasAddedJob) {
                    log += "\n-Stand";
                    ent.jobComponent.TriggerStand(out producedJob);
                }
            } else {
                log += "\n-Outside territory";
                int fiftyPercentOfMaxHP = Mathf.RoundToInt(ent.maxHP * 0.5f);
                if (ent.currentHP < fiftyPercentOfMaxHP) {
                    log += "\n-Less than 50% of Max HP, Return Territory";
                    ent.jobComponent.TriggerReturnTerritory(out producedJob);
                } else {
                    log += "\n-50% chance to Roam Around Tile";
                    int roll = UnityEngine.Random.Range(0, 100);
                    log += $"\n-Roll: {roll}";
                    if (roll < 50) {
                        ent.jobComponent.TriggerRoamAroundTile(out producedJob);
                    } else {
                        log += "\n-Return Territory";
                        ent.jobComponent.TriggerReturnTerritory(out producedJob);
                    }
                }
            }
        }
    }
}
