using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ruinarch.Custom_UI;
using TMPro;

public class RaceWorldOptionItem : MonoBehaviour {
    public RuinarchToggle toggle;
    public TextMeshProUGUI title;

    public RACE race { get; private set; }

    public void SetRace(RACE race) {
        this.race = race;
        title.text = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(race.ToString());
    }
    public void OnToggle(bool state) {
        Messenger.Broadcast(Signals.RACE_WORLD_OPTION_ITEM_CLICKED, race, state);
    }
}
