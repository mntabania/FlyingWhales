using System.Collections;
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
            List<Area> targetChoices = ObjectPoolManager.Instance.CreateNewAreaList();
            PopulateTargetChoices(targetChoices, character);
            if (targetChoices != null) {
                Area chosenTarget = CollectionUtilities.GetRandomElement(targetChoices);
                character.behaviourComponent.SetAbominationTarget(chosenTarget);
            }
            ObjectPoolManager.Instance.ReturnAreaListToPool(targetChoices);
        }

        if (character.behaviourComponent.abominationTarget == null) {
            //if still no target tile, then just roam around current one
            return character.jobComponent.TriggerRoamAroundTile(out producedJob);
        } else {
            LocationGridTile targetTile = character.behaviourComponent.abominationTarget.gridTileComponent.GetRandomPassableTile();
            return character.jobComponent.TriggerRoamAroundTile(out producedJob, targetTile);
        }
    }

    private void PopulateTargetChoices(List<Area> areas, Character actor) {
        Area areaLocation = actor.areaLocation;
        if (areaLocation != null) {
            for (int i = 0; i < actor.areaLocation.neighbourComponent.neighbours.Count; i++) {
                Area neighbour = actor.areaLocation.neighbourComponent.neighbours[i];
                if (neighbour.elevationType != ELEVATION.WATER && neighbour.region == actor.currentRegion && actor.movementComponent.HasPathTo(neighbour)) {
                    areas.Add(neighbour);
                }
            }
        }
    }
}