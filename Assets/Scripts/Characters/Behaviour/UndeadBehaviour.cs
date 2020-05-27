using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

public class UndeadBehaviour : CharacterBehaviourComponent {
	public UndeadBehaviour() {
		priority = 9;
	}
	public override bool TryDoBehaviour(Character character, ref string log) {
        log += $"\n-{character.name} is an undead";
        if (character.race == RACE.SKELETON) {
            log += $"\n-Character is a skeleton";
            Faction undeadFaction = FactionManager.Instance.undeadFaction;
            if(undeadFaction.leader != null && undeadFaction.leader is Character undeadFactionLeader) {
                log += $"\n-Undead faction has a character leader";
                if(undeadFactionLeader.necromancerTrait.lairStructure != null) {
                    LocationStructure lair = undeadFactionLeader.necromancerTrait.lairStructure;
                    log += $"\n-Undead faction leader has a lair";
                    log += $"\n-Character must migrate there if the lair is not yet his home";
                    if (character.homeStructure != lair) {
                        log += $"\n-Character migrated to the lair";
                        character.MigrateHomeStructureTo(lair);
                        character.ClearTerritory();
                    }
                    if (character.currentStructure == lair && undeadFactionLeader.currentStructure == lair) {
                        log += $"\n-Character and faction leader is in lair, roam";
                        character.jobComponent.TriggerRoamAroundTile();
                    } else {
                        if (!undeadFactionLeader.isBeingSeized && undeadFactionLeader.marker && undeadFactionLeader.gridTileLocation != null && !undeadFactionLeader.isDead) {
                            if (character.marker.inVisionCharacters.Contains(undeadFactionLeader)) {
                                log += $"\n-Character can see faction leader, do nothing";
                                return true;
                            } else {
                                log += $"\n-Character cannot see faction leader, go to him";
                                if (character.jobComponent.CreateGoToJob(undeadFactionLeader)) {
                                    return true;
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
