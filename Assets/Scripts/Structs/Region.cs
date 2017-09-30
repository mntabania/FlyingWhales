﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Region {
    private int id;
    private HexTile _centerOfMass;
    private List<HexTile> _tilesInRegion; //This also includes the center of mass
    private Color regionColor;
    private List<Region> _adjacentRegions;
    private City _occupant;

    private Color defaultBorderColor = new Color(94f / 255f, 94f / 255f, 94f / 255f, 255f / 255f);

    //Resources
    private RESOURCE _specialResource;
    private HexTile _tileWithSpecialResource;
    private Dictionary<RACE, int> _naturalResourceLevel;
    private int _cityLevelCap;

    //Population
    private int _populationGrowth;

    private List<HexTile> _outerTiles;
    private List<SpriteRenderer> regionBorderLines;

    #region getters/sertters
    internal HexTile centerOfMass {
        get { return _centerOfMass; }
    }
    internal List<HexTile> tilesInRegion {
        get { return _tilesInRegion; }
    }
    internal List<Region> adjacentRegions {
        get { return _adjacentRegions; }
    }
    internal City occupant {
        get { return _occupant; }
    }
    internal RESOURCE specialResource {
        get { return _specialResource; }
    }
    internal HexTile tileWithSpecialResource {
        get { return _tileWithSpecialResource; }
    }
    internal Dictionary<RACE, int> naturalResourceLevel {
        get { return _naturalResourceLevel; }
    }
    internal int cityLevelCap {
        get { return _cityLevelCap; }
    }
    internal int populationGrowth {
        get { return _populationGrowth; }
    }
    #endregion

    public Region(HexTile centerOfMass) {
        id = Utilities.SetID<Region>(this);
        SetCenterOfMass(centerOfMass);
        _tilesInRegion = new List<HexTile>();
        AddTile(_centerOfMass);
        regionColor = Random.ColorHSV(0f, 1f, 0f, 1f, 0f, 1f);
        SetSpecialResource(RESOURCE.NONE);

        //Generate population growth
        int[] possiblePopulationGrowths = new int[] { 1, 3, 8, 5 };
        _populationGrowth = possiblePopulationGrowths[Random.Range(0, possiblePopulationGrowths.Length)];
    }

    #region Center Of Mass Functions
    internal void ReComputeCenterOfMass() {
        int maxXCoordinate = _tilesInRegion.Max(x => x.xCoordinate);
        int minXCoordinate = _tilesInRegion.Min(x => x.xCoordinate);
        int maxYCoordinate = _tilesInRegion.Max(x => x.yCoordinate);
        int minYCoordinate = _tilesInRegion.Min(x => x.yCoordinate);

        int midPointX = (minXCoordinate + maxXCoordinate) / 2;
        int midPointY = (minYCoordinate + maxYCoordinate) / 2;

        SetCenterOfMass(GridMap.Instance.map[midPointX, midPointY]);
    }
    internal void RevalidateCenterOfMass() {
        if (_centerOfMass.elevationType != ELEVATION.PLAIN || _centerOfMass.specialResource != RESOURCE.NONE) {
            SetCenterOfMass(_tilesInRegion.Where(x => x.elevationType == ELEVATION.PLAIN && x.specialResource == RESOURCE.NONE)
                .OrderBy(x => x.GetDistanceTo(_centerOfMass)).FirstOrDefault());
            if (_centerOfMass == null) {
                throw new System.Exception("center of mass is null!");
            }
        }
    }
    internal void SetCenterOfMass(HexTile newCenter) {
        if(_centerOfMass != null) {
            _centerOfMass.isHabitable = false;
        }
        _centerOfMass = newCenter;
        _centerOfMass.isHabitable = true;
    }
    #endregion

    #region Adjacency Functions
    /*
     * <summary>
     * Check For Adjacent regions, this will populate the
     * _outerTiles and _adjacentRegions Lists. This is only called at the
     * start of the game, after all the regions have been determined. This will
     * also populate regionBorderLines.
     * </summary>
     * */
    internal void CheckForAdjacency() {
        _outerTiles = new List<HexTile>();
        _adjacentRegions = new List<Region>();
        regionBorderLines = new List<SpriteRenderer>();
        for (int i = 0; i < _tilesInRegion.Count; i++) {
            HexTile currTile = _tilesInRegion[i];
            for (int j = 0; j < currTile.AllNeighbours.Count; j++) {
                HexTile currNeighbour = currTile.AllNeighbours[j];
                if (currNeighbour.region != currTile.region) {
                    //Load Border For currTile
                    HEXTILE_DIRECTION borderTileToActivate = currTile.GetNeighbourDirection(currNeighbour);
                    SpriteRenderer border = currTile.ActivateBorder(borderTileToActivate);
                    if (!regionBorderLines.Contains(border)) {
                        regionBorderLines.Add(border);
                    }

                    if (!_outerTiles.Contains(currTile)) {
                        //currTile has a neighbour that is part of a different region, this means it is an outer tile.
                        _outerTiles.Add(currTile);
                    }
                    if (!_adjacentRegions.Contains(currNeighbour.region)) {
                        _adjacentRegions.Add(currNeighbour.region);
                    }
                }
            } 
        }
    }
    internal bool IsAdjacentToKingdom(Kingdom kingdom) {
        for (int i = 0; i < _adjacentRegions.Count; i++) {
            Region currRegion = _adjacentRegions[i];
            if(currRegion.occupant != null && currRegion.occupant.kingdom == kingdom) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Tile Functions
    internal void AddTile(HexTile tile) {
        _tilesInRegion.Add(tile);
        tile.SetRegion(this);
    }
    internal void ResetTilesInRegion() {
        for (int i = 0; i < _tilesInRegion.Count; i++) {
            _tilesInRegion[i].SetRegion(null);
        }
        _tilesInRegion.Clear();
    }
    internal void SetOccupant(City occupant) {
        _occupant = occupant;
        _occupant.kingdom.SetFogOfWarStateForRegion(this, FOG_OF_WAR_STATE.VISIBLE);
        _cityLevelCap = _naturalResourceLevel[occupant.kingdom.race];
        SetAdjacentRegionsAsVisibleForOccupant();
        Color solidKingdomColor = _occupant.kingdom.kingdomColor;
        solidKingdomColor.a = 255f / 255f;
        ReColorBorderTiles(solidKingdomColor);
        if(_specialResource != RESOURCE.NONE) {
            _tileWithSpecialResource.Occupy(occupant);
            CreateStructureOnSpecialResourceTile();
        }
    }
    internal void RemoveOccupant() {
        City previousOccupant = _occupant;
        _occupant = null;
        //Check if this region has adjacent regions that has the same occupant as this one, if so set region as visible
        if (IsAdjacentToKingdom(previousOccupant.kingdom)) {
            previousOccupant.kingdom.SetFogOfWarStateForRegion(this, FOG_OF_WAR_STATE.VISIBLE);
        } else {
            previousOccupant.kingdom.SetFogOfWarStateForRegion(this, FOG_OF_WAR_STATE.SEEN);
        }

        //Change fog of war of region for discovered kingdoms
        for (int i = 0; i < previousOccupant.kingdom.discoveredKingdoms.Count; i++) {
            Kingdom otherKingdom = previousOccupant.kingdom.discoveredKingdoms[i];
            if (IsAdjacentToKingdom(otherKingdom)) {
                otherKingdom.SetFogOfWarStateForRegion(this, FOG_OF_WAR_STATE.VISIBLE);
            } else {
                otherKingdom.SetFogOfWarStateForRegion(this, FOG_OF_WAR_STATE.HIDDEN);
            }
        }

        //Check adjacent regions
        for (int i = 0; i < adjacentRegions.Count; i++) {
            Region adjacentRegion = adjacentRegions[i];
            if (adjacentRegion.IsAdjacentToKingdom(previousOccupant.kingdom)) {
                previousOccupant.kingdom.SetFogOfWarStateForRegion(adjacentRegion, FOG_OF_WAR_STATE.VISIBLE);
                continue;
            }

            if (adjacentRegion.occupant == null) {
                previousOccupant.kingdom.SetFogOfWarStateForRegion(adjacentRegion, FOG_OF_WAR_STATE.HIDDEN);
            } else {
                Kingdom occupantOfAdjacentRegion = adjacentRegion.occupant.kingdom;
                if (previousOccupant.kingdom.discoveredKingdoms.Contains(occupantOfAdjacentRegion)) {
                    previousOccupant.kingdom.SetFogOfWarStateForRegion(adjacentRegion, FOG_OF_WAR_STATE.SEEN);
                } else if(occupantOfAdjacentRegion == previousOccupant.kingdom) {
                    previousOccupant.kingdom.SetFogOfWarStateForRegion(adjacentRegion, FOG_OF_WAR_STATE.VISIBLE);
                } else {
                    previousOccupant.kingdom.SetFogOfWarStateForRegion(adjacentRegion, FOG_OF_WAR_STATE.HIDDEN);
                }
            }
        }
        
        
        ReColorBorderTiles(defaultBorderColor);
        
        if (_specialResource != RESOURCE.NONE) {
            _tileWithSpecialResource.Unoccupy();
        }
    }
    private void SetAdjacentRegionsAsVisibleForOccupant() {
        for (int i = 0; i < _adjacentRegions.Count; i++) {
            Region currRegion = _adjacentRegions[i];
            if(currRegion._occupant == null || currRegion._occupant.kingdom != _occupant.kingdom) {
                _occupant.kingdom.SetFogOfWarStateForRegion(currRegion, FOG_OF_WAR_STATE.VISIBLE);
            }
        }
    }
    private void ReColorBorderTiles(Color color) {
        for (int i = 0; i < regionBorderLines.Count; i++) {
            regionBorderLines[i].color = color;
        }
    }
    /*
     * <summary>
     * Create a structure on the tile with special resource.
     * This is for visuals only, this does not increase the city's(occupant) level.
     * </sumary>
     * */
    private void CreateStructureOnSpecialResourceTile() {
        if(_specialResource != RESOURCE.NONE) {
            tileWithSpecialResource
                .CreateStructureOnTile(Utilities.GetStructureTypeForResource(_occupant.kingdom.race, _specialResource));
        }
    }
    #endregion

    #region Resource Functions
    internal void SetSpecialResource(RESOURCE resource) {
        _specialResource = resource;
        if(_specialResource != RESOURCE.NONE) {
            List<HexTile> elligibleTiles = _tilesInRegion.Where(x => x.elevationType == ELEVATION.PLAIN && x != centerOfMass).ToList();
            _tileWithSpecialResource = elligibleTiles[Random.Range(0, elligibleTiles.Count)];
            _tileWithSpecialResource.AssignSpecialResource(_specialResource);
        }
    }
    /*
     * <summary>
     * Compute the natural resource level for each race.
     * NOTE: Only Call this once special resource is determined, to compute
     * the correct value.
     * </summary>
     * */
    internal void ComputeNaturalResourceLevel() {
        int humanTilePoints = 0;
        int elvenTilePoints = 0;
        _naturalResourceLevel = new Dictionary<RACE, int>() {
            {RACE.HUMANS, 0},
            {RACE.ELVES, 0},
            {RACE.MINGONS, 0},
            {RACE.CROMADS, 0}
        };
        for (int i = 0; i < _tilesInRegion.Count; i++) {
            HexTile currTile = _tilesInRegion[i];
            if(currTile.elevationType == ELEVATION.MOUNTAIN) {
                //if current tile is mountain continue with other additions
                elvenTilePoints += 1;
            } else if (currTile.elevationType == ELEVATION.WATER) {
                //if current tile is water disregard any other additions
                humanTilePoints += 3;
                elvenTilePoints += 3;
                continue;
            }
            switch (currTile.biomeType) {
                case BIOMES.SNOW:
                    humanTilePoints += 1;
                    elvenTilePoints += 1;
                    break;
                case BIOMES.TUNDRA:
                    humanTilePoints += 2;
                    elvenTilePoints += 2;
                    break;
                case BIOMES.DESERT:
                    humanTilePoints += 3;
                    elvenTilePoints += 1;
                    break;
                case BIOMES.GRASSLAND:
                    humanTilePoints += 6;
                    elvenTilePoints += 3;
                    break;
                case BIOMES.WOODLAND:
                    humanTilePoints += 4;
                    elvenTilePoints += 5;
                    break;
                case BIOMES.FOREST:
                    humanTilePoints += 2;
                    elvenTilePoints += 6;
                    break;
                default:
                    break;
            }
        }

        int increaseFromSpecialResource = 0;
        if(_specialResource != RESOURCE.NONE) {
            increaseFromSpecialResource = 3;
        }

        _naturalResourceLevel[RACE.HUMANS] = (humanTilePoints / 10) + increaseFromSpecialResource;
        _naturalResourceLevel[RACE.ELVES] = (elvenTilePoints / 10) + increaseFromSpecialResource;

        //_centerOfMass.SetTileText(specialResource.ToString() + "\n" +
        //    naturalResourceLevel[RACE.HUMANS].ToString() + "\n" +
        //    naturalResourceLevel[RACE.ELVES].ToString(), 5, Color.white, "Minimap");
    }
    internal void ShowNaturalResourceLevelForRace(RACE race) {
        int maxXCoordinate = _tilesInRegion.Max(x => x.xCoordinate);
        int minXCoordinate = _tilesInRegion.Min(x => x.xCoordinate);
        int maxYCoordinate = _tilesInRegion.Max(x => x.yCoordinate);
        int minYCoordinate = _tilesInRegion.Min(x => x.yCoordinate);

        int midPointX = (minXCoordinate + maxXCoordinate) / 2;
        int midPointY = (minYCoordinate + maxYCoordinate) / 2;

        HexTile midPoint = GridMap.Instance.map[midPointX, midPointY];

        string text = "0";
        if(_occupant != null) {
            text = _occupant.ownedTiles.Count.ToString();
        }
        text += "/" + naturalResourceLevel[race].ToString();
        midPoint.SetTileText(text, 6, Color.white, "Minimap");
    }
    #endregion

    #region Kingdom Discovery Functions
    internal void CheckForDiscoveredKingdoms() {
        List<Region> adjacentRegionsOfOtherRegions = new List<Region>();
        List<Kingdom> adjacentKingdoms = new List<Kingdom>();

        for (int i = 0; i < _adjacentRegions.Count; i++) {
            Region adjacentRegion = _adjacentRegions[i];
            if(adjacentRegion.occupant != null) {
                Kingdom otherKingdom = adjacentRegion.occupant.kingdom;
                if (otherKingdom != occupant.kingdom) {
                    if (!adjacentKingdoms.Contains(otherKingdom)) {
                        adjacentKingdoms.Add(otherKingdom);
                    }
                    if (!_occupant.kingdom.discoveredKingdoms.Contains(otherKingdom)) {
                        KingdomManager.Instance.DiscoverKingdom(_occupant.kingdom, otherKingdom);
                    }
                    _occupant.kingdom.GetRelationshipWithKingdom(otherKingdom).ChangeAdjacency(true);
                }
                
                for (int j = 0; j < adjacentRegion.adjacentRegions.Count; j++) {
                    Region otherAdjacentRegion = adjacentRegion.adjacentRegions[j];
                    if (!_adjacentRegions.Contains(otherAdjacentRegion) && !adjacentRegionsOfOtherRegions.Contains(otherAdjacentRegion) && otherAdjacentRegion != this) {
                        adjacentRegionsOfOtherRegions.Add(otherAdjacentRegion);
                    }
                }
            }
        }

        ////When you discover another kingdom via adjacency, you discover all other kingdoms that it has discovered
        //for (int i = 0; i < adjacentKingdoms.Count; i++) {
        //    Kingdom otherKingdom = adjacentKingdoms[i];
        //    List<Kingdom> discoveredKingdomsOfOtherKingdom = otherKingdom.adjacentKingdoms;
        //    for (int j = 0; j < discoveredKingdomsOfOtherKingdom.Count; j++) {
        //        Kingdom kingdomToDiscover = discoveredKingdomsOfOtherKingdom[j];
        //        if (kingdomToDiscover != _occupant.kingdom && !_occupant.kingdom.discoveredKingdoms.Contains(kingdomToDiscover)) {
        //            KingdomManager.Instance.DiscoverKingdom(_occupant.kingdom, kingdomToDiscover);
        //        }
        //    }
        //}

        //When you discover another kingdom via adjacency, you also discover all other regions it is adjacent to.
        for (int i = 0; i < adjacentRegionsOfOtherRegions.Count; i++) {
            Region otherAdjacentRegion = adjacentRegionsOfOtherRegions[i];
            if (otherAdjacentRegion.occupant != null) {
                Kingdom adjacentKingdomOfOtherKingdom = otherAdjacentRegion.occupant.kingdom;
                if (adjacentKingdomOfOtherKingdom != _occupant.kingdom && !_occupant.kingdom.discoveredKingdoms.Contains(adjacentKingdomOfOtherKingdom)) {
                    KingdomManager.Instance.DiscoverKingdom(_occupant.kingdom, adjacentKingdomOfOtherKingdom);
                }
            }
        }

        //When you discover another kingdom via adjacency, you also discover all other kingdoms it is adjacent to.
        for (int i = 0; i < adjacentKingdoms.Count; i++) {
            Kingdom otherKingdom = adjacentKingdoms[i];
            List<Kingdom> adjacentKingdomsOfOtherKingdom = otherKingdom.adjacentKingdoms;
            for (int j = 0; j < adjacentKingdomsOfOtherKingdom.Count; j++) {
                Kingdom kingdomToDiscover = adjacentKingdomsOfOtherKingdom[j];
                if (kingdomToDiscover != _occupant.kingdom && !_occupant.kingdom.discoveredKingdoms.Contains(kingdomToDiscover)) {
                    KingdomManager.Instance.DiscoverKingdom(_occupant.kingdom, kingdomToDiscover);
                }
            }
        }

        //When a kingdom expands kingdoms it is adjacent to should discover each other
        if (_occupant.kingdom.adjacentKingdoms.Count > 1) {
            for (int i = 0; i < _occupant.kingdom.adjacentKingdoms.Count; i++) {
                Kingdom currentKingdom = _occupant.kingdom.adjacentKingdoms[i];
                for (int j = 0; j < _occupant.kingdom.adjacentKingdoms.Count; j++) {
                    Kingdom otherKingdom = _occupant.kingdom.adjacentKingdoms[j];
                    if (currentKingdom.id != otherKingdom.id && !currentKingdom.discoveredKingdoms.Contains(otherKingdom)) {
                        KingdomManager.Instance.DiscoverKingdom(currentKingdom, otherKingdom);
                    }
                }
            }
        }
    }
    #endregion



}
