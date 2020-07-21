using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

public class RevenantBehaviour : CharacterBehaviourComponent {
    public RevenantBehaviour() {
        priority = 8;
        // attributes = new[] { BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY };
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-{character.name} is a revenant";
        if (character.gridTileLocation != null) {
            TIME_IN_WORDS currentTimeOfDay = GameManager.GetCurrentTimeInWordsOfTick(character);
            if(currentTimeOfDay == TIME_IN_WORDS.EARLY_NIGHT || currentTimeOfDay == TIME_IN_WORDS.LATE_NIGHT || currentTimeOfDay == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                log += $"\n-Early/Late Night or After Midnight, 9% chance to spawn a ghost";
                int roll = UnityEngine.Random.Range(0, 100);
                log += $"\n-Roll: " + roll;
                if(roll < 10) {
                    if (character is Revenant revenant) {
                        if(revenant.numOfSummonedGhosts < 5) {
                            log += $"\n-Will spawn ghost";
                            Character betrayer = revenant.GetRandomBetrayer();
                            Summon ghost = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Ghost, FactionManager.Instance.undeadFaction, homeLocation: revenant.homeSettlement, homeRegion: revenant.homeRegion);
                            (ghost as Ghost).SetBetrayedBy(betrayer);
                            CharacterManager.Instance.PlaceSummon(ghost, revenant.homeSettlement.GetRandomHexTile().GetRandomTile());
                            revenant.AdjustNumOfSummonedGhosts(1);
                        } else {
                            log += $"\n-Already reached maximum number of spawned ghosts: 5";
                        }
                    }
                }
                
            }

            if ((character.HasTerritory() && !character.IsInTerritory()) || (character.homeStructure != null && !character.isAtHomeStructure)) {
                log += "\n-Return to territory";
                return character.jobComponent.PlanIdleReturnHome(out producedJob);
            } else {
                log += "\n-Already in territory or has no territory, Roam";
                return character.jobComponent.TriggerRoamAroundTile(out producedJob);
            }
        }
        return false;
    }
}