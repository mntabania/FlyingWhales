using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class MonsterUnderlingQuantityNameplateItem : NameplateItem<MonsterAndDemonUnderlingCharges> {

    [Header("Attributes")]
    [SerializeField] private CharacterPortrait portrait;
    [SerializeField] private RuinarchText txtHp;
    [SerializeField] private RuinarchText txtAttack;
    [SerializeField] private RuinarchText txtAttackSpeed;
    [SerializeField] private RuinarchText txtManaCost;
    [SerializeField] private Image cooldownCoverImage;

    private MonsterAndDemonUnderlingCharges _monsterOrMinion;
    private MinionPlayerSkill _demonPlayerSkill;
    public override MonsterAndDemonUnderlingCharges obj => _monsterOrMinion;
    public int summonCost { get; private set; }

    public override void SetObject(MonsterAndDemonUnderlingCharges o) {
        base.SetObject(o);
        _monsterOrMinion = o;
        if (_monsterOrMinion.isDemon) {
            _demonPlayerSkill = PlayerSkillManager.Instance.GetMinionPlayerSkillDataByMinionType(_monsterOrMinion.minionType);
        }
        UpdateVisuals();
        UpdateMainText();
        UpdateBasicData();
        UpdateCooldownState();
        SubscribeToSignals();
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
        CharacterClass cClass = CharacterManager.Instance.GetCharacterClass(obj.characterClassName);
        txtHp.text = cClass.baseHP.ToString();
        txtAttack.text = cClass.baseAttackPower.ToString();
        txtAttackSpeed.text = $"{cClass.baseAttackSpeed / 1000f}s";
        summonCost = CharacterManager.Instance.GetOrCreateCharacterClassData(cClass.className).GetSummonCost();
        txtManaCost.text = summonCost.ToString();
        if (_monsterOrMinion.isDemon) {
            mainLbl.text = cClass.className;
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
    private void SubscribeToSignals() {
        Messenger.AddListener<SkillData>(PlayerSkillSignals.SPELL_COOLDOWN_STARTED, OnSpellCooldownStarted);
        Messenger.AddListener<SkillData>(PlayerSkillSignals.SPELL_COOLDOWN_FINISHED, OnSpellCooldownFinished);
        Messenger.AddListener<MonsterAndDemonUnderlingCharges>(PlayerSkillSignals.START_MONSTER_UNDERLING_COOLDOWN, OnStartMonsterUnderlingCooldown);
        Messenger.AddListener<MonsterAndDemonUnderlingCharges>(PlayerSkillSignals.STOP_MONSTER_UNDERLING_COOLDOWN, OnStopMonsterUnderlingCooldown);
        Messenger.AddListener<MinionPlayerSkill>(PlayerSkillSignals.PER_TICK_DEMON_COOLDOWN, PerTickDemonCooldownListener);
        Messenger.AddListener<MonsterAndDemonUnderlingCharges>(PlayerSkillSignals.PER_TICK_MONSTER_UNDERLING_COOLDOWN, PerTickMonsterUnderlingCooldownListener);

    }
    private void UnsubscribeToSignals() {
        Messenger.RemoveListener<SkillData>(PlayerSkillSignals.SPELL_COOLDOWN_STARTED, OnSpellCooldownStarted);
        Messenger.RemoveListener<SkillData>(PlayerSkillSignals.SPELL_COOLDOWN_FINISHED, OnSpellCooldownFinished);
        Messenger.RemoveListener<MonsterAndDemonUnderlingCharges>(PlayerSkillSignals.START_MONSTER_UNDERLING_COOLDOWN, OnStartMonsterUnderlingCooldown);
        Messenger.RemoveListener<MonsterAndDemonUnderlingCharges>(PlayerSkillSignals.STOP_MONSTER_UNDERLING_COOLDOWN, OnStopMonsterUnderlingCooldown);
        Messenger.RemoveListener<MinionPlayerSkill>(PlayerSkillSignals.PER_TICK_DEMON_COOLDOWN, PerTickDemonCooldownListener);
        Messenger.RemoveListener<MonsterAndDemonUnderlingCharges>(PlayerSkillSignals.PER_TICK_MONSTER_UNDERLING_COOLDOWN, PerTickMonsterUnderlingCooldownListener);
    }

    #region Listeners
    private void OnSpellCooldownStarted(SkillData data) {
        if (_demonPlayerSkill == data) {
            StartCooldownFillDemon();
        }
    }
    private void OnSpellCooldownFinished(SkillData data) {
        if (_demonPlayerSkill == data) {
            StopCooldownFill();
        }
    }
    private void OnStartMonsterUnderlingCooldown(MonsterAndDemonUnderlingCharges data) {
        if (_monsterOrMinion == data) {
            StartCooldownFillMonster();
        }
    }
    private void OnStopMonsterUnderlingCooldown(MonsterAndDemonUnderlingCharges data) {
        if (_monsterOrMinion == data) {
            StopCooldownFill();
        }
    }
    private void PerTickMonsterUnderlingCooldownListener(MonsterAndDemonUnderlingCharges data) {
        if (_monsterOrMinion == data) {
            PerTickCooldownMonster();
        }
    }
    private void PerTickDemonCooldownListener(MinionPlayerSkill data) {
        if (_demonPlayerSkill == data) {
            PerTickCooldownDemon();
        }
    }
    #endregion

    #region Cooldown
    private void UpdateCooldownState() {
        if (_monsterOrMinion.isReplenishing) {
            StartCooldownFillMonster();
        } else if (_demonPlayerSkill != null && _demonPlayerSkill.isInCooldown) {
            StartCooldownFillDemon();
        } else {
            StopCooldownFill();
        }
    }
    private void StartCooldownFillDemon() {
        cooldownCoverImage.fillAmount = ((float) _demonPlayerSkill.currentCooldownTick / _demonPlayerSkill.cooldown);
        cooldownCoverImage.gameObject.SetActive(true);
    }
    private void StartCooldownFillMonster() {
        cooldownCoverImage.fillAmount = ((float) _monsterOrMinion.currentCooldownTick / _monsterOrMinion.cooldown);
        cooldownCoverImage.gameObject.SetActive(true);
    }
    private void PerTickCooldownDemon() {
        float fillAmount = ((float) _demonPlayerSkill.currentCooldownTick / _demonPlayerSkill.cooldown);
        cooldownCoverImage.DOFillAmount(fillAmount, 0.4f);
    }
    private void PerTickCooldownMonster() {
        float fillAmount = ((float) _monsterOrMinion.currentCooldownTick / _monsterOrMinion.cooldown);
        cooldownCoverImage.DOFillAmount(fillAmount, 0.4f);
    }
    private void StopCooldownFill() {
        cooldownCoverImage.fillAmount = 0f;
        cooldownCoverImage.gameObject.SetActive(false);
    }
    #endregion

    public override void Reset() {
        base.Reset();
        _monsterOrMinion = null;
        _demonPlayerSkill = null;
        StopCooldownFill();
        UnsubscribeToSignals();
    }
}
