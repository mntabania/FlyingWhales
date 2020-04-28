using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class Biomes : MonoBehaviour {
	public static Biomes Instance;

    [Header("Biome Generation Settings")]
	public float initialTemperature;
	public float initialTemperature2;
	public float intervalTemperature;
	public float temperature;
	public int[] hexInterval;
	public float[] temperatureInterval;

	[Space(10)]
    [Header("Biome Sprites")]
    [SerializeField] private Sprite[] grasslandTiles;
    [SerializeField] private Sprite[] grasslandCorruptedTiles;
    [SerializeField] private Sprite[] forestTiles;
    [SerializeField] private Sprite[] forestCorruptedTiles;
    [SerializeField] private Sprite[] desertTiles;
    [SerializeField] private Sprite[] desertCorruptedTiles;
    [SerializeField] private Sprite[] tundraTiles;
    [SerializeField] private Sprite[] tundraCorruptedTiles;
    [SerializeField] private Sprite[] waterTiles;
	[SerializeField] private Sprite[] snowTiles;
    [SerializeField] private Sprite[] snowCorruptedTiles;
    [SerializeField] private Sprite[] _bareTiles;
    [SerializeField] private Sprite[] _ancientRuinTiles;
    [SerializeField] private Sprite[] ancientRuinCorruptedTiles;

    [Space(10)]
    [Header("Mountain Sprites")]
    [SerializeField] private Sprite[] grasslandMountains;
    [SerializeField] private Sprite[] grasslandMountainsCorrupted;
    [SerializeField] private Sprite[] forestMountains;
    [SerializeField] private Sprite[] forestMountainsCorrupted;
    [SerializeField] private Sprite[] desertMountains;
    [SerializeField] private Sprite[] desertMountainsCorrupted;
    [SerializeField] private Sprite[] snowMountains;
    [SerializeField] private Sprite[] snowMountainsCorrupted;
    [SerializeField] private Sprite[] tundraMounains;
    [SerializeField] private Sprite[] tundraMountainsCorrupted;

    [Space(10)]
    [Header("Tree Sprites")]
    [SerializeField] private Sprite[] grasslandTrees;
    [SerializeField] private Sprite[] grasslandTreesCorrupted;
    [SerializeField] private Sprite[] forestTrees;
    [SerializeField] private Sprite[] forestTreesCorrupted;
    [SerializeField] private Sprite[] desertTrees;
    [SerializeField] private Sprite[] desertTreesCorrupted;
    [SerializeField] private Sprite[] snowTrees;
    [SerializeField] private Sprite[] snowTreesCorrupted;
    [SerializeField] private Sprite[] tundraTrees;
    [SerializeField] private Sprite[] tundraTreesCorrupted;

    [Space(10)]
    [Header("Animations")]
    [SerializeField] private BiomeSpriteAnimationDictionary biomeSpriteAnimations;

    #region getters/setters
    public Sprite[] bareTiles{
		get{ return this._bareTiles; }
	}
    public Sprite[] ancienctRuinTiles {
        get { return this._ancientRuinTiles; }
    }
    #endregion

    void Awake(){
		Instance = this;
	}
    public IEnumerator GenerateBiome(List<HexTile> tiles) {
        int batchCount = 0;
        for(int i = 0; i < tiles.Count; i++){
            HexTile currentHexTile = tiles[i];
            BIOMES biomeForTile = GetBiomeSimple(currentHexTile.gameObject);
            SetBiomeForTile(biomeForTile, currentHexTile);
            
            batchCount++;
            if (batchCount == MapGenerationData.WorldMapTileGenerationBatches) {
                batchCount = 0;
                yield return null;    
            }
        }
    }
    internal void SetBiomeForTile(BIOMES biomeForTile, HexTile currentHexTile) {
        currentHexTile.SetBiome(biomeForTile);
    }
    public void UpdateTileVisuals(List<HexTile> allTiles) {
        for (int i = 0; i < allTiles.Count; i++) {
            HexTile currentHexTile = allTiles[i];
            UpdateTileVisuals(currentHexTile);
        }
    }
    public void UpdateTileVisuals(HexTile currentHexTile) {
        int sortingOrder = 0;
        int mapHeight = (int)GridMap.Instance.height - 1;
        int yCoordinate = currentHexTile.yCoordinate - 2;
        if (GridMap.Instance.outerGridList.Contains(currentHexTile)) {
            mapHeight -= GridMap.Instance._borderThickness * 2;
            if (currentHexTile.yCoordinate < GridMap.Instance.height && currentHexTile.yCoordinate >= 0) {
                sortingOrder = GridMap.Instance.map[0, currentHexTile.yCoordinate].spriteRenderer.sortingOrder;
            } else if (currentHexTile.yCoordinate < 0) {
                int originSortingOrder = GridMap.Instance.map[0, 0].spriteRenderer.sortingOrder;
                int differenceFromOrigin = Mathf.Abs(currentHexTile.yCoordinate);
                sortingOrder = originSortingOrder + (differenceFromOrigin * EditableValuesManager.Instance.sortingOrdersInBetweenHexTileRows);
            } else {
                sortingOrder = (mapHeight -  yCoordinate) * EditableValuesManager.Instance.sortingOrdersInBetweenHexTileRows;
            }
        } else {
            sortingOrder = (mapHeight -  yCoordinate) * EditableValuesManager.Instance.sortingOrdersInBetweenHexTileRows;
        }

        if (PlayerManager.Instance.player != null && currentHexTile.settlementOnTile != null) {
            Faction factionOwner = currentHexTile.settlementOnTile.owner;
            if (factionOwner != null && factionOwner.isPlayerFaction) {
                return;
            }
        }
        LoadBeachVisuals(currentHexTile);
        UpdateTileSprite(currentHexTile, sortingOrder);
    }
    public void UpdateTileSprite(HexTile tile, int sortingOrder) {
        if (tile.isCorrupted) {
            CorruptTileVisuals(tile);
        } else {
            if (tile.elevationType == ELEVATION.PLAIN) {
                LoadPlainTileVisuals(tile, sortingOrder);
            } else if (tile.elevationType == ELEVATION.MOUNTAIN) {
                LoadMountainTileVisuals(tile, sortingOrder);
            } else if (tile.elevationType == ELEVATION.TREES) {
                LoadTreeTileVisuals(tile, sortingOrder);
            } else {
                //For Water
                LoadWaterTileVisuals(tile, sortingOrder);
            }
        }
    }
    private void CorruptTileVisuals(HexTile tile) {
        if (tile.elevationType == ELEVATION.PLAIN) {
            LoadCorruptedPlainTileVisuals(tile);
        }else if (tile.elevationType == ELEVATION.MOUNTAIN) {
            LoadCorruptedMountainTileVisuals(tile);
        }else if (tile.elevationType == ELEVATION.TREES) {
            LoadCorruptedTreeTileVisuals(tile);
        }
        if (tile.landmarkOnTile != null && tile.landmarkOnTile.specificLandmarkType != LANDMARK_TYPE.THE_PORTAL) {
            LoadCorruptedStructureVisuals(tile);
        }
    }
    private void LoadPlainTileVisuals(HexTile tile, int sortingOrder) {
        switch (tile.biomeType) {
            case BIOMES.SNOW:
                Sprite snowSpriteToUse = snowTiles[UnityEngine.Random.Range(0, snowTiles.Length)];
                tile.SetBaseSprite(snowSpriteToUse);
                break;
            case BIOMES.TUNDRA:
                Sprite tundraSpriteToUse = tundraTiles[UnityEngine.Random.Range(0, tundraTiles.Length)];
                tile.SetBaseSprite(tundraSpriteToUse);
                break;
            case BIOMES.DESERT:
                Sprite desertSpriteToUse = desertTiles[UnityEngine.Random.Range(0, desertTiles.Length)];
                tile.SetBaseSprite(desertSpriteToUse);
                break;
            case BIOMES.GRASSLAND:
                Sprite grasslandSpriteToUse = grasslandTiles[UnityEngine.Random.Range(0, grasslandTiles.Length)];
                tile.SetBaseSprite(grasslandSpriteToUse);
                break;
            case BIOMES.FOREST:
                Sprite forestSpriteToUse = forestTiles[UnityEngine.Random.Range(0, forestTiles.Length)];
                tile.SetBaseSprite(forestSpriteToUse);
                break;
            case BIOMES.ANCIENT_RUIN:
                Sprite ruinSpriteToUse = ancienctRuinTiles[UnityEngine.Random.Range(0, ancienctRuinTiles.Length)];
                tile.SetBaseSprite(ruinSpriteToUse);
                break;
        }
        tile.SetSortingOrder(sortingOrder);
    }
    private void LoadCorruptedPlainTileVisuals(HexTile tile) {
        int index = 0;
        Sprite[] choices = null;
        switch (tile.biomeType) {
            case BIOMES.SNOW:
                index = Array.IndexOf(snowTiles, tile.spriteRenderer.sprite);
                choices = snowCorruptedTiles;
                break;
            case BIOMES.TUNDRA:
                index = Array.IndexOf(tundraTiles, tile.spriteRenderer.sprite);
                choices = tundraCorruptedTiles;
                break;
            case BIOMES.DESERT:
                index = Array.IndexOf(desertTiles, tile.spriteRenderer.sprite);
                choices = desertCorruptedTiles;
                break;
            case BIOMES.GRASSLAND:
                index = Array.IndexOf(grasslandTiles, tile.spriteRenderer.sprite);
                choices = grasslandCorruptedTiles;
                break;
            case BIOMES.FOREST:
                index = Array.IndexOf(forestTiles, tile.spriteRenderer.sprite);
                choices = forestCorruptedTiles;
                break;
            case BIOMES.ANCIENT_RUIN:
                index = Array.IndexOf(ancienctRuinTiles, tile.spriteRenderer.sprite);
                choices = ancientRuinCorruptedTiles;
                break;
        }
        if (index != -1) {
            tile.SetBaseSprite(choices[index]);
        }
    }
    private void LoadMountainTileVisuals(HexTile tile, int sortingOrder) {
        switch (tile.biomeType) {
            case BIOMES.SNOW:
                Sprite snowSpriteToUse = snowMountains[UnityEngine.Random.Range(0, snowMountains.Length)];
                tile.SetBaseSprite(snowSpriteToUse);
                break;
            case BIOMES.TUNDRA:
                Sprite tundraSpriteToUse = tundraMounains[UnityEngine.Random.Range(0, tundraMounains.Length)];
                tile.SetBaseSprite(tundraSpriteToUse);
                break;
            case BIOMES.DESERT:
                Sprite desertSpriteToUse = desertMountains[UnityEngine.Random.Range(0, desertMountains.Length)];
                tile.SetBaseSprite(desertSpriteToUse);
                break;
            case BIOMES.GRASSLAND:
                Sprite grasslandSpriteToUse = grasslandMountains[UnityEngine.Random.Range(0, grasslandMountains.Length)];
                tile.SetBaseSprite(grasslandSpriteToUse);
                break;
            case BIOMES.FOREST:
                Sprite forestSpriteToUse = forestMountains[UnityEngine.Random.Range(0, forestMountains.Length)];
                tile.SetBaseSprite(forestSpriteToUse);
                break;
            case BIOMES.ANCIENT_RUIN:
                Sprite ruinSpriteToUse = ancienctRuinTiles[UnityEngine.Random.Range(0, ancienctRuinTiles.Length)];
                tile.SetBaseSprite(ruinSpriteToUse);
                break;
        }
        tile.SetSortingOrder(sortingOrder);
    }
    private void LoadCorruptedMountainTileVisuals(HexTile tile) {
        int index = 0;
        switch (tile.biomeType) {
            case BIOMES.SNOW:
            index = Array.IndexOf(snowMountains, tile.spriteRenderer.sprite);
            tile.SetBaseSprite(snowMountainsCorrupted[index]);
            break;
            case BIOMES.TUNDRA:
            index = Array.IndexOf(tundraMounains, tile.spriteRenderer.sprite);
            tile.SetBaseSprite(tundraMountainsCorrupted[index]);
            break;
            case BIOMES.DESERT:
            index = Array.IndexOf(desertMountains, tile.spriteRenderer.sprite);
            tile.SetBaseSprite(desertMountainsCorrupted[index]);
            break;
            case BIOMES.GRASSLAND:
            index = Array.IndexOf(grasslandMountains, tile.spriteRenderer.sprite);
            tile.SetBaseSprite(grasslandMountainsCorrupted[index]);
            break;
            case BIOMES.FOREST:
            index = Array.IndexOf(forestMountains, tile.spriteRenderer.sprite);
            tile.SetBaseSprite(forestMountainsCorrupted[index]);
            break;
            case BIOMES.ANCIENT_RUIN:
            index = Array.IndexOf(ancienctRuinTiles, tile.spriteRenderer.sprite);
            tile.SetBaseSprite(ancientRuinCorruptedTiles[index]);
            break;
        }
    }
    private void LoadTreeTileVisuals(HexTile tile, int sortingOrder) {
        switch (tile.biomeType) {
            case BIOMES.SNOW:
                Sprite snowSpriteToUse = snowTrees[UnityEngine.Random.Range(0, snowTrees.Length)];
                tile.SetBaseSprite(snowSpriteToUse);
                break;
            case BIOMES.TUNDRA:
                Sprite tundraSpriteToUse = tundraTrees[UnityEngine.Random.Range(0, tundraTrees.Length)];
                tile.SetBaseSprite(tundraSpriteToUse);
                break;
            case BIOMES.DESERT:
                Sprite desertSpriteToUse = desertTrees[UnityEngine.Random.Range(0, desertTrees.Length)];
                tile.SetBaseSprite(desertSpriteToUse);
                break;
            case BIOMES.GRASSLAND:
                Sprite grasslandSpriteToUse = grasslandTrees[UnityEngine.Random.Range(0, grasslandTrees.Length)];
                tile.SetBaseSprite(grasslandSpriteToUse);
                break;
            case BIOMES.FOREST:
                Sprite forestSpriteToUse = forestTrees[UnityEngine.Random.Range(0, forestTrees.Length)];
                tile.SetBaseSprite(forestSpriteToUse);
                break;
            case BIOMES.ANCIENT_RUIN:
                Sprite ruinSpriteToUse = ancienctRuinTiles[UnityEngine.Random.Range(0, ancienctRuinTiles.Length)];
                tile.SetBaseSprite(ruinSpriteToUse);
                break;
        }
        tile.SetSortingOrder(sortingOrder);
    }
    private void LoadCorruptedTreeTileVisuals(HexTile tile) {
        int index = 0;
        switch (tile.biomeType) {
            case BIOMES.SNOW:
            index = Array.IndexOf(snowTrees, tile.baseSprite);
            tile.SetBaseSprite(snowTreesCorrupted[index]);
            break;
            case BIOMES.TUNDRA:
            index = Array.IndexOf(tundraTrees, tile.baseSprite);
            tile.SetBaseSprite(tundraTreesCorrupted[index]);
            break;
            case BIOMES.DESERT:
            index = Array.IndexOf(desertTrees, tile.baseSprite);
            tile.SetBaseSprite(desertTreesCorrupted[index]);
            break;
            case BIOMES.GRASSLAND:
            index = Array.IndexOf(grasslandTrees, tile.baseSprite);
            tile.SetBaseSprite(grasslandTreesCorrupted[index]);
            break;
            case BIOMES.FOREST:
            index = Array.IndexOf(forestTrees, tile.baseSprite);
            tile.SetBaseSprite(forestTreesCorrupted[index]);
            break;
            case BIOMES.ANCIENT_RUIN:
            index = Array.IndexOf(ancienctRuinTiles, tile.baseSprite);
            tile.SetBaseSprite(ancientRuinCorruptedTiles[index]);
            break;
        }
    }
    private void LoadCorruptedStructureVisuals(HexTile tile) {
        if (tile.landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.CAVE) {
            tile.SetLandmarkTileSprite(LandmarkStructureSprite.Empty);
            Sprite[] choices = null;
            switch (tile.biomeType) {
                case BIOMES.SNOW:
                    choices = snowMountainsCorrupted;
                    break;
                case BIOMES.TUNDRA:
                    choices = tundraMountainsCorrupted;
                    break;
                case BIOMES.DESERT:
                    choices = desertMountainsCorrupted;
                    break;
                case BIOMES.GRASSLAND:
                    choices = grasslandMountainsCorrupted;
                    break;
                case BIOMES.FOREST:
                    choices = forestMountainsCorrupted;
                    break;
                case BIOMES.ANCIENT_RUIN:
                    choices = ancientRuinCorruptedTiles;
                    break;
            }
            tile.SetBaseSprite(choices[UnityEngine.Random.Range(0, choices.Length)]);
        } else {
            //LandmarkStructureSprite sprites = PlayerManager.Instance.playerAreaDefaultStructureSprites[UnityEngine.Random.Range(0, PlayerManager.Instance.playerAreaDefaultStructureSprites.Length)];
            tile.SetLandmarkTileSprite(LandmarkStructureSprite.Empty);
        }
    }
    private void LoadWaterTileVisuals(HexTile tile, int sortingOrder) {
        Sprite waterSpriteToUse = waterTiles[UnityEngine.Random.Range(0, waterTiles.Length)];
        //tile.spriteRenderer.sortingLayerName = "Water";
        tile.spriteRenderer.sprite = waterSpriteToUse;
        tile.DeactivateCenterPiece();
        tile.SetSortingOrder(sortingOrder);
    }
    private void LoadBeachVisuals(HexTile tile) {
        tile.LoadBeaches();
    }
	public IEnumerator GenerateElevation(List<HexTile> tiles, int mapWidth, int mapHeight) {
        float elevationFrequency = WorldConfigManager.Instance.isDemoWorld ? 15f : 8.93f;
        float moistureFrequency = 12.34f;
        float tempFrequency = 2.64f;

        float elevationRand = UnityEngine.Random.Range(500f,2000f);
        float moistureRand = UnityEngine.Random.Range(500f,2000f);
        float temperatureRand = UnityEngine.Random.Range(500f,2000f);

        string[] splittedNameEq = EquatorGenerator.Instance.listEquator[0].name.Split(new char[]{','});
        int equatorY = int.Parse (splittedNameEq [1]);

        int batchCount = 0;
        
        for(int i = 0; i < tiles.Count; i++){
            HexTile currTile = tiles[i];

            string[] splittedName = currTile.name.Split(new char[]{','});
            int[] xy = {int.Parse(splittedName[0]), int.Parse(splittedName[1])};

            float nx = ((float)xy[0]/mapWidth);
            float ny = ((float)xy[1]/mapHeight);

            float elevationNoise = Mathf.PerlinNoise((nx + elevationRand) * elevationFrequency, (ny + elevationRand) * elevationFrequency);
            ELEVATION elevationType = GetElevationType(elevationNoise);

            currTile.data.elevationNoise = elevationNoise;
            currTile.SetElevation (elevationType);
            currTile.data.moistureNoise = Mathf.PerlinNoise((nx + moistureRand) * moistureFrequency, (ny + moistureRand) * moistureFrequency);

            int distanceToEquator = Mathf.Abs (xy [1] - equatorY);
            float tempGradient = 1.23f / mapHeight;
            currTile.data.temperature = distanceToEquator * tempGradient;
            currTile.data.temperature += (Mathf.PerlinNoise((nx + temperatureRand) * tempFrequency, (ny + temperatureRand) * tempFrequency)) * 0.6f;

            batchCount++;
            if (batchCount == MapGenerationData.WorldMapTileGenerationBatches) {
                batchCount = 0;
                yield return null;    
            }
        }
    }
    private ELEVATION GetElevationType(float elevationNoise){
        //return ELEVATION.PLAIN;
        if (elevationNoise <= 0.20f) {
			return ELEVATION.WATER;
		} else if (elevationNoise > 0.20f && elevationNoise <= 0.39f) {
			return ELEVATION.TREES;
        } else if (elevationNoise > 0.39f && elevationNoise <= 0.6f) {
            return ELEVATION.PLAIN;
        } else { 
            return ELEVATION.MOUNTAIN;
        }
    }
	private BIOMES GetBiomeSimple(GameObject goHex){
		float moistureNoise = goHex.GetComponent<HexTile>().moistureNoise;
		float temperature = goHex.GetComponent<HexTile>().temperature;

		if(temperature <= 0.4f) {
            if (moistureNoise <= 0.45f) {
                return BIOMES.DESERT;
            } else {
                return BIOMES.GRASSLAND;
            }

            //if(moistureNoise <= 0.45f){
            //	return BIOMES.DESERT;
            //}else if(moistureNoise > 0.45f && moistureNoise <= 0.65f){
            //	return BIOMES.GRASSLAND;
            //}else if(moistureNoise > 0.65f){
            //	return BIOMES.WOODLAND;
            //}	

            /*
			if(moistureNoise <= 0.20f){
				return BIOMES.DESERT;
			}else if(moistureNoise > 0.20f && moistureNoise <= 0.40f){
				return BIOMES.GRASSLAND;
			}else if(moistureNoise > 0.40f && moistureNoise <= 0.55f){
				return BIOMES.WOODLAND;
			}else if(moistureNoise > 0.55f){
				return BIOMES.FOREST;
			}
			*/
        } else if(temperature > 0.4f && temperature <= 0.72f){
            if (moistureNoise <= 0.45f) {
                return BIOMES.GRASSLAND;
            } else {
                return BIOMES.FOREST;
            }
            //if(moistureNoise <= 0.45f){
            //	return BIOMES.GRASSLAND;
            //}else if(moistureNoise > 0.45f && moistureNoise <= 0.55f){
            //	return BIOMES.WOODLAND;
            //}else if(moistureNoise > 0.55f){
            //	return BIOMES.FOREST;
            //}			
            /*
			if(moistureNoise <= 0.20f){
				return BIOMES.DESERT;
			}else if(moistureNoise > 0.20f && moistureNoise <= 0.55f){
				return BIOMES.GRASSLAND;
			}else if(moistureNoise > 0.55f && moistureNoise <= 0.75f){
				return BIOMES.WOODLAND;
			}else if(moistureNoise > 0.75f){
				return BIOMES.FOREST;
			}
			*/
        } else if(temperature > 0.72f && temperature <= 0.82f){
			if (moistureNoise <= 0.62f){
				return BIOMES.TUNDRA;			
			} else if (moistureNoise > 0.62f){
				return BIOMES.SNOW;
			}

			/*
			if(moistureNoise <= 0.2f){
				return BIOMES.TUNDRA;
			}else if(moistureNoise > 0.2f && moistureNoise <= 0.55f){
				return BIOMES.GRASSLAND;
			}else if(moistureNoise > 0.55f && moistureNoise <= 0.75f){
				return BIOMES.WOODLAND;
			}else if(moistureNoise > 0.75f){
				return BIOMES.SNOW;
			}
			*/
		} else if(temperature > 0.82f){
			if(moistureNoise <= 0.4f){
				return BIOMES.TUNDRA;
			}else if(moistureNoise > 0.4f){
				return BIOMES.SNOW;
			}
		}
		return BIOMES.DESERT;
	}
    public bool TryGetTileSpriteAnimation(Sprite sprite, out RuntimeAnimatorController animator) {
        if (biomeSpriteAnimations.ContainsKey(sprite)) {
            animator = biomeSpriteAnimations[sprite];
            return true;
        }
        animator = default(RuntimeAnimatorController);
        return false;
    }
}