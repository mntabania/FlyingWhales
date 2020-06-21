using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;

public class TileHighlighter : MonoBehaviour {

    public static TileHighlighter Instance;

    [SerializeField] private Transform parentTransform;
    [SerializeField] private ParticleSystem[] _particleSystems;
    [SerializeField] private SpriteRenderer innerImage;

    [SerializeField] private BiomeHighlightColorDictionary _biomeHighlightColor;
    private void Awake() {
        Instance = this;
    }
    private void SetupHighlight(int radius, BIOMES biome) {
        int diameter = radius * 2;
        if (UtilityScripts.Utilities.IsEven(diameter)) {
            diameter++;
        } else {
            diameter--;
        }
        if (diameter == 0) {
            diameter = 1;
        }
        innerImage.gameObject.SetActive(false);  
        Vector3 scale = new Vector3(diameter, 1f, diameter);
        for (int i = 0; i < _particleSystems.Length; i++) {
            ParticleSystem p = _particleSystems[i];
            //shape
            ParticleSystem.ShapeModule shapeModule = p.shape;
            shapeModule.scale = scale;
            //color
            p.GetComponent<ParticleSystemRenderer>().material = _biomeHighlightColor[biome];

            int maxParticles;
            int rateOverTime;
            
            if (biome == BIOMES.SNOW) {
                maxParticles = diameter * 80;
                rateOverTime = diameter * 40;  
            } else {
                maxParticles = diameter * 50;
                rateOverTime = diameter * 10;
            }
            
            //max particles
            ParticleSystem.MainModule mainModule = p.main; 
            mainModule.maxParticles = maxParticles;

            //emission module
            ParticleSystem.EmissionModule emissionModule = p.emission;
            emissionModule.rateOverTime = rateOverTime;
        }
    }
    private void SetupHighlight(int radius, BIOMES biome, Color innerImageColor) {
        SetupHighlight(radius, biome);
        innerImage.color = innerImageColor;
        innerImage.gameObject.SetActive(true);  
        
    }
    public void PositionHighlight(int radius, LocationGridTile centerTile) {
        SetupHighlight(radius, centerTile.parentMap.region.coreTile.biomeType);
        parentTransform.transform.position = centerTile.centeredWorldLocation;
        parentTransform.gameObject.SetActive(true);
    }
    public void PositionHighlight(HexTile tile) {
        SetupHighlight(InnerMapManager.BuildingSpotSize.x - 1, tile.biomeType);
        parentTransform.transform.position = tile.worldPosition;
        parentTransform.gameObject.SetActive(true);
    }
    public void PositionHighlight(HexTile tile, Color color) {
        SetupHighlight(InnerMapManager.BuildingSpotSize.x - 1, tile.biomeType, color);
        parentTransform.transform.position = tile.worldPosition;
        parentTransform.gameObject.SetActive(true);
    }
    public void HideHighlight() {
        parentTransform.gameObject.SetActive(false);
    }
}
