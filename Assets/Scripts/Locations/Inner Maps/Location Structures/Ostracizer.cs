﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;
using Random = UnityEngine.Random;
namespace Inner_Maps.Location_Structures {
    public class Ostracizer : DemonicStructure {

        private bool _isLearnSpellInCooldown;
        private string _cooldownScheduleKey;
        
        public Ostracizer(Region location) : base(STRUCTURE_TYPE.OSTRACIZER, location){ }
        public Ostracizer(Region location, SaveDataDemonicStructure data) : base(location, data) { }

        //#region Overrides
        //public override void Initialize() {
        //    base.Initialize();
        //    AddLearnSpell();
        //}
        //protected override void DestroyStructure() {
        //    base.DestroyStructure();
        //    RemoveLearnSpell();
        //    if (string.IsNullOrEmpty(_cooldownScheduleKey) == false) {
        //        SchedulingManager.Instance.RemoveSpecificEntry(_cooldownScheduleKey);
        //    }
        //}
        //#endregion

        //#region Learn Spell
        //private void AddLearnSpell() {
        //    //PlayerAction learnSpellAction = new PlayerAction(PlayerDB.Learn_Spell_Action, CanLearnSpell, null, TryLearnASpellOrAffliction);
        //    AddPlayerAction(PLAYER_SKILL_TYPE.LEARN_SPELL);
        //}
        //private void RemoveLearnSpell() {
        //    RemovePlayerAction(PLAYER_SKILL_TYPE.LEARN_SPELL);
        //}
        //public bool CanLearnSpell() {
        //    return _isLearnSpellInCooldown == false && PlayerManager.Instance.player.mana >= 100 &&
        //           PlayerManager.Instance.player.unlearnedSpells.Count > 0 && PlayerManager.Instance.player.unlearnedAfflictions.Count > 0;
        //}
        //public void TryLearnASpellOrAffliction() {
        //    bool canLearnSpells = PlayerManager.Instance.player.unlearnedSpells.Count > 0;
        //    bool canLearnAfflictions = PlayerManager.Instance.player.unlearnedAfflictions.Count > 0;
        //    if (canLearnSpells && canLearnAfflictions) {
        //        //can still learn both ways
        //        if (Random.Range(0, 2) == 0) {
        //            //learn spell
        //            TryLearnSpell();
        //        } else {
        //            //learn affliction
        //            TryLearnAffliction();
        //        }
        //    } else if (canLearnSpells) {
        //        //can only learn spells
        //        TryLearnSpell();
        //    } else if (canLearnAfflictions) {
        //        //can only learn afflictions
        //        TryLearnAffliction();
        //    }
        //}
        //private void TryLearnSpell() {
        //    // List<SPELL_TYPE> spellChoices =
        //    //     CollectionUtilities.GetRandomElements(PlayerManager.Instance.player.unlearnedSpells, 2);
        //    // UIManager.Instance.ShowClickableObjectPicker(spellChoices, LearnNewSpell, null, null, 
        //    //     "Choose a spell to learn", OnHoverSpell, OnHoverExitSpell, showCover: true);
        //}
        //private void LearnNewSpell(object obj) {
        //    //SPELL_TYPE spellType = (SPELL_TYPE) obj;
        //    //PlayerManager.Instance.player.LearnSpell(spellType);
        //    UIManager.Instance.HideObjectPicker();
        //    OnSpellLearned();
        //    PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.LEARN_SPELL).OnExecuteSpellActionAffliction();
        //}
        //private void OnHoverSpell(PLAYER_SKILL_TYPE spell) {
            
        //}
        //private void OnHoverExitSpell(PLAYER_SKILL_TYPE spell) {
            
        //}
        //private void TryLearnAffliction() {
        //    // List<SPELL_TYPE> afflictionChoices =
        //    //     CollectionUtilities.GetRandomElements(PlayerManager.Instance.player.unlearnedAfflictions, 2);
        //    // UIManager.Instance.ShowClickableObjectPicker(afflictionChoices, LearnNewAffliction, null, null, 
        //    //     "Choose a spell to learn", OnHoverAffliction, OnHoverExitAffliction, showCover: true, portraitGetter:AfflictionPortraitGetter);
        //}
        //private void LearnNewAffliction(object obj) {
        //    //SPELL_TYPE spellType = (SPELL_TYPE)obj;
        //    //PlayerManager.Instance.player.LearnAffliction(spellType);
        //    UIManager.Instance.HideObjectPicker();
        //    OnSpellLearned();
        //    PlayerSkillManager.Instance.GetPlayerActionData(PLAYER_SKILL_TYPE.LEARN_SPELL).OnExecuteSpellActionAffliction();
        //}
        //private void OnHoverAffliction(PLAYER_SKILL_TYPE spellType) {
        //    SpellData data = PlayerSkillManager.Instance.GetAfflictionData(spellType);
        //    UIManager.Instance.ShowSmallInfo(data.description, data.name);
        //}
        //private void OnHoverExitAffliction(PLAYER_SKILL_TYPE spellType) {
        //    UIManager.Instance.HideSmallInfo();
        //}
        //private Sprite AfflictionPortraitGetter(string afflictionStr) {
        //    return PlayerManager.Instance.GetJobActionSprite(UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(afflictionStr));
        //}
        //private void OnSpellLearned() {
        //    //reduce mana
        //    PlayerManager.Instance.player.AdjustMana(-100);
        //    //start cooldown
        //    _isLearnSpellInCooldown = true;
        //    GameDate dueDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(6));
        //    _cooldownScheduleKey = SchedulingManager.Instance.AddEntry(dueDate, CooldownFinished, this);
        //    Messenger.Broadcast(SpellSignals.RELOAD_PLAYER_ACTIONS, this as IPlayerActionTarget);
        //}
        //private void CooldownFinished() {
        //    _cooldownScheduleKey = string.Empty;
        //    _isLearnSpellInCooldown = false;
        //    Messenger.Broadcast(SpellSignals.RELOAD_PLAYER_ACTIONS, this as IPlayerActionTarget);
        //}
        //#endregion
    }
}