using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class RaceSetting {
    public RACE race;
    //public float attackPowerModifier;
    //public float speedModifier;
    //public float hpModifier;
    public float hpMultiplier;
    public float attackMultiplier;
    public float walkSpeed;
    public float runSpeed;
    //public int neutralSpawnLevelModifier;
    //public int[] hpPerLevel;
    //public int[] attackPerLevel;
    public string[] traitNames;

    //#region getters/setters
    //public int neutralSpawnLevel {
    //    get { return FactionManager.Instance.GetAverageFactionLevel() + neutralSpawnLevelModifier; }
    //}
    //#endregion

    public RaceSetting CreateNewCopy() {
        RaceSetting newRaceSetting = new RaceSetting();
        newRaceSetting.race = this.race;
        //newRaceSetting.attackPowerModifier = this.attackPowerModifier;
        //newRaceSetting.speedModifier = this.speedModifier;
        //newRaceSetting.hpModifier = this.hpModifier;
        newRaceSetting.hpMultiplier = this.hpMultiplier;
        newRaceSetting.attackMultiplier = this.attackMultiplier;
        newRaceSetting.runSpeed = this.runSpeed;
        newRaceSetting.walkSpeed = this.walkSpeed;
        //newRaceSetting.hpPerLevel = this.hpPerLevel;
        //newRaceSetting.attackPerLevel = this.attackPerLevel;
        newRaceSetting.traitNames = this.traitNames;
        return newRaceSetting;
    }

    public void SetDataFromRacePanelUI() {
        this.race = (RACE) System.Enum.Parse(typeof(RACE), RacePanelUI.Instance.raceOptions.options[RacePanelUI.Instance.raceOptions.value].text);
        this.hpMultiplier = float.Parse(RacePanelUI.Instance.hpMultiplierInput.text);
        this.attackMultiplier = float.Parse(RacePanelUI.Instance.attackMultiplierInput.text);
        //this.speedModifier = float.Parse(RacePanelUI.Instance.speedModifierInput.text);
        this.runSpeed = float.Parse(RacePanelUI.Instance.runSpeedInput.text);
        this.walkSpeed = float.Parse(RacePanelUI.Instance.walkSpeedInput.text);
        //this.hpPerLevel = RacePanelUI.Instance.hpPerLevel.ToArray();
        //this.attackPerLevel = RacePanelUI.Instance.attackPerLevel.ToArray();
        this.traitNames = RacePanelUI.Instance.traitNames.ToArray();
        //this.neutralSpawnLevelModifier = int.Parse(RacePanelUI.Instance.neutralSpawnLevelModInput.text);
    }
}