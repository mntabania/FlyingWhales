using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ruinarch.Custom_UI;
using TMPro;

public class BiomeWorldOptionItem : MonoBehaviour {
    public RuinarchToggle toggle;
    public TextMeshProUGUI title;

    public BIOMES biome { get; private set; }

    public void SetBiome(BIOMES biome) {
        this.biome = biome;
        title.text = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(biome.ToString());
    }
    public void OnToggle(bool state) {
        Messenger.Broadcast(Signals.BIOME_WORLD_OPTION_ITEM_CLICKED, biome, state);
    }
}
