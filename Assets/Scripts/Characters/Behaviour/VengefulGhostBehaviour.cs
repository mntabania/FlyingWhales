using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UtilityScripts;

public class VengefulGhostBehaviour : BaseMonsterBehaviour {
	public VengefulGhostBehaviour() {
		priority = 8;
	}
	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        log += $"\n-{character.name} is a vengeful ghost";
        if (character.gridTileLocation != null) {
            if (!character.HasTerritory()) {
                log += "\n-No territory, will set nearest hex tile as territory";
                HexTile hex = character.gridTileLocation.GetNearestHexTileWithinRegion();
                character.SetTerritory(hex);
            }
            if (character.faction != null && character.faction.IsHostileWith(PlayerManager.Instance.player.playerFaction)) {
                log += $"\n-Hostile with player faction, will attack player";
                if (character.traitContainer.HasTrait("Invader")) {
                    character.traitContainer.RemoveTrait(character, "Invader");
                }
                if (!character.behaviourComponent.isAttackingDemonicStructure) {
                    log += $"\n-15% chance to attack a demonic structure";
                    if (GameUtilities.RollChance(15)) {
                        LocationStructure targetStructure = PlayerManager.Instance.player.playerSettlement.GetRandomStructureInRegion(character.currentRegion);
                        if (targetStructure != null) {
                            log += $"\n-Chosen target structure: " + targetStructure.name;
                            character.behaviourComponent.SetIsAttackingDemonicStructure(true, targetStructure as DemonicStructure);
                            return true;
                        } else {
                            log += $"\n-No demonic structure found in current region";
                        }
                    }
                }

                log += $"\n-20% chance to a character of the player faction";
                if (GameUtilities.RollChance(20)) {
                    Character targetCharacter = character.currentRegion.GetRandomCharacterThatMeetCriteria(c => c != character && !c.isDead && c.movementComponent.HasPathTo(character.gridTileLocation) && c.faction?.factionType.type == FACTION_TYPE.Demons);
                    if (targetCharacter != null) {
                        log += $"\n-Chosen target character: " + targetCharacter.name;
                        character.combatComponent.Fight(targetCharacter, CombatManager.Hostility);
                        return true;
                    } else {
                        log += $"\n-No character found in current region";
                    }
                }

                if (character.HasTerritory() && !character.IsInTerritory()) {
                    log += "\n-Return to territory";
                    return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
                } else {
                    log += "\n-Already in territory, Roam";
                    return character.jobComponent.TriggerRoamAroundTile(out producedJob);
                }
            } else {
                log += $"\n-Not Hostile with player faction, will attack villagers";
                if (!character.traitContainer.HasTrait("Invader")) {
                    character.traitContainer.AddTrait(character, "Invader");
                    return true;
                }
            }
        }
        return false;
	}
    protected override bool TamedBehaviour(Character p_character, ref string p_log, out JobQueueItem p_producedJob) {
        if (InvadeBehaviour(p_character, ref p_log, out p_producedJob)) {
            return true;
        } else if (GameUtilities.RollChance(10)) {
            if (p_character.behaviourComponent.invadeVillageTarget == null) {
                p_log += $"\n-No invade target yet, setting one...";
                p_character.behaviourComponent.SetInvadeVillageTarget(GetVillageTargetsByPriority(p_character));
            } 
        }
        return TriggerRoamAroundTerritory(p_character, ref p_log, out p_producedJob);
    }

    private bool InvadeBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        if (character.behaviourComponent.invadeVillageTarget != null) {
            log += $"\n-Already has village target";
            if (character.hexTileLocation != null && character.behaviourComponent.invadeVillageTarget.Contains(character.hexTileLocation)) {
                log += $"\n-Already at village target, will find character to attack";
                //character is already at target village
                List<Character> targets = ObjectPoolManager.Instance.CreateNewCharactersList();
                PopulateTargetChoicesFor(targets, character, character.behaviourComponent.invadeVillageTarget);
                if (targets.Count > 0) {
                    //Fight a random target
                    Character chosenTarget = CollectionUtilities.GetRandomElement(targets);
                    log += $"\n-Chosen target is {chosenTarget.name}";
                    character.combatComponent.Fight(chosenTarget, CombatManager.Hostility);
                } else {
                    log += $"\n-No more valid targets, clearing target village data...";
                    //No more valid targets exist, clearing village target. 
                    character.behaviourComponent.SetInvadeVillageTarget(null);
                }
                ObjectPoolManager.Instance.ReturnCharactersListToPool(targets);
                producedJob = null;
                return true;
            } else {
                log += $"\n-character is not yet at village target, will go there now...";
                //character is not yet at target village
                HexTile targetHextile = CollectionUtilities.GetRandomElement(character.behaviourComponent.invadeVillageTarget);
                LocationGridTile targetTile = CollectionUtilities.GetRandomElement(targetHextile.locationGridTiles);
                return character.jobComponent.CreateGoToJob(targetTile, out producedJob);
            }
        }
        producedJob = null;
        return false;
    }
    private void PopulateTargetChoicesFor(List<Character> p_targetChoices, Character p_invader, List<HexTile> tiles) {
        for (int i = 0; i < tiles.Count; i++) {
            HexTile tile = tiles[i];
            tile.PopulateCharacterListInsideHexThatMeetCriteria(p_targetChoices, c => c != p_invader && p_invader.IsHostileWith(c) && IsCharacterValidForInvade(c));
        }
    }
    private bool IsCharacterValidForInvade(Character character) {
        return !character.isDead && !character.traitContainer.HasTrait("Hibernating", "Indestructible") && !character.isInLimbo && !character.isBeingSeized && character.carryComponent.IsNotBeingCarried();
    }
    private List<HexTile> GetVillageTargetsByPriority(Character p_invader) {
        List<BaseSettlement> validSettlementsInRegion = p_invader.currentRegion?.GetSettlementsInRegion(
            settlement => settlement.locationType != LOCATION_TYPE.DEMONIC_INTRUSION && settlement.owner != null && settlement.owner != p_invader.faction && p_invader.faction.IsHostileWith(settlement.owner) && settlement.residents.Count(IsCharacterValidForInvade) > 0
        );
        if (validSettlementsInRegion != null) {
            return CollectionUtilities.GetRandomElement(validSettlementsInRegion).tiles;
        }
        return null;
    }
}
