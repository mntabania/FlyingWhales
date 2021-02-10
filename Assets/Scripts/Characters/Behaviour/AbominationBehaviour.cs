﻿using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UtilityScripts;

public class AbominationBehaviour : BaseMonsterBehaviour {
    public AbominationBehaviour() {
        priority = 10;
    }
    protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        if (character.behaviourComponent.abominationTarget == null) {
            //determine new abomination target
            List<HexTile> targetChoices = GetTargetChoices(character);
            if (targetChoices != null) {
                HexTile chosenTarget = CollectionUtilities.GetRandomElement(targetChoices);
                character.behaviourComponent.SetAbominationTarget(chosenTarget);
            }
        }

        if (character.behaviourComponent.abominationTarget == null) {
            //if still no target tile, then just roam around current one
            return character.jobComponent.TriggerRoamAroundTile(out producedJob);
        } else {
            LocationGridTile targetTile =
                CollectionUtilities.GetRandomElement(character.behaviourComponent.abominationTarget.locationGridTiles);
            return character.jobComponent.TriggerRoamAroundTile(out producedJob, targetTile);
        }
    }

    private List<HexTile> GetTargetChoices(Character actor) {
        List<HexTile> choices = null;
        if (actor.areaLocation != null) {
            for (int i = 0; i < actor.areaLocation.AllNeighbours.Count; i++) {
                HexTile neighbour = actor.areaLocation.AllNeighbours[i];
                if (neighbour.elevationType != ELEVATION.WATER && neighbour.region == actor.currentRegion && actor.movementComponent.HasPathTo(neighbour)) {
                    if (choices == null) {
                        choices = new List<HexTile>();
                    }
                    choices.Add(neighbour);
                }
            }
        }
        return choices;
    }
   
}
