using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MonsterUnderlingQuantityNameplateItem : NameplateItem<MonsterAndMinionUnderlingCharges> {

    [Header("Attributes")]
    [SerializeField] private CharacterPortrait portrait;


    private MonsterAndMinionUnderlingCharges _monsterOrMinion;
    public override MonsterAndMinionUnderlingCharges obj => _monsterOrMinion;

    public override void SetObject(MonsterAndMinionUnderlingCharges o) {
        base.SetObject(o);
        _monsterOrMinion = o;
        UpdateVisuals();
        UpdateMainText();
        UpdateBasicData();
    }
    public void UpdateBasicData() {
        UpdateMainText();
        UpdateQuantityText();
    }
    private void UpdateVisuals() {
        if(_monsterOrMinion.minionType != PLAYER_SKILL_TYPE.NONE) {
            portrait.GeneratePortrait(_monsterOrMinion.mi);
        }
        portrait.GeneratePortrait(_monsterOrMinion.monsterType);
    }
    private void UpdateMainText() {
        mainLbl.text = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(_monsterOrMinion.monsterType.ToString());
    }
    private void UpdateQuantityText() {
        int currentCharges = Math.Max(0, _monsterOrMinion.currentCharges);
        subLbl.text = currentCharges + "/" + _monsterOrMinion.maxCharges;
    }
}
