using System;
using System.Collections.Generic;
using Inner_Maps;
using UtilityScripts;

public class ArsonistBehaviour : CharacterBehaviourComponent {

    public ArsonistBehaviour() {
        priority = 30;
    }
    
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        if (character.behaviourComponent.canArson) {
            if (character.behaviourComponent.arsonVillageTarget.Count <= 0) {
                //character.behaviourComponent.ResetArsonistVillageTarget();
                character.behaviourComponent.PopulateVillageTargetsByPriority(character.behaviourComponent.arsonVillageTarget);
                //character.behaviourComponent.SetArsonistVillageTarget(target);
            } else {
                //if already has arson village target, check if it is still valid.
                bool shouldRepopulate = false;
                Area target = character.behaviourComponent.arsonVillageTarget[0];
                if (target.IsPartOfVillage(out var village)) {
                    if (village.owner != null && !village.owner.IsHostileWith(PlayerManager.Instance.player.playerFaction)) {
                        //target is no longer hostile with player faction, redetermine target.
                        shouldRepopulate = true;
                    }
                }
                //for (int i = 0; i < character.behaviourComponent.arsonVillageTarget.Count; i++) {
                //    Area target = character.behaviourComponent.arsonVillageTarget[i];
                //    if (target.IsPartOfVillage(out var village)) {
                //        if (village.owner != null && !village.owner.IsHostileWith(PlayerManager.Instance.player.playerFaction)) {
                //            //target is no longer hostile with player faction, redetermine target.
                //            shouldRepopulate = true;
                //            break;
                //        }
                //    }
                //}
                if (shouldRepopulate) {
                    character.behaviourComponent.ResetArsonistVillageTarget();
                    character.behaviourComponent.PopulateVillageTargetsByPriority(character.behaviourComponent.arsonVillageTarget);
                    //character.behaviourComponent.SetArsonistVillageTarget(newTarget);
                }
            }
            if (character.behaviourComponent.arsonVillageTarget.Count > 0) {
                Area areaLocation = character.areaLocation;
                if (areaLocation != null && character.behaviourComponent.arsonVillageTarget.Contains(areaLocation)) {
                    //character is already at village target, do arson job on random object inside village target
                    List<TileObject> arsonChoices = GetArsonTargetChoices(character);
                    if (arsonChoices != null) {
                        TileObject target = CollectionUtilities.GetRandomElement(arsonChoices);
                        return character.jobComponent.TriggerArson(target, out producedJob);
                    } else {
                        //no arson choices, go to random tile at village
                        Area targetArea =
                            CollectionUtilities.GetRandomElement(character.behaviourComponent.arsonVillageTarget);
                        LocationGridTile targetTile = CollectionUtilities.GetRandomElement(targetArea.gridTileComponent.borderTiles);
                        return character.jobComponent.CreateGoToJob(targetTile, out producedJob);
                    }
                } else {
                    //go to target village
                    Area targetArea =
                        CollectionUtilities.GetRandomElement(character.behaviourComponent.arsonVillageTarget);
                    LocationGridTile targetTile = CollectionUtilities.GetRandomElement(targetArea.gridTileComponent.borderTiles);
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
        character.behaviourComponent.ResetArsonistVillageTarget();
        character.behaviourComponent.PopulateVillageTargetsByPriority(character.behaviourComponent.arsonVillageTarget);
        //character.behaviourComponent.SetArsonistVillageTarget(target);
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
        List<LocationGridTile> area = RuinarchListPool<LocationGridTile>.Claim();
        arson.gridTileLocation.PopulateTilesInRadius(area, 2, includeCenterTile: true, includeTilesInDifferentStructure: true);
        for (int i = 0; i < area.Count; i++) {
            LocationGridTile tile = area[i];
            TileObject tileObject = tile.tileObjectComponent.objHere;
            if (tileObject != null &&
                tileObject.traitContainer.HasTrait("Burning", "Fire Resistant") == false &&
                tileObject.traitContainer.HasTrait("Flammable")) {
                if (choices == null) {
                    choices = new List<TileObject>();
                }
                choices.Add(tileObject);
            }
        }
        RuinarchListPool<LocationGridTile>.Release(area);
        return choices;
    }
}
