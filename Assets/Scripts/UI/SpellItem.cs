using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using EZObjectPools;
using TMPro;
using UnityEngine.Profiling;
using UnityEngine.UI;
using UtilityScripts;

public class SpellItem : NameplateItem<SkillData> {
    [SerializeField] private Image cooldownImage;
    [SerializeField] private Image cooldownCoverImage;
    [SerializeField] private TextMeshProUGUI currencyLbl;

    public SkillData spellData { get; private set; }

    //private Image _coverImg;

    private Func<SkillData, bool> _shouldBeInteractableChecker;

    public override void SetObject(SkillData spellData) {
        base.SetObject(spellData);
        name = spellData.name;
        button.name = spellData.name;
        toggle.name = spellData.name;
        this.spellData = spellData;
        UpdateData();
        Messenger.AddListener<SkillData>(PlayerSkillSignals.PLAYER_NO_ACTIVE_SPELL, OnPlayerNoActiveSpell);
        Messenger.AddListener<SkillData>(PlayerSkillSignals.SPELL_COOLDOWN_STARTED, OnSpellCooldownStarted);
        Messenger.AddListener<SkillData>(PlayerSkillSignals.SPELL_COOLDOWN_FINISHED, OnSpellCooldownFinished);
        Messenger.AddListener<SkillData>(PlayerSkillSignals.PLAYER_SKILL_LEVEL_UP, OnSpellUpgraded);
        Messenger.AddListener<SkillData>(PlayerSkillSignals.ON_EXECUTE_PLAYER_SKILL, OnExecuteSpell);
        Messenger.AddListener<SkillData>(PlayerSkillSignals.CHARGES_ADJUSTED, OnChargesAdjusted);
        Messenger.AddListener<SkillData>(PlayerSkillSignals.BONUS_CHARGES_ADJUSTED, OnBonusChargesAdjusted);
        Messenger.AddListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_MANA, OnPlayerAdjustedMana);
        SetAsDefault();

        //_coverImg = coverGO.GetComponent<Image>();
        //_coverImg.type = Image.Type.Filled;
        //_coverImg.fillMethod = Image.FillMethod.Horizontal;
    }
    public void UpdateData() {
        mainLbl.text = spellData.name;
        currencyLbl.text = string.Empty;
        PlayerSkillData playerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(spellData.type);
        SkillData updatedSkillData = PlayerSkillManager.Instance.GetSkillData(this.spellData.type);
        this.spellData = updatedSkillData;
        if (playerSkillData != null) {
            int manaCost = WorldSettings.Instance.worldSettingsData.IsScenarioMap()
                ? playerSkillData.GetManaCostForScenarios()
                : playerSkillData.GetManaCostBaseOnLevel(spellData.currentLevel);
            if (manaCost > 0) {
                manaCost = WorldSettings.Instance.worldSettingsData.IsScenarioMap()
                    ? playerSkillData.GetManaCostForScenarios()
                    : playerSkillData.GetManaCostBaseOnLevel(updatedSkillData.currentLevel);
                currencyLbl.text += $"{UtilityScripts.Utilities.ManaIcon()}{manaCost.ToString()} ";
            }
            //if (playerSkillData.GetMaxChargesBaseOnLevel(spellData.currentLevel) > 0) {
            //    currencyLbl.text += $"{UtilityScripts.Utilities.ChargesIcon()}{playerSkillData.GetMaxChargesBaseOnLevel(spellData.currentLevel)}  ";
            //}    
        } else {
            if (spellData.manaCost > 0) {
                currencyLbl.text += $"{UtilityScripts.Utilities.ManaIcon()}{spellData.manaCost} ";
            }
        }
        currencyLbl.text += $"{spellData.displayOfCurrentChargesWithBonusChargesCombinedIconFirst}  ";

        if (spellData.cooldown >= 0) {
            currencyLbl.text += $"{UtilityScripts.Utilities.CooldownIcon()}{GameManager.GetTimeAsWholeDuration(spellData.cooldown)} {GameManager.GetTimeIdentifierAsWholeDuration(spellData.cooldown)}  ";
        }
        //if (spellData.threat > 0) {
        //    currencyLbl.text += $"{UtilityScripts.Utilities.ThreatIcon()}{spellData.threat} ";
        //}
    }

    #region Listeners
    private void OnPlayerNoActiveSpell(SkillData spellData) {
        if(this.spellData == spellData) {
            if (_toggle.isOn) {
                _toggle.isOn = false;
            }
            UpdateData();
            UpdateInteractableState();
        }
    }
    private void OnSpellCooldownStarted(SkillData spellData) {
        if (this.spellData == spellData) {
            UpdateData();
            UpdateInteractableState();
            if (spellData is MinionPlayerSkill) {
                //do not check charges if spell is minion, because minion spells always regenerate, even if they have no more charges.
                SetCooldownState(spellData.isInCooldown);
                StartCooldownFill();
            } 
            // else if (spellData.hasCharges && spellData.charges <= 0) {
            //     //if spell uses charges, but has no more, do not show cooldown icon even if it is in cooldown
            //     SetCooldownState(false);
            // } 
            else {
                SetCooldownState(spellData.isInCooldown);
                StartCooldownFill();
            }
        }
    }

    private void OnSpellUpgraded(SkillData p_upgradedSkill) {
        //Debug.LogError(spellData.type + " == " + p_upgradedSkill.type);
        if (this.spellData.type == p_upgradedSkill.type) {
            UpdateData();
        }
    }

    private void OnSpellCooldownFinished(SkillData spellData) {
        if (this.spellData == spellData) {
            SetCooldownState(spellData.isInCooldown);
            UpdateData();
            // UpdateInteractableState();
            StopCooldownFill();
        }
    }
    private void OnExecuteSpell(SkillData spellData) {
        if (this.spellData == spellData) {
            UpdateData();
            UpdateInteractableState();
        }
    }
    private void OnChargesAdjusted(SkillData spellData) {
        if (this.spellData == spellData) {
            UpdateData();
            UpdateInteractableState();
        }
    }
    private void OnBonusChargesAdjusted(SkillData spellData) {
        if (this.spellData == spellData) {
            UpdateData();
            UpdateInteractableState();
        }
    }
    private void OnPlayerAdjustedMana(int adjusted, int mana) {
        UpdateData();
        UpdateInteractableState();
    }
    #endregion

    #region Utilities
    private void SetAsDefault() {
        SetAsToggle();
        ClearAllHoverEnterActions();
        ClearAllHoverExitActions();
        AddHoverEnterAction((spellData) => PlayerUI.Instance.OnHoverSpell(spellData, PlayerUI.Instance.spellListHoverPosition));
        AddHoverExitAction((spellData) => PlayerUI.Instance.OnHoverOutSpell(spellData));
    }
    #endregion

    #region Interactability
    private void SetCooldownState(bool state) {
        //cooldownImage.gameObject.SetActive(state);
        cooldownCoverImage.gameObject.SetActive(state);
    }
    public void ForceUpdateInteractableState() {
        UpdateInteractableState();
    }
    private void UpdateInteractableState() {
        SetInteractableState(_shouldBeInteractableChecker?.Invoke(spellData) ?? spellData.CanPerformAbility());
    }
    public void OnToggleSpell(bool state) {
        PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(null);
        if (state) {
            PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(spellData);
        }
    }
    public void SetInteractableChecker(System.Func<SkillData, bool> p_checker) {
        _shouldBeInteractableChecker = p_checker;
    }
    #endregion

    #region Cooldown
    public void UpdateCooldownFromLastState() {
        SetCooldownState(spellData.isInCooldown);
        if (cooldownCoverImage.gameObject.activeSelf) {
            float fillAmount = ((float) spellData.currentCooldownTick / spellData.cooldown);
            cooldownCoverImage.fillAmount = fillAmount;
            //Messenger.AddListener(Signals.TICK_STARTED, PerTickCooldown);
        }
    }
    private void StartCooldownFill() {
        cooldownCoverImage.fillAmount = 0f;
        PerTickCooldown();
        Messenger.AddListener(Signals.TICK_STARTED, PerTickCooldown);
    }
    private void PerTickCooldown() {
#if DEBUG_PROFILER
        Profiler.BeginSample($"Spell Item Per Tick Effect");
#endif
        float fillAmount = ((float)spellData.currentCooldownTick / spellData.cooldown);
        cooldownCoverImage.DOFillAmount(fillAmount, 0.4f);
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
    }
    private void StopCooldownFill() {
        cooldownCoverImage.fillAmount = 0f;
        UpdateInteractableState();
        //cooldownCoverImage.DOFillAmount(0f, 0.4f).OnComplete(UpdateInteractableState);
        Messenger.RemoveListener(Signals.TICK_STARTED, PerTickCooldown);
    }
#endregion

    public override void Reset() {
        base.Reset();
        button.name = "Button";
        toggle.name = "Toggle";
        SetInteractableState(true);
        SetCooldownState(false);
        spellData = null;
        cooldownCoverImage.fillAmount = 0f;
        if (Messenger.eventTable.ContainsKey(Signals.TICK_STARTED)) {
            Messenger.RemoveListener(Signals.TICK_STARTED, PerTickCooldown);
        }
        Messenger.RemoveListener<SkillData>(PlayerSkillSignals.PLAYER_NO_ACTIVE_SPELL, OnPlayerNoActiveSpell);
        Messenger.RemoveListener<SkillData>(PlayerSkillSignals.SPELL_COOLDOWN_STARTED, OnSpellCooldownStarted);
        Messenger.RemoveListener<SkillData>(PlayerSkillSignals.SPELL_COOLDOWN_FINISHED, OnSpellCooldownFinished);
        Messenger.RemoveListener<SkillData>(PlayerSkillSignals.ON_EXECUTE_PLAYER_SKILL, OnExecuteSpell);
        Messenger.RemoveListener<SkillData>(PlayerSkillSignals.CHARGES_ADJUSTED, OnChargesAdjusted);
        Messenger.RemoveListener<SkillData>(PlayerSkillSignals.BONUS_CHARGES_ADJUSTED, OnBonusChargesAdjusted);
        Messenger.RemoveListener<SkillData>(PlayerSkillSignals.PLAYER_SKILL_LEVEL_UP, OnSpellUpgraded);
        Messenger.RemoveListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_MANA, OnPlayerAdjustedMana);
    }
}
