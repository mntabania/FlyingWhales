using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using EZObjectPools;

public class EyeWardHighlight : PooledObject {

    [SerializeField] private ParticleSystem[] _particleSystems;
    public void SetupHighlight(int radius) {
        int diameter = radius * 2;
        if (UtilityScripts.Utilities.IsEven(diameter)) {
            diameter++;
        } else {
            diameter--;
        }
        if (diameter == 0) {
            diameter = 1;
        }
        Vector3 scale = new Vector3(diameter, diameter, 0f);
        for (int i = 0; i < _particleSystems.Length; i++) {
            ParticleSystem p = _particleSystems[i];
            //shape
            ParticleSystem.ShapeModule shapeModule = p.shape;
            shapeModule.scale = scale;

            //int maxParticles = diameter * 72;
            //float rateOverTime = diameter * 25f;
            
            //max particles
            //ParticleSystem.MainModule mainModule = p.main; 
            //mainModule.maxParticles = maxParticles;

            //emission module
            //ParticleSystem.EmissionModule emissionModule = p.emission;
            //emissionModule.rateOverTime = rateOverTime;

        }
    }
    //private void SetupHighlight(int radius) {
    //    SetupHighlight(radius);
    //}
    //public void PositionHighlight(int radius, LocationGridTile centerTile) {
    //    SetupHighlight(radius, centerTile.biomeType);
    //    transform.position = centerTile.centeredWorldLocation;
    //    gameObject.SetActive(true);
    //}
    //public void PositionHighlight(Area p_area) {
    //    SetupHighlight((InnerMapManager.AreaLocationGridTileSize.x / 2) - 1, p_area.biomeType);
    //    transform.position = p_area.worldPosition;
    //    gameObject.SetActive(true);
    //}
    //public void PositionHighlight(Area p_area, Color color) {
    //    SetupHighlight((InnerMapManager.AreaLocationGridTileSize.x / 2) - 1, p_area.biomeType, color);
    //    transform.position = p_area.worldPosition;
    //    gameObject.SetActive(true);
    //}
    public void HideHighlight() {
        for (int i = 0; i < _particleSystems.Length; i++) {
            _particleSystems[i].Stop();
        }
        gameObject.SetActive(false);
    }
    public void ShowHighlight() {
        for (int i = 0; i < _particleSystems.Length; i++) {
            _particleSystems[i].Play();
        }
        gameObject.SetActive(true);
    }
}
