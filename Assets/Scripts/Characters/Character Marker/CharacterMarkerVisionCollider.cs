using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine;
using Traits;

public class CharacterMarkerVisionCollider : BaseVisionCollider {

    public CharacterMarker parentMarker;
    public List<IPointOfInterest> poisInRangeButDiffStructure = new List<IPointOfInterest>();

    private void OnDisable() {
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

    public void Initialize() {
        VoteToFilterVision();
        Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
    }
    public void Reset() {
        Messenger.RemoveListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
        OnDisable();
    }

    #region Triggers
    protected override void OnTriggerEnter2D(Collider2D collision) {
        if(!parentMarker || parentMarker.character == null) {
            return;
        }
        if (!parentMarker.character.IsInOwnParty()) {
            return;
        }
        POIVisionTrigger collidedWith = collision.gameObject.GetComponent<POIVisionTrigger>();
        if (collidedWith != null && collidedWith.damageable != null
            && collidedWith.damageable != parentMarker.character) {
            if (collidedWith.damageable is Character target) {
                if (!target.IsInOwnParty()) {
                    return;
                }
            }

            if (collidedWith.damageable.gridTileLocation == null) {
                return; //ignore, Usually happens if an item is picked up just as this character sees it.
            }
            List<Trait> traitOverrideFunctions = collidedWith.poi.traitContainer.GetTraitOverrideFunctions(TraitManager.Collision_Trait);
            if(traitOverrideFunctions != null) {
                for (int i = 0; i < traitOverrideFunctions.Count; i++) {
                    Trait trait = traitOverrideFunctions[i];
                    trait.OnCollideWith(parentMarker.character, collidedWith.poi);
                }
            }

            if (collidedWith.poi.gridTileLocation.structure == parentMarker.character.gridTileLocation.structure || collidedWith.IgnoresStructureDifference()) {
                //if it is, just follow the normal procedure when a poi becomes in range
                NormalEnterHandling(collidedWith.poi);
            } else {
                if (collidedWith.poi.gridTileLocation != null && collidedWith.poi.gridTileLocation.structure != null 
                    && parentMarker.character.gridTileLocation != null && parentMarker.character.gridTileLocation.structure != null
                    && collidedWith.poi.gridTileLocation.structure.structureType.IsOpenSpace() && parentMarker.character.gridTileLocation.structure.structureType.IsOpenSpace()) {
                    NormalEnterHandling(collidedWith.poi);
                }
                else {
                    AddPOIAsInRangeButDifferentStructure(collidedWith.poi);
                }
            }
        }
    }
    protected override void OnTriggerExit2D(Collider2D collision) {
        POIVisionTrigger collidedWith = collision.gameObject.GetComponent<POIVisionTrigger>();
        if (collidedWith != null && collidedWith.poi != null
            && collidedWith.poi != parentMarker.character) {
            parentMarker.RemovePOIFromInVisionRange(collidedWith.poi);
            RemovePOIAsInRangeButDifferentStructure(collidedWith.poi);
        }
    }
    #endregion
    
    private void NormalEnterHandling(IPointOfInterest poi) {
        parentMarker.AddPOIAsInVisionRange(poi);
    }

    #region Different Structure Handling
    public void AddPOIAsInRangeButDifferentStructure(IPointOfInterest poi) {
        poisInRangeButDiffStructure.Add(poi);
    }
    public void RemovePOIAsInRangeButDifferentStructure(IPointOfInterest poi) {
        poisInRangeButDiffStructure.Remove(poi);
    }
    private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
         //if the character that arrived at the new structure is in this character different structure list
         //check if that character now has the same structure as this character,
        if (poisInRangeButDiffStructure.Contains(character) && (structure == parentMarker.character.currentStructure || (structure.structureType.IsOpenSpace() && parentMarker.character.currentStructure.structureType.IsOpenSpace()))) {
            //if it does, add as normal
            NormalEnterHandling(character);
            RemovePOIAsInRangeButDifferentStructure(character);
        }
        //else if the character that arrived at the new structure is in this character's vision list and the character no longer has the same structure as this character, 
        else if (parentMarker.inVisionCharacters.Contains(character) && structure != parentMarker.character.currentStructure) {
            //if both characters are in open space, do not remove from vision
            if (structure.structureType.IsOpenSpace() && parentMarker.character.currentStructure.structureType.IsOpenSpace()) {
                return;
            }
            //remove from vision and hostile range
            parentMarker.RemovePOIFromInVisionRange(character);
            AddPOIAsInRangeButDifferentStructure(character);
        }
        //if the character that changed structures is this character
        else if (character.id == parentMarker.character.id) {
            //check all pois that were in different structures and revalidate them
            List<IPointOfInterest> pois = new List<IPointOfInterest>(poisInRangeButDiffStructure);
            for (int i = 0; i < pois.Count; i++) {
                IPointOfInterest poi = pois[i];
                if (poi.gridTileLocation == null || poi.gridTileLocation.structure == null) {
                    RemovePOIAsInRangeButDifferentStructure(poi);
                } else if (poi.gridTileLocation.structure == parentMarker.character.currentStructure
                    || (poi.gridTileLocation.structure.structureType.IsOpenSpace() && parentMarker.character.currentStructure.structureType.IsOpenSpace())) {
                    NormalEnterHandling(poi);
                    RemovePOIAsInRangeButDifferentStructure(poi);
                }
            }
            //also check all pois in vision
            pois = new List<IPointOfInterest>(parentMarker.inVisionPOIs);
            for (int i = 0; i < pois.Count; i++) {
                IPointOfInterest poi = pois[i];
                if (poi.gridTileLocation == null || poi.gridTileLocation.structure == null) {
                    parentMarker.RemovePOIFromInVisionRange(poi);
                } else if (poi.gridTileLocation.structure != parentMarker.character.currentStructure 
                    && (!poi.gridTileLocation.structure.structureType.IsOpenSpace() || !parentMarker.character.currentStructure.structureType.IsOpenSpace())) {
                    //if the character in vision no longer has the same structure as the character, and at least one of them is not in an open space structure
                    parentMarker.RemovePOIFromInVisionRange(poi);
                    AddPOIAsInRangeButDifferentStructure(poi);
                }
            }
        }
    }
    #endregion

    [ContextMenu("Log Diff Struct")]
    public void LogCharactersInDifferentStructures() {
        string summary = $"{parentMarker.character.name}'s diff structure pois";
        for (int i = 0; i < poisInRangeButDiffStructure.Count; i++) {
            summary += $"\n{poisInRangeButDiffStructure[i].name}";
        }
        Debug.Log(summary);
    }

    #region Utilities
    public void OnDeath() {
        poisInRangeButDiffStructure.Clear();
        OnDisable();
    }
    #endregion
}
