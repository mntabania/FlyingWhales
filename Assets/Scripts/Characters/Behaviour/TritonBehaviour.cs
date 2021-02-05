using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class TritonBehaviour : BaseMonsterBehaviour {

    public TritonBehaviour() {
        priority = 9;
    }

    protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        if (character.currentStructure is Kennel) {
            return false;
        }
        LocationGridTile tileLocation = character.gridTileLocation;
        if (tileLocation != null) {
            if(character is Triton triton) {
                if(tileLocation == triton.spawnLocationTile) {
                    //Disappear
                    triton.SetDestroyMarkerOnDeath(true);
                    triton.Death("disappear");
                    return true;
                } else {
                    return character.jobComponent.CreateGoToSpecificTileJob(triton.spawnLocationTile, out producedJob);
                }
            }
        }
        return false;
    }
}