﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class CarryComponent : CharacterComponent {
    public IPointOfInterest carriedPOI { get; private set; }
    public Character isBeingCarriedBy { get; private set; }
    public Character justGotCarriedBy { get; private set; }

    #region getters
    public bool isCarryingAnyPOI => carriedPOI != null;
    public Character masterCharacter => isBeingCarriedBy != null ? isBeingCarriedBy : owner;
    #endregion
    public CarryComponent() {
    }

    public CarryComponent(SaveDataCarryComponent data) {
    }

    #region General
    public void SetIsBeingCarriedBy(Character carrier) {
        if(isBeingCarriedBy != carrier) {
            justGotCarriedBy = isBeingCarriedBy;
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
    public bool CarryPOI(IPointOfInterest poi, bool isOwner = false, bool isFromSave = false) {
        if (poi is Character) {
            return CarryCharacter(poi as Character, isOwner, isFromSave);
        } else if (poi is TileObject) {
            return CarryTileObject(poi as TileObject);
        }
        return false;
    }
    private bool CarryTileObject(TileObject tileObject) {
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
            Messenger.Broadcast(Signals.RELOAD_PLAYER_ACTIONS, tileObject as IPlayerActionTarget);
            return true;
        }
        return false;
    }
    private bool CarryCharacter(Character character, bool isOwner, bool isFromSave) {
        if (carriedPOI == null) {
            carriedPOI = character;
            character.carryComponent.SetIsBeingCarriedBy(owner);

            character.SetGridTileLocation(owner.gridTileLocation);
            character.SetCurrentStructureLocation(owner.currentStructure);

            if (!character.marker) {
                character.CreateMarker();
            }
            
            character.marker.transform.SetParent(owner.marker.visualsParent);
            character.marker.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            character.marker.visualsParent.eulerAngles = Vector3.zero;
            character.marker.transform.eulerAngles = Vector3.zero;
            // character.marker.SetNameState(false);

            //We added isFromSave checker, because if the carrying happens because we loaded the game from save data, infecting should not happen
            if (!isFromSave) {
                if (!owner.traitContainer.HasTrait("Plagued") && character.traitContainer.HasTrait("Plagued")) {
                    Traits.Plagued targetPlagued = character.traitContainer.GetTraitOrStatus<Traits.Plagued>("Plagued");
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
                            //     Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "contracted_plague");
                            //     log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                            //     log.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                            //     log.AddLogToInvolvedObjects();
                            // }
                        }
                        Debug.Log(GameManager.Instance.TodayLogString() + plaguedSummary);
                    }
                }
            }
            return true;
        }
        return false;
    }
    public void UncarryPOI(IPointOfInterest poi, bool addToLocation = true, LocationGridTile dropLocation = null) {
        if (IsPOICarried(poi)) {
            if (poi is Character) {
                RemoveCharacter(poi as Character, dropLocation);
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
                    LocationGridTile chosenTile = owner.gridTileLocation.GetFirstNoObjectNeighbor();
                    if (chosenTile != null) {
                        owner.gridTileLocation.structure.AddPOI(tileObject, chosenTile);
                    } else {
                        //If neighbours of the grid tile already have objects, get the nearest tile with no object
                        chosenTile = owner.gridTileLocation.GetFirstNearestTileFromThisWithNoObject(new List<LocationGridTile>());
                        owner.gridTileLocation.structure.AddPOI(tileObject, chosenTile);
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
    private void RemoveCharacter(Character character, LocationGridTile dropLocation) {
        if (owner == character) {
            return;
        }
        if(character == null) {
            //Cannot remove a null character
            return;
        }
        //LocationGridTile gridTile = owner.gridTileLocation.GetNearestUnoccupiedTileFromThis();
        //owner.specificLocation.AddCharacterToLocation(character);
        carriedPOI = null;
        character.carryComponent.SetIsBeingCarriedBy(null);
        
        //TODO: Find out why characters marker can be null while it is being carried! (https://trello.com/c/ZKPLZXjx/2485-nullreference-removecharacter)
        if (character.marker != null) {
            if (dropLocation == null) {
                character.marker.PlaceMarkerAt(owner.gridTileLocation);
                //if (owner.gridTileLocation.isOccupied) {
                //    LocationGridTile chosenTile = owner.gridTileLocation.GetRandomUnoccupiedNeighbor();
                //    if (chosenTile != null) {
                //        character.marker.PlaceMarkerAt(chosenTile);
                //    } else {
                //        Debug.LogWarning(
                //            $"{GameManager.Instance.TodayLogString()}{character.name} is being dropped by {owner.name} but there is no unoccupied neighbor tile including the tile he/she is standing on. Default behavior is to drop character on the tile he/she is standing on regardless if it is unoccupied or not.");
                //        character.marker.PlaceMarkerAt(owner.gridTileLocation);
                //    }
                //} else {
                //    character.marker.PlaceMarkerAt(owner.gridTileLocation);
                //}
            } else {
                character.marker.PlaceMarkerAt(dropLocation);
            }
            character.marker.transform.eulerAngles = Vector3.zero;
        }
        // character.marker.SetNameState(true);
        // Messenger.Broadcast(Signals.CHARACTER_LEFT_PARTY, character, this);
    }
    public bool IsPOICarried(IPointOfInterest poi) {
        return carriedPOI != null && carriedPOI == poi;
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
    public void SetJustGotCarriedBy(Character character) {
        justGotCarriedBy = character;
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataCarryComponent data) {
        if (!string.IsNullOrEmpty(data.isBeingCarriedBy)) {
            isBeingCarriedBy = CharacterManager.Instance.GetCharacterByPersistentID(data.isBeingCarriedBy);
        }
        if (!string.IsNullOrEmpty(data.justGotCarriedBy)) {
            justGotCarriedBy = CharacterManager.Instance.GetCharacterByPersistentID(data.justGotCarriedBy);
        }
    }
    public void LoadCarryReference(SaveDataCarryComponent data) {
        if (!string.IsNullOrEmpty(data.carriedPOI)) {
            IPointOfInterest poi = null;
            if (data.carriedPOIType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                poi = CharacterManager.Instance.GetCharacterByPersistentID(data.carriedPOI);
            } else if (data.carriedPOIType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                poi = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(data.carriedPOI);
            }
            //carriedPOI = poi;
            CarryPOI(poi, isFromSave: true);
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataCarryComponent : SaveData<CarryComponent> {
    public string carriedPOI;
    public POINT_OF_INTEREST_TYPE carriedPOIType;
    public string isBeingCarriedBy;
    public string justGotCarriedBy;

    #region Overrides
    public override void Save(CarryComponent data) {
        if(data.carriedPOI != null) {
            carriedPOI = data.carriedPOI.persistentID;
            carriedPOIType = data.carriedPOI.poiType;
            if(data.carriedPOI is Character character) {
                SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(character);
            } else if (data.carriedPOI is TileObject tileObject) {
                SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(tileObject);
            }
        }
        if (data.isBeingCarriedBy != null) {
            isBeingCarriedBy = data.isBeingCarriedBy.persistentID;
        }
        if (data.justGotCarriedBy != null) {
            justGotCarriedBy = data.justGotCarriedBy.persistentID;
        }
    }

    public override CarryComponent Load() {
        CarryComponent component = new CarryComponent(this);
        return component;
    }
    #endregion
}