﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class MonsterInfoUI : UIMenu {

    [Space(10)]
    [Header("Basic Info")]
    [SerializeField] private TextMeshProUGUI monsterInfoNameLbl;
    [SerializeField] private TextMeshProUGUI monsterInfoLevelLbl;

    [Space(10)]
    [Header("Stat Info")]
    [SerializeField] private Slider healthProgressBar;
    [SerializeField] private Slider manaProgressBar;
    [SerializeField] private TextMeshProUGUI strengthLbl;
    [SerializeField] private TextMeshProUGUI agilityLbl;
    [SerializeField] private TextMeshProUGUI intelligenceLbl;
    [SerializeField] private TextMeshProUGUI vitalityLbl;
    [SerializeField] private TextMeshProUGUI expDropLbl;

    [Space(10)]
    [Header("Other Info")]
    [SerializeField] private TextMeshProUGUI otherInfoLbl;

    internal Monster currentlyShowingMonster {
        get { return _data as Monster; }
    }

    //internal override void Initialize() {
    //    base.Initialize();
    //    //Messenger.AddListener(Signals.UPDATE_UI, UpdateMonsterInfo);
    //}

    #region Overrides
    public override void OpenMenu() {
        base.OpenMenu();
        UpdateMonsterInfo();
        PlayerAbilitiesUI.Instance.ShowPlayerAbilitiesUI(currentlyShowingMonster);
    }
    public override void CloseMenu() {
        base.CloseMenu();
        PlayerAbilitiesUI.Instance.HidePlayerAbilitiesUI();
    }
    public override void SetData(object data) {
        base.SetData(data);
        if (isShowing) {
            UpdateMonsterInfo();
        }
    }
    public override void ShowTooltip(GameObject objectHovered) {
        base.ShowTooltip(objectHovered);
        if (objectHovered == healthProgressBar.gameObject) {
            UIManager.Instance.ShowSmallInfo(currentlyShowingMonster.currentHP + "/" + currentlyShowingMonster.maxHP);
        } else if (objectHovered == manaProgressBar.gameObject) {
            UIManager.Instance.ShowSmallInfo(currentlyShowingMonster.currentSP + "/" + currentlyShowingMonster.maxSP);
        } 
        //else if (objectHovered == overallProgressBar.gameObject) {
        //    UIManager.Instance.ShowSmallInfo(currentlyShowingCharacter.role.happiness.ToString());
        //} else if (objectHovered == energyProgressBar.gameObject) {
        //    UIManager.Instance.ShowSmallInfo(currentlyShowingCharacter.role.energy.ToString());
        //} else if (objectHovered == fullnessProgressBar.gameObject) {
        //    UIManager.Instance.ShowSmallInfo(currentlyShowingCharacter.role.fullness.ToString());
        //} else if (objectHovered == funProgressBar.gameObject) {
        //    UIManager.Instance.ShowSmallInfo(currentlyShowingCharacter.role.fun.ToString());
        //}
        //else if (objectHovered == prestigeProgressBar.gameObject) {
        //    UIManager.Instance.ShowSmallInfo(currentlyShowingCharacter.role.prestige.ToString());
        //} else if (objectHovered == sanityProgressBar.gameObject) {
        //    UIManager.Instance.ShowSmallInfo(currentlyShowingCharacter.role.sanity.ToString());
        //}
    }
    #endregion

    public void UpdateMonsterInfo() {
        if (currentlyShowingMonster == null) {
            return;
        }
        UpdateBasicInfo();
        UpdateStatsInfo();
        UpdateOtherInfo();
        //Item drop info

    }

    private void UpdateBasicInfo() {
        monsterInfoNameLbl.text = currentlyShowingMonster.name;
        monsterInfoLevelLbl.text = "Lvl." + currentlyShowingMonster.level;
    }
    private void UpdateStatsInfo() {
        healthProgressBar.value = (float) currentlyShowingMonster.currentHP / (float) currentlyShowingMonster.maxHP;
        manaProgressBar.value = (float) currentlyShowingMonster.currentSP / (float) currentlyShowingMonster.maxSP;
        strengthLbl.text = currentlyShowingMonster.strength.ToString();
        agilityLbl.text = currentlyShowingMonster.agility.ToString();
        intelligenceLbl.text = currentlyShowingMonster.intelligence.ToString();
        vitalityLbl.text = currentlyShowingMonster.vitality.ToString();
        expDropLbl.text = currentlyShowingMonster.experienceDrop.ToString();
    }
    private void UpdateOtherInfo() {
        string text = string.Empty;
        text += "\n<b>P Final Attack:</b> " + currentlyShowingMonster.pFinalAttack;
        text += "\n<b>M Final Attack:</b> " + currentlyShowingMonster.pFinalAttack;
        text += "\n<b>Def:</b> " + currentlyShowingMonster.def;
        text += "\n<b>Crit Chance:</b> " + currentlyShowingMonster.critChance + "%";
        text += "\n<b>Dodge Chance:</b> " + currentlyShowingMonster.dodgeChance + "%";
        text += "\n<b>Computed Power:</b> " + currentlyShowingMonster.computedPower;
        text += "\n<b>Is Sleeping:</b> " + currentlyShowingMonster.isSleeping;

        otherInfoLbl.text = text;
    }
}
