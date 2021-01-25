using System.Collections.Generic;
using UnityEngine;

public class WorldGenOptionsTester : MonoBehaviour {

    public WorldGenOptionsUIController controller;

    [SerializeField] private List<Sprite> factionEmblems;
    
    private void Awake() {
        RandomNameGenerator.Initialize();
        FactionEmblemRandomizer.Initialize(factionEmblems);
        controller.InitUI(null);
    }
}
