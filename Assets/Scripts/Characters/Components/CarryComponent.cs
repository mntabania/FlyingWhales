using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;

public class CarryComponent {
    public Character owner { get; private set; }
    public IPointOfInterest carriedPOI { get; private set; }
    public Character isBeingCarriedBy { get; private set; }

    #region getters
    public bool isCarryingAnyPOI => carriedPOI != null;
    public Character masterCharacter => isBeingCarriedBy != null ? isBeingCarriedBy : owner;
    #endregion

    public CarryComponent(Character owner) {
        this.owner = owner;
    }

    #region General
    public void SetIsBeingCarriedBy(Character carrier) {
        if(isBeingCarriedBy != carrier) {
            isBeingCarriedBy = carrier;
            if (owner.marker) {
                if (isBeingCarriedBy != null) {
                    owner.marker.visionTrigger.SetAllCollidersState(false);
                } else {
                    owner.marker.visionTrigger.SetAllCollidersState(true);
                }
                owner.marker.UpdateAnimation();
            }
        }
    }
    public bool CarryPOI(IPointOfInterest poi, bool isOwner = false) {
        if (poi is Character) {
            return CarryCharacter(poi as Character, isOwner);
        } else if (poi is TileObject) {
            return CarryTileObkect(poi as TileObject);
        }
        return false;
    }
    private bool CarryTileObkect(TileObject tileObject) {
        if (carriedPOI == null) {
            carriedPOI = tileObject;
            // tileObject.SetIsBeingCarriedBy(owner);
            if (tileObject.gridTileLocation != null) {
                tileObject.gridTileLocation.structure.RemovePOIWithoutDestroying(tileObject);
            }
            if (tileObject.mapVisual == null) {
                tileObject.InitializeMapObject(tileObject);
            }
            //tileObject.SetGridTileLocation(owner.gridTileLocation);
            tileObject.visionTrigger.SetAllCollidersState(false);
            Transform mapVisualTransform;
            (mapVisualTransform = tileObject.mapVisual.transform).SetParent(owner.marker.visualsParent);
            mapVisualTransform.localPosition = new Vector3(0f, 0.5f, 0f);
            mapVisualTransform.eulerAngles = Vector3.zero;
            tileObject.mapVisual.UpdateSortingOrders(tileObject);
            return true;
        }
        return false;
    }
    private bool CarryCharacter(Character character, bool isOwner) {
        if (carriedPOI == null) {
            carriedPOI = character;
            character.carryComponent.SetIsBeingCarriedBy(owner);

            character.SetGridTileLocation(owner.gridTileLocation);
            character.SetCurrentStructureLocation(owner.currentStructure);
            character.marker.transform.SetParent(owner.marker.visualsParent);
            character.marker.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            character.marker.visualsParent.eulerAngles = Vector3.zero;
            character.marker.transform.eulerAngles = Vector3.zero;
            character.marker.SetNameState(false);

            Traits.Plagued targetPlagued = character.traitContainer.GetNormalTrait<Traits.Plagued>("Plagued");
            if (targetPlagued != null) {
                string plaguedSummary = $"{owner.name} carried a plagued character. Rolling for infection.";
                int roll = UnityEngine.Random.Range(0, 100);
                int carryInfectChance = targetPlagued.GetCarryInfectChance();
                plaguedSummary += $"\nRoll is: {roll.ToString()}, Chance is: {carryInfectChance.ToString()}";
                if (roll < carryInfectChance) {
                    //carrier will be infected with plague
                    plaguedSummary += $"\nWill infect {owner.name} with plague!";
                    owner.interruptComponent.TriggerInterrupt(INTERRUPT.Plagued, owner);
                    // if (owner.traitContainer.AddTrait(owner, "Plagued", character)) {
                    //     Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "contracted_plague");
                    //     log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    //     log.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    //     log.AddLogToInvolvedObjects();
                    // }
                }
                Debug.Log(GameManager.Instance.TodayLogString() + plaguedSummary);
            }
            Messenger.Broadcast(Signals.CHARACTER_JOINED_PARTY, character, this);
            return true;
        }
        return false;
    }
    public void UncarryPOI(IPointOfInterest poi, bool addToLocation = true, LocationGridTile dropLocation = null) {
        if (IsPOICarried(poi)) {
            if (poi is Character) {
                RemoveCharacter(poi as Character, addToLocation, dropLocation);
            } else if (poi is TileObject) {
                RemoveTileObject(poi as TileObject, addToLocation, dropLocation);
            }
        }
    }
    private void RemoveTileObject(TileObject tileObject, bool addToLocation, LocationGridTile dropLocation) {
        carriedPOI = null;
        // tileObject.SetIsBeingCarriedBy(null);
        if (addToLocation) {
            //tileObject.areaMapVisual.collisionTrigger.SetMainColliderState(true);
            if (dropLocation == null) {
                if (owner.gridTileLocation.isOccupied) {
                    LocationGridTile chosenTile = owner.gridTileLocation.GetRandomUnoccupiedNeighbor();
                    if (chosenTile != null) {
                        owner.gridTileLocation.structure.AddPOI(tileObject, chosenTile);
                    } else {
                        Debug.LogWarning(
                            $"{GameManager.Instance.TodayLogString()}{tileObject.name} is being dropped by {owner.name} but there is no unoccupied neighbor tile including the tile he/she is standing on. Default behavior is to drop character on the tile he/she is standing on regardless if it is unoccupied or not.");
                        owner.gridTileLocation.structure.AddPOI(tileObject);
                    }
                } else {
                    owner.gridTileLocation.structure.AddPOI(tileObject, owner.gridTileLocation);
                }
            } else {
                owner.gridTileLocation.structure.AddPOI(tileObject, dropLocation);
            }
        } else {
            if (tileObject.gridTileLocation != null) {
                tileObject.gridTileLocation.structure.RemovePOIDestroyVisualOnly(tileObject, owner);
            } else if (tileObject.mapVisual != null) {
                tileObject.DestroyMapVisualGameObject();
            }
        }
        if (tileObject.mapVisual != null) {
            tileObject.mapVisual.transform.eulerAngles = Vector3.zero;
        }
        //character.ownParty.icon.transform.position = this.specificLocation.coreTile.transform.position;
        //Messenger.Broadcast(Signals.CHARACTER_LEFT_PARTY, character, this);
    }
    private void RemoveCharacter(Character character, bool addToLocation, LocationGridTile dropLocation) {
        if (owner == character) {
            return;
        }
        //LocationGridTile gridTile = owner.gridTileLocation.GetNearestUnoccupiedTileFromThis();
        //owner.specificLocation.AddCharacterToLocation(character);
        carriedPOI = null;
        character.carryComponent.SetIsBeingCarriedBy(null);
        if (dropLocation == null) {
            if (owner.gridTileLocation.isOccupied) {
                LocationGridTile chosenTile = owner.gridTileLocation.GetRandomUnoccupiedNeighbor();
                if (chosenTile != null) {
                    character.marker.PlaceMarkerAt(chosenTile, addToLocation);
                } else {
                    Debug.LogWarning(
                        $"{GameManager.Instance.TodayLogString()}{character.name} is being dropped by {owner.name} but there is no unoccupied neighbor tile including the tile he/she is standing on. Default behavior is to drop character on the tile he/she is standing on regardless if it is unoccupied or not.");
                    character.marker.PlaceMarkerAt(owner.gridTileLocation, addToLocation);
                }
            } else {
                character.marker.PlaceMarkerAt(owner.gridTileLocation, addToLocation);
            }
        } else {
            character.marker.PlaceMarkerAt(dropLocation, addToLocation);
        }

        character.marker.transform.eulerAngles = Vector3.zero;
        character.marker.SetNameState(true);

        character.avatar.transform.position = owner.currentRegion.coreTile.transform.position;
        Messenger.Broadcast(Signals.CHARACTER_LEFT_PARTY, character, this);
    }
    public bool IsPOICarried(IPointOfInterest poi) {
        return carriedPOI == poi;
    }
    public bool IsPOICarried(string name) {
        return carriedPOI != null && carriedPOI.name == name;
    }
    public bool IsNotBeingCarried() {
        return isBeingCarriedBy == null;
    }
    public bool IsCurrentlyPartOf(Character character) {
        return character != null && (owner == character || isBeingCarriedBy == character);
    }
    #endregion
}
