using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MonsterUnderlingQuantityNameplateItem : NameplateItem<MonsterAndDemonUnderlingCharges> {

    [Header("Attributes")]
    [SerializeField] private CharacterPortrait portrait;
    [SerializeField] private RuinarchText txtHp;
    [SerializeField] private RuinarchText txtAttack;
    [SerializeField] private RuinarchText txtAttackSpeed;
    [SerializeField] private RuinarchText txtManaCost;

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
            mainLbl.text = obj.characterClass.className;
            txtHp.text = obj.characterClass.baseHP.ToString();
            txtAttack.text = obj.characterClass.baseAttackPower.ToString();
            txtAttackSpeed.text = obj.characterClass.baseAttackSpeed.ToString();
        } else if (_monsterOrMinion.monsterType != SUMMON_TYPE.None) {
            mainLbl.text = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(_monsterOrMinion.monsterType.ToString());
        }
    }
    private void UpdateQuantityText() {
        int currentCharges = Math.Max(0, _monsterOrMinion.currentCharges);
        m_displayRemainingChargeText = currentCharges;
        m_displayMaxChrageText = _monsterOrMinion.maxCharges;
        subLbl.text = currentCharges + "/" + _monsterOrMinion.maxCharges;
    }

    public override void Reset() {
        base.Reset();
        _monsterOrMinion = null;
    }
}
