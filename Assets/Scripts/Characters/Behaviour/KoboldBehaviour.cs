using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UtilityScripts;

public class KoboldBehaviour : CharacterBehaviourComponent {
    
    public KoboldBehaviour() {
        priority = 9;
    }
    
    public override bool TryDoBehaviour(Character character, ref string log) {
        if (UnityEngine.Random.Range(0, 100) < 10) {
            // List<HexTile> hexTileChoices = GetTilesNextToActiveSettlement(character.currentRegion);
            List<HexTile> hexTileChoices = GetTilesNextToHome(character);
            if (hexTileChoices.Count > 0) {
                HexTile chosenTile = CollectionUtilities.GetRandomElement(hexTileChoices);
                List<LocationGridTile> locationGridTileChoices =
                    chosenTile.locationGridTiles.Where(x => 
                        x.hasFreezingTrap == false && x.isOccupied == false && x.IsNextToSettlement() == false).ToList();
                if (locationGridTileChoices.Count > 0) {
                    LocationGridTile targetTile = CollectionUtilities.GetRandomElement(locationGridTileChoices);
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.PLACE_TRAP,
                        INTERACTION_TYPE.PLACE_FREEZING_TRAP, targetTile.genericTileObject, character);
                    job.AddOtherData(INTERACTION_TYPE.PLACE_FREEZING_TRAP,  
                        new object[] { new TrapChecker(c => c.race != RACE.KOBOLD) });
                    character.jobQueue.AddJobInQueue(job);
                    return true;
                } else {
                    return false;
                }
            } else {
                return false;
            }
        }
        return false;
    }
    
    private List<HexTile> GetTilesNextToActiveSettlement(Region region) {
        List<HexTile> hexTiles = new List<HexTile>();
        for (int i = 0; i < region.tiles.Count; i++) {
            HexTile tile = region.tiles[i];
            if (tile.HasActiveSettlementNeighbour()) {
                hexTiles.Add(tile);
            }
        }
        return hexTiles;
    }
    private List<HexTile> GetTilesNextToHome(Character character) {
        if (character.homeStructure?.occupiedHexTile != null) {
            HexTile homeTile = character.homeStructure.occupiedHexTile.hexTileOwner;
            return homeTile.AllNeighbours.Where(x => x.region == homeTile.region).ToList();
        } else if (character.territorries != null && character.territorries.Count > 0) {
            HexTile homeTile = CollectionUtilities.GetRandomElement(character.territorries);
            return homeTile.AllNeighbours.Where(x => x.region == homeTile.region).ToList();
        }
        return null;
    }
}