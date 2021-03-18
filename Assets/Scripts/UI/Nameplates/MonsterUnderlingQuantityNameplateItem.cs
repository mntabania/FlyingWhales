using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MonsterUnderlingQuantityNameplateItem : NameplateItem<MonsterAndDemonUnderlingCharges> {

    [Header("Attributes")]
    [SerializeField] private CharacterPortrait portrait;


    private MonsterAndDemonUnderlingCharges _monsterOrMinion;
    public override MonsterAndDemonUnderlingCharges obj => _monsterOrMinion;

    public override void SetObject(MonsterAndDemonUnderlingCharges o) {
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
        if (_monsterOrMinion.isDemon) {
            portrait.GeneratePortrait(_monsterOrMinion.minionType);
        } else if (_monsterOrMinion.monsterType != SUMMON_TYPE.None) {
            portrait.GeneratePortrait(_monsterOrMinion.monsterType);
        }
    }
    private void UpdateMainText() {
        if (_monsterOrMinion.isDemon) {
            MinionSettings settings = CharacterManager.Instance.GetMinionSettings(_monsterOrMinion.minionType);
            mainLbl.text = settings.className;
        } else if (_monsterOrMinion.monsterType != SUMMON_TYPE.None) {
            mainLbl.text = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(_monsterOrMinion.monsterType.ToString());
        }
    }
    private void UpdateQuantityText() {
        int currentCharges = Math.Max(0, _monsterOrMinion.currentCharges);
        subLbl.text = currentCharges + "/" + _monsterOrMinion.maxCharges;
    }

    public override void Reset() {
        base.Reset();
        _monsterOrMinion = null;
    }
}
