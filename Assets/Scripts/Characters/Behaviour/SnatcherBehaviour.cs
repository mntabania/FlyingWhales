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
            Area areaLocation = character.areaLocation;
            if (areaLocation != null) {
                if (!areaLocation.gridTileComponent.HasCorruption()) {
                    //character is not yet at demonic structure, get nearest one then go there.
                    Area nearestDemonicArea = GetNearestDemonicStructure(character);
                    if (nearestDemonicArea != null) {
                        List<LocationGridTile> choices = nearestDemonicArea.gridTileComponent.gridTiles.Where(
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
#if DEBUG_LOG
                        Debug.LogWarning($"{character.name} could not find a near demonic structure!");
#endif
                    }
                } else {
                    List<LocationGridTile> choices = RuinarchListPool<LocationGridTile>.Claim();
                    LocationGridTile chosen = null;
                    for (int i = 0; i < areaLocation.gridTileComponent.gridTiles.Count; i++) {
                        LocationGridTile t = areaLocation.gridTileComponent.gridTiles[i];
                        if (!t.structure.IsTilePartOfARoom(t, out var room) && t.IsPassable() && PathfindingManager.Instance.HasPath(character.gridTileLocation, t)) {
                            choices.Add(t);
                        }
                    }
                    if (choices.Count > 0) {
                        chosen = CollectionUtilities.GetRandomElement(choices);
                    }
                    RuinarchListPool<LocationGridTile>.Release(choices);
                    if (chosen != null) {
                        //character is already at demonic structure, just roam around there
                        return character.jobComponent.TriggerRoamAroundTile(JOB_TYPE.ROAM_AROUND_CORRUPTION, out producedJob, chosen);
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
        private Area GetNearestDemonicStructure(Character character) {
            Area areaLocation = character.areaLocation;
            if (areaLocation != null) {
                Area nearest = null;
                float nearestDist = 0;
                for (int i = 0; i < PlayerManager.Instance.player.playerSettlement.areas.Count; i++) {
                    Area area = PlayerManager.Instance.player.playerSettlement.areas[i];
                    float dist = Vector2.Distance(area.gridTileComponent.centerGridTile.centeredWorldLocation, areaLocation.gridTileComponent.centerGridTile.centeredWorldLocation);
                    if (dist < nearestDist) {
                        nearest = area;
                        nearestDist = dist;
                    }
                }
                return nearest;
            }
            return null;
        }
    }
}