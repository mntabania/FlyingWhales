using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;

public class TileHighlighter : MonoBehaviour {

    public static TileHighlighter Instance;

    [SerializeField] private Transform parentTransform;
    [SerializeField] private ParticleSystem[] _particleSystems;
    
    private void Awake() {
        Instance = this;
    }
    private void SetupHighlight(int radius) {
        int diameter = radius * 2;
        if (UtilityScripts.Utilities.IsEven(diameter)) {
            diameter++;
        } else {
            diameter--;
        }
        if (diameter == 0) {
            diameter = 1;
        }

        Vector3 scale = new Vector3(diameter, 1f, diameter);
        for (int i = 0; i < _particleSystems.Length; i++) {
            ParticleSystem p = _particleSystems[i];
            //shape
            ParticleSystem.ShapeModule shapeModule = p.shape;
            shapeModule.scale = scale;
            
            //max particles
            ParticleSystem.MainModule mainModule = p.main; 
            mainModule.maxParticles = diameter * 60;
            
            //emission module
            ParticleSystem.EmissionModule emissionModule = p.emission;
            emissionModule.rateOverTime = diameter * 20;
        }
    }
    public void PositionHighlight(int radius, LocationGridTile centerTile) {
        SetupHighlight(radius);
        parentTransform.transform.position = centerTile.centeredWorldLocation;
        parentTransform.gameObject.SetActive(true);
    }
    public void PositionHighlight(HexTile tile) {
        SetupHighlight(InnerMapManager.BuildingSpotSize.x - 1);
        parentTransform.transform.position = tile.worldPosition;
        parentTransform.gameObject.SetActive(true);
    }
    public void HideHighlight() {
        parentTransform.gameObject.SetActive(false);
    }
}
