using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Archetype;

public class ArchetypeSelectorItem : MonoBehaviour {
    public PLAYER_ARCHETYPE archetype;

    public StringSpriteDictionary minionPortraits;

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI spellsText;
    public TextMeshProUGUI monstersText;
    public TextMeshProUGUI manipulationsText;
    public TextMeshProUGUI structuresText;
    public Image[] minionImages;

    private void Start() {
        Initialize();
    }

    private void Initialize() {
        PlayerArchetype playerArchetype = PlayerManager.CreateNewArchetype(archetype);
        titleText.text = "The " + playerArchetype.name;
        descriptionText.text = playerArchetype.selectorDescription;
        UpdateSpellsText(playerArchetype);
        UpdateMonstersText(playerArchetype);
        UpdateManipulationsText(playerArchetype);
        UpdateStructuresText(playerArchetype);
        UpdateMinionImages(playerArchetype);
    }
    private void UpdateSpellsText(PlayerArchetype playerArchetype) {
        string spells = string.Empty;
        for (int i = 0; i < playerArchetype.spells.Count; i++) {
            if(i > 0) {
                spells += ", ";
            }
            spells += UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(playerArchetype.spells[i].ToString());
        }
        if(spells != string.Empty) {
            spellsText.text = spells;
        } else {
            spellsText.text = "None";
        }
    }
    private void UpdateMonstersText(PlayerArchetype playerArchetype) {
        string monsters = string.Empty;
        for (int i = 0; i < playerArchetype.monsters.Count; i++) {
            if (i > 0) {
                monsters += ", ";
            }
            monsters += UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(playerArchetype.monsters[i].race.ToString());
        }
        if (monsters != string.Empty) {
            monstersText.text = monsters;
        } else {
            monstersText.text = "None";
        }
    }
    private void UpdateManipulationsText(PlayerArchetype playerArchetype) {
        string afflictions = string.Empty;
        for (int i = 0; i < playerArchetype.afflictions.Count; i++) {
            if (i > 0) {
                afflictions += ", ";
            }
            afflictions += UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(playerArchetype.afflictions[i].ToString());
        }
        if (afflictions != string.Empty) {
            manipulationsText.text = afflictions;
        } else {
            manipulationsText.text = "None";
        }
    }
    private void UpdateStructuresText(PlayerArchetype playerArchetype) {
        string structures = string.Empty;
        for (int i = 0; i < playerArchetype.demonicStructures.Count; i++) {
            if (i > 0) {
                structures += ", ";
            }
            structures += UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(playerArchetype.demonicStructures[i].ToString());
        }
        if (structures != string.Empty) {
            structuresText.text = structures;
        } else {
            structuresText.text = "None";
        }
    }
    private void UpdateMinionImages(PlayerArchetype playerArchetype) {
        for (int i = 0; i < playerArchetype.minionClasses.Count; i++) {
            minionImages[i].sprite = minionPortraits[playerArchetype.minionClasses[i]];
        }
    }
}
