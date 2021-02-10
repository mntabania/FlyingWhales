﻿using System;
using System.Collections.Generic;
using Inner_Maps;
using UtilityScripts;

public class ArsonistBehaviour : CharacterBehaviourComponent {

    public ArsonistBehaviour() {
        priority = 30;
    }
    
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        if (character.behaviourComponent.canArson) {
            if (character.behaviourComponent.arsonVillageTarget == null) {
                List<HexTile> target = character.behaviourComponent.GetVillageTargetsByPriority();
                character.behaviourComponent.SetArsonistVillageTarget(target);
            } else {
                //if already has arson village target, check if it is still valid.
                for (int i = 0; i < character.behaviourComponent.arsonVillageTarget.Count; i++) {
                    HexTile target = character.behaviourComponent.arsonVillageTarget[i];
                    if (target.IsPartOfVillage(out var village)) {
                        if (village.owner != null && !village.owner.IsHostileWith(PlayerManager.Instance.player.playerFaction)) {
                            //target is no longer hostile with player faction, redetermine target.
                            List<HexTile> newTarget = character.behaviourComponent.GetVillageTargetsByPriority();
                            character.behaviourComponent.SetArsonistVillageTarget(newTarget);
                            break;
                        }
                    }
                }
            }
            if (character.behaviourComponent.arsonVillageTarget != null) {
                if (character.areaLocation != null && character.behaviourComponent.arsonVillageTarget.Contains(character.areaLocation)) {
                    //character is already at village target, do arson job on random object inside village target
                    List<TileObject> arsonChoices = GetArsonTargetChoices(character);
                    if (arsonChoices != null) {
                        TileObject target = CollectionUtilities.GetRandomElement(arsonChoices);
                        return character.jobComponent.TriggerArson(target, out producedJob);
                    } else {
                        //no arson choices, go to random tile at village
                        HexTile targetHextile =
                            CollectionUtilities.GetRandomElement(character.behaviourComponent.arsonVillageTarget);
                        LocationGridTile targetTile = CollectionUtilities.GetRandomElement(targetHextile.borderTiles);
                        return character.jobComponent.CreateGoToJob(targetTile, out producedJob);
                    }
                } else {
                    //go to target village
                    HexTile targetHextile =
                        CollectionUtilities.GetRandomElement(character.behaviourComponent.arsonVillageTarget);
                    LocationGridTile targetTile = CollectionUtilities.GetRandomElement(targetHextile.borderTiles);
                    return character.jobComponent.CreateGoToJob(targetTile, out producedJob);
                }
            } else {
                //roam around territory
                return character.jobComponent.TriggerRoamAroundTerritory(out producedJob);
            }
        } else {
            //roam around territory
            return character.jobComponent.TriggerRoamAroundTerritory(out producedJob);
        }
    }
    public override void OnAddBehaviourToCharacter(Character character) {
        base.OnAddBehaviourToCharacter(character);
        character.behaviourComponent.OnBecomeArsonist();
        //immediately set arson village target, so that arsonist will immediately do arson.
        List<HexTile> target = character.behaviourComponent.GetVillageTargetsByPriority();
        character.behaviourComponent.SetArsonistVillageTarget(target);
    }
    public override void OnRemoveBehaviourFromCharacter(Character character) {
        base.OnRemoveBehaviourFromCharacter(character);
        character.behaviourComponent.OnNoLongerArsonist();
    }
    public override void OnLoadBehaviourToCharacter(Character character) {
        base.OnLoadBehaviourToCharacter(character);
        character.behaviourComponent.OnBecomeArsonist();
    }
    private List<TileObject> GetArsonTargetChoices(Character arson) {
        List<TileObject> choices = null;
        List<LocationGridTile> area =
            arson.gridTileLocation.GetTilesInRadius(2, includeCenterTile: true, includeTilesInDifferentStructure: true);
        for (int i = 0; i < area.Count; i++) {
            LocationGridTile tile = area[i];
            if (tile.objHere is TileObject tileObject && 
                tileObject.traitContainer.HasTrait("Burning", "Fireproof") == false && 
                tileObject.traitContainer.HasTrait("Flammable")) {
                if (choices == null) {
                    choices = new List<TileObject>();
                }
                choices.Add(tileObject);
            }
        }
        return choices;
    }
}
