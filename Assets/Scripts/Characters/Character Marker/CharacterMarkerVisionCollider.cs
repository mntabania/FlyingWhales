using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Pathfinding;
using UnityEngine;
using Traits;

public class CharacterMarkerVisionCollider : BaseVisionCollider {

    public CharacterMarker parentMarker;
    private bool isApplicationQuitting = false;
    
    private void OnDisable() {
        if (isApplicationQuitting) { return; }
        if (LevelLoaderManager.Instance.isLoadingNewScene) { return; }
        if (SchedulingManager.Instance == null) { return; }
        if (parentMarker.inVisionPOIs != null) {
            parentMarker.ClearPOIsInVisionRange();
        }
        if (parentMarker.character?.combatComponent.hostilesInRange != null) {
            parentMarker.character.combatComponent.ClearHostilesInRange();
        }
        if (parentMarker.character?.combatComponent.avoidInRange != null) {
            parentMarker.character.combatComponent.ClearAvoidInRange();
        }
    }
    private void OnApplicationQuit() {
        isApplicationQuitting = true;
    }
    public void Initialize() {
        // Debug.Log($"Initialization of vision collider of {parentMarker.character?.name ?? parentMarker.name}");
        VoteToFilterVision();
        Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
    }
    public override void Reset() {
        base.Reset();
        Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
        OnDisable();
    }
    protected override void OnDestroy() {
        base.OnDestroy();
        parentMarker = null;
    }

    #region Triggers
    protected override void OnTriggerEnter2D(Collider2D collision) {
        if(!parentMarker || parentMarker.character == null) {
            return;
        }
        if (!parentMarker.character.carryComponent.IsNotBeingCarried()) {
            return;
        }
        POIVisionTrigger collidedWith = collision.gameObject.GetComponent<POIVisionTrigger>();
        if (collidedWith != null && collidedWith.damageable != null
            && collidedWith.damageable != parentMarker.character) {
            if (collidedWith.damageable is Character target) {
                if (!target.carryComponent.IsNotBeingCarried()) {
                    return;
                }
            }

            if (collidedWith.damageable.gridTileLocation == null) {
                if(collidedWith.damageable is FeebleSpirit || collidedWith.damageable is RavenousSpirit || collidedWith.damageable is ForlornSpirit) {
                    //Spirits can be collided even without gridTileLocation
                    //TODO: Put inside a system instead of special case?
                } else {
                    return; //ignore, Usually happens if an item is picked up just as this character sees it.
                }
            }
            List<Trait> traitOverrideFunctions = collidedWith.poi.traitContainer.GetTraitOverrideFunctions(TraitManager.Collision_Trait);
            if(traitOverrideFunctions != null) {
                for (int i = 0; i < traitOverrideFunctions.Count; i++) {
                    Trait trait = traitOverrideFunctions[i];
                    trait.OnCollideWith(parentMarker.character, collidedWith.poi);
                }
            }

            TryAddPOIToVision(collidedWith.poi);
        }
    }
    protected override void OnTriggerExit2D(Collider2D collision) {
        if (SchedulingManager.Instance == null) { return; } //need to check this for cleanup. Since SchedulingManager may have been already cleaned up, but actions in this function may try to access it. NOTE: Might be a better way to do this.
        POIVisionTrigger collidedWith = collision.gameObject.GetComponent<POIVisionTrigger>();
        if (collidedWith != null && collidedWith.poi != null && collidedWith.poi != parentMarker.character) {
            parentMarker.RemovePOIFromInVisionRange(collidedWith.poi);
            parentMarker.RemovePOIAsInRangeButDifferentStructure(collidedWith.poi);
        }
    }
    #endregion
    
    private void NormalEnterHandling(IPointOfInterest poi) {
        parentMarker.AddPOIAsInVisionRange(poi);
    }
    public void TransferAllDifferentStructureCharacters() {
#if DEBUG_LOG
        Debug.Log($"{GameManager.Instance.TodayLogString()} {parentMarker.character.name} is transferring all objects in different structures to its normal vision");
#endif
        for (int i = 0; i < parentMarker.inVisionPOIsButDiffStructure.Count; i++) {
            IPointOfInterest poi = parentMarker.inVisionPOIsButDiffStructure[i];
            if (poi.gridTileLocation == null) { continue; }
            if (TryAddPOIToVision(poi)) {
                if (parentMarker.RemovePOIAsInRangeButDifferentStructure(poi)) {
                    i--;
                }
            }
        }
    }
    public void ReCategorizeVision() {
#if DEBUG_LOG
        Debug.Log($"{GameManager.Instance.TodayLogString()} {parentMarker.character.name} Re categorizing objects in its normal vision");
#endif
        //List<IPointOfInterest> poisInVision = new List<IPointOfInterest>(parentMarker.inVisionPOIs);
        for (int i = 0; i < parentMarker.inVisionPOIs.Count; i++) {
            IPointOfInterest pointOfInterest = parentMarker.inVisionPOIs[i];
            if (pointOfInterest.gridTileLocation == null) { continue; }
            //if poi wasn't added to the characters normal vision, remove that poi from inVisionPOIs 
            if (TryAddPOIToVision(pointOfInterest) == false) {
                if (parentMarker.RemovePOIFromInVisionRange(pointOfInterest)) {
                    i--;
                }
            }
        }
    }
    /// <summary>
    /// Try to add a POI to this character's normal vision.
    /// </summary>
    /// <param name="poi">The POI to check.</param>
    /// <returns>Whether or not the poi was added to the character's normal vision.</returns>
    private bool TryAddPOIToVision(IPointOfInterest poi) {
        //(parentMarker.character.stateComponent.currentState != null && parentMarker.character.stateComponent.currentState is CombatState && 
        //parentMarker.character.movementComponent.HasPathTo(poi.gridTileLocation))|| 
        LocationStructure actorStructure = parentMarker.character.gridTileLocation?.structure;
        LocationStructure targetStructure = poi.gridTileLocation?.structure;
        if(parentMarker && actorStructure != null && targetStructure != null) {
            if (actorStructure.region == targetStructure.region) {
                //if same structure or ignores structure difference, add in vision
                if (ShouldBeConsideredInVision(poi) ||
                    (poi.mapObjectVisual != null && poi.mapObjectVisual.visionTrigger != null && poi.mapObjectVisual.visionTrigger is POIVisionTrigger poiVisionTrigger && poiVisionTrigger.IgnoresStructureDifference())) {
                    bool shouldAddToVisionBasedOnRoom = ShouldAddToVisionBasedOnRoom(poi);
                    if (shouldAddToVisionBasedOnRoom || 
                        (poi.mapObjectVisual != null && poi.mapObjectVisual.visionTrigger != null && poi.mapObjectVisual.visionTrigger is POIVisionTrigger visionTrigger && visionTrigger.IgnoresRoomDifference())) {
                        NormalEnterHandling(poi);
                        return true;
                    }
                }
                //if (actorStructure.structureType.IsOpenSpace() && targetStructure.structureType.IsOpenSpace()) {
                //    //if both the character that owns this and the object is part of a structure that is open space
                //    //process as if normal
                //    NormalEnterHandling(poi);
                //    return true;
                //}
                //if (parentMarker.IsCharacterInLineOfSightWith(poi)) {
                //    //If actor is in line of sight with target, add it in vision even if they are in different structures
                //    NormalEnterHandling(poi);
                //    return true;
                //}
                parentMarker.AddPOIAsInRangeButDifferentStructure(poi);
                return false;
            }
        }
        return false;
    }
    private bool ShouldAddToVisionBasedOnRoom(IPointOfInterest seenObject) {
        bool shouldAddToVision = true;
        if (seenObject.gridTileLocation != null && seenObject.gridTileLocation.structure.IsTilePartOfARoom(seenObject.gridTileLocation, out var firstRoom)) {
            //if tile of seen object is part of a room, check that this character is also part of that room
            shouldAddToVision = parentMarker.character.gridTileLocation.structure.IsTilePartOfARoom(parentMarker.character.gridTileLocation, out var otherRoom) && otherRoom == firstRoom;
        }
        else if (parentMarker.character.gridTileLocation != null && parentMarker.character.gridTileLocation.structure.IsTilePartOfARoom(parentMarker.character.gridTileLocation, out firstRoom)) {
            //if seen object is not part of a room, then check if this character is part of a room, if it is then immediately set shouldAddToVision as false since this character is in a room
            //and the seen object is not.
            shouldAddToVision = false;
        }
        return shouldAddToVision;
    }

#region Different Structure Handling
    private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
        //if (parentMarker.character.combatComponent.isInCombat) { return; } //if character is in combat, ignore this
        //if the character that arrived at the new structure is in this character different structure list
        //check if that character now has the same structure as this character,
        if (parentMarker.inVisionPOIsButDiffStructure.Contains(character) && 
            ShouldBeConsideredInVision(structure, character)) {
            //(structure == parentMarker.character.currentStructure || (structure.structureType.IsOpenSpace() && parentMarker.character.currentStructure.structureType.IsOpenSpace()))

            //if it does, add as normal
            bool shouldAddToVisionBasedOnRoom = ShouldAddToVisionBasedOnRoom(character);
            if (shouldAddToVisionBasedOnRoom) {
                NormalEnterHandling(character);
                parentMarker.RemovePOIAsInRangeButDifferentStructure(character);
            }
        }
        //else if the character that arrived at the new structure is in this character's vision list and the character no longer has the same structure as this character, 
        else if (parentMarker.IsPOIInVision(character) && !ShouldBeConsideredInVision(structure, character)) {
            //if (structure.structureType.IsOpenSpace() && parentMarker.character.currentStructure.structureType.IsOpenSpace()) {
            //    //if both characters are in open space, do not remove from vision
            //    return;
            //}
            //if (parentMarker.IsCharacterInLineOfSightWith(character)) {
            //    //if actor is still in line of sight with target, do not remove from vision even if they are in diff structure
            //    return;
            //}
            //remove from vision and hostile range
            parentMarker.RemovePOIFromInVisionRange(character);
            parentMarker.AddPOIAsInRangeButDifferentStructure(character);
        }
        //if the character that changed structures is this character
        else if (character.id == parentMarker.character.id) {
            //check all pois that were in different structures and revalidate them
            for (int i = 0; i < parentMarker.inVisionPOIsButDiffStructure.Count; i++) {
                IPointOfInterest poi = parentMarker.inVisionPOIsButDiffStructure[i];
                if (poi.gridTileLocation == null || poi.gridTileLocation.structure == null) {
                    if (parentMarker.RemovePOIAsInRangeButDifferentStructure(poi)) {
                        i--;
                    }
                } else if (ShouldBeConsideredInVision(poi)) {
                    bool shouldAddToVisionBasedOnRoom = ShouldAddToVisionBasedOnRoom(poi);
                    if (shouldAddToVisionBasedOnRoom) {
                        NormalEnterHandling(poi);
                        if (parentMarker.RemovePOIAsInRangeButDifferentStructure(poi)) {
                            i--;
                        }
                    }
                }
            }
            //also check all pois in vision
            for (int i = 0; i < parentMarker.inVisionPOIs.Count; i++) {
                IPointOfInterest poi = parentMarker.inVisionPOIs[i];
                if (poi.gridTileLocation == null || poi.gridTileLocation.structure == null) {
                    if (parentMarker.RemovePOIFromInVisionRange(poi)) {
                        i--;
                    }
                } else if (!ShouldBeConsideredInVision(poi)) {
                    //if the character in vision no longer has the same structure as the character, and at least one of them is not in an open space structure
                    if (parentMarker.RemovePOIFromInVisionRange(poi)) {
                        i--;
                    }
                    parentMarker.AddPOIAsInRangeButDifferentStructure(poi);
                }
            }

            if (character.currentActionNode != null && character.currentActionNode.action.actionLocationType == ACTION_LOCATION_TYPE.UPON_STRUCTURE_ARRIVAL && structure.structureType != STRUCTURE_TYPE.WILDERNESS) {
                bool isAtLocation = false;
                if (character.currentActionNode.targetStructure == structure) {
                    isAtLocation = true;
                } else if (character.currentActionNode.targetStructure is DemonicStructure demonicStructure) {
                    isAtLocation = demonicStructure.CanSeeObjectLocatedHere(character);
                }
                if (isAtLocation) {
                    parentMarker.pathfindingAI.ClearAllCurrentPathData();
                    character.PerformGoapAction();    
                }
            }
        }
    }
    private bool ShouldBeConsideredInVision(IPointOfInterest target) {
        LocationStructure targetStructure = target?.gridTileLocation?.structure;
        return ShouldBeConsideredInVision(targetStructure, target);
    }
    private bool ShouldBeConsideredInVision(LocationStructure targetStructure, IPointOfInterest target) {
        //if same structure, or both in open space, or is in line of sight, add in vision
        LocationStructure actorStructure = parentMarker.character?.currentStructure;
        return actorStructure != null && targetStructure != null
            && (IsTheSameStructureOrSameOpenSpace(actorStructure, targetStructure) || parentMarker.IsCharacterInLineOfSightWith(target));
    }
    public bool IsTheSameStructureOrSameOpenSpaceWithPOI(IPointOfInterest poi) {
        LocationStructure actorStructure = parentMarker.character?.currentStructure;
        LocationStructure targetStructure = poi?.gridTileLocation?.structure;

        return IsTheSameStructureOrSameOpenSpace(actorStructure, targetStructure);
    }
    private bool IsTheSameStructureOrSameOpenSpace(LocationStructure structure1, LocationStructure structure2) {
        return structure1 != null && structure2 != null && (structure1 == structure2 || (structure1.structureType.IsOpenSpace() && structure2.structureType.IsOpenSpace()));
    }
#endregion

    [ContextMenu("Log Diff Struct")]
    public void LogCharactersInDifferentStructures() {
#if DEBUG_LOG
        string summary = $"{parentMarker.character.name}'s diff structure pois";
        for (int i = 0; i < parentMarker.inVisionPOIsButDiffStructure.Count; i++) {
            summary += $"\n{parentMarker.inVisionPOIsButDiffStructure[i].name}";
        }
        Debug.Log(summary);
#endif
    }

#region Utilities
    public void OnDeath() {
        parentMarker.inVisionPOIsButDiffStructure.Clear();
        OnDisable();
    }
#endregion
}
