using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;
namespace Characters.Behaviour {
    public class SnatcherBehaviour : CharacterBehaviourComponent {

        public SnatcherBehaviour() {
            priority = 8;
        }
        
        public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
            if (character.hexTileLocation != null) {
                if (!character.hexTileLocation.isCorrupted) {
                    //character is not yet at demonic structure, get nearest one then go there.
                    HexTile nearestDemonicArea = GetNearestDemonicStructure(character);
                    if (nearestDemonicArea != null) {
                        List<LocationGridTile> choices = nearestDemonicArea.locationGridTiles.Where(
                            t => !t.structure.IsTilePartOfARoom(t, out var room) && t.IsPassable() && PathfindingManager.Instance.HasPathEvenDiffRegion(character.gridTileLocation, t)
                        ).ToList();
                        if (choices.Count > 0) {
                            LocationGridTile targetTile = CollectionUtilities.GetRandomElement(choices);
                            return character.jobComponent.CreateGoToJob(targetTile, out producedJob);    
                        } else {
                            character.PlanFixedJob(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.STAND, character, out producedJob);
                            return true;
                        }
                    } else {
                        Debug.LogWarning($"{character.name} could not find a near demonic structure!");
                    }
                } else {
                    List<LocationGridTile> choices = character.hexTileLocation.locationGridTiles.Where(
                        t => !t.structure.IsTilePartOfARoom(t, out var room) && t.IsPassable() && PathfindingManager.Instance.HasPath(character.gridTileLocation, t)
                    ).ToList();
                    if (choices.Count > 0) {
                        //character is already at demonic structure, just roam around there
                        return character.jobComponent.TriggerRoamAroundTile(JOB_TYPE.ROAM_AROUND_CORRUPTION, out producedJob, CollectionUtilities.GetRandomElement(choices));
                    } else {
                        character.PlanFixedJob(JOB_TYPE.IDLE_STAND, INTERACTION_TYPE.STAND, character, out producedJob);
                        return true;
                    }
                    
                }
            }
            producedJob = null;
            return false;
        }
        public override void OnAddBehaviourToCharacter(Character character) {
            base.OnAddBehaviourToCharacter(character);
            character.movementComponent.SetEnableDigging(true);
            character.behaviourComponent.OnBecomeSnatcher();
        }
        public override void OnRemoveBehaviourFromCharacter(Character character) {
            base.OnRemoveBehaviourFromCharacter(character);
            character.movementComponent.SetEnableDigging(false);
            character.behaviourComponent.OnNoLongerSnatcher();
        }
        public override void OnLoadBehaviourToCharacter(Character character) {
            base.OnLoadBehaviourToCharacter(character);
            character.behaviourComponent.OnBecomeSnatcher();
        }
        private HexTile GetNearestDemonicStructure(Character character) {
            if (character.hexTileLocation != null) {
                HexTile nearest = null;
                float nearestDist = 99999f;
                for (int i = 0; i < PlayerManager.Instance.player.playerSettlement.tiles.Count; i++) {
                    HexTile hexTile = PlayerManager.Instance.player.playerSettlement.tiles[i];
                    float dist = Vector2.Distance(hexTile.transform.position, character.hexTileLocation.transform.position);
                    if (dist < nearestDist) {
                        nearest = hexTile;
                        nearestDist = dist;
                    }
                }
                return nearest;
            }
            return null;
        }
    }
}