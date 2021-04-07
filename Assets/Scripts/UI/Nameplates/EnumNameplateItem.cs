﻿using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class EnumNameplateItem : NameplateItem<Enum> {

    [SerializeField] private Image portrait;
    [SerializeField] private Image cooldownImage;
    [SerializeField] private Image lockedImage;

    public bool isLocked => lockedImage.gameObject.activeSelf;
    
    public override void SetObject(Enum o) {
        base.SetObject(o);
        string gameObjectName = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(o.ToString());
        name = gameObjectName;
        button.name = gameObjectName;
        toggle.name = gameObjectName;
        mainLbl.text = gameObjectName;
        subLbl.text = string.Empty;

        //TODO: Make this better
        if (o is PLAYER_SKILL_TYPE spellType) {
            // if (WorldConfigManager.Instance.isTutorialWorld) {
            //     SetLockedState(WorldConfigManager.Instance.availableSpellsInTutorial.Contains(spellType) == false);
            // }
            SkillData spellData = PlayerSkillManager.Instance.GetSkillData(spellType);
            //if (PlayerSkillManager.Instance.IsAffliction(spellType)) {
            //    spellData = PlayerSkillManager.Instance.GetAfflictionData(spellType);
            //} else if (PlayerSkillManager.Instance.IsPlayerAction(spellType)) {
            //    spellData = PlayerSkillManager.Instance.GetPlayerSkillData(spellType);
            //} else if (PlayerSkillManager.Instance.IsDemonicStructure(spellType)) {
            //    spellData = PlayerSkillManager.Instance.GetDemonicStructureSkillData(spellType);
            //} else {
            //    spellData = PlayerSkillManager.Instance.GetSpellData(spellType);
            //}
            Assert.IsNotNull(spellData, $"There is no spellData for {spellType.ToString()}");
            if (spellData.hasCharges && spellData.charges <= 0) {
                //if spell uses charges, but has no more, do not show cooldown icon even if it is in cooldown
                SetCooldownState(false);
            } else {
                SetCooldownState(spellData.isInCooldown);
            }
            
        } else {
            SetLockedState(false);
            SetCooldownState(false);
        }
    }
    public void SetPortrait(Sprite sprite) {
        portrait.sprite = sprite;
        portrait.gameObject.SetActive(portrait.sprite != null);
        
    }

    private void SetLockedState(bool state) {
        lockedImage.gameObject.SetActive(state);
    }
    private void SetCooldownState(bool state) {
        cooldownImage.gameObject.SetActive(state);
    }
    public override void Reset() {
        base.Reset();
        name = "Nameplate Item";
    }
}
