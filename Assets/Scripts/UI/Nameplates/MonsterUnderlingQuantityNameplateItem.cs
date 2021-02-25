using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MonsterUnderlingQuantityNameplateItem : NameplateItem<MonsterUnderlingCharges> {

    [Header("Attributes")]
    [SerializeField] private CharacterPortrait portrait;


    private MonsterUnderlingCharges _monster;
    public override MonsterUnderlingCharges obj => _monster;

    public override void SetObject(MonsterUnderlingCharges o) {
        base.SetObject(o);
        _monster = o;
        UpdateVisuals();
        UpdateMainText();
        UpdateBasicData();
    }
    public void UpdateBasicData() {
        UpdateMainText();
        UpdateQuantityText();
    }
    private void UpdateVisuals() {
        portrait.GeneratePortrait(_monster.monsterType);
    }
    private void UpdateMainText() {
        mainLbl.text = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(_monster.monsterType.ToString());
    }
    private void UpdateQuantityText() {
        subLbl.text = _monster.currentCharges + "/" + _monster.maxCharges;
    }
}
