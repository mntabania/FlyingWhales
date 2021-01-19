using System;
using System.Collections.Generic;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
namespace Events.World_Events {
    public class UndeadAttackEvent : WorldEvent {

        public UndeadAttackEvent() {
            
        }
        public UndeadAttackEvent(SaveUndeadAttackEvent data) {
            
        }

        public override void InitializeEvent() {
            Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted);
        
        }

        public void StartUndeadAttack() {
            for (int i = 0; i < FactionManager.Instance.undeadFaction.characters.Count; i++) {
                Character character = FactionManager.Instance.undeadFaction.characters[i];
                if (!character.isDead) {
                    character.behaviourComponent.AddBehaviourComponent(typeof(PangatLooVillageInvaderBehaviour));
                    character.jobQueue.CancelAllJobs();
                }
            }

            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Tile Object", "DesertRose", "activated_village", providedTags: LOG_TAG.Player);
            log.AddLogToDatabase();
            PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
        }

        private void OnDayStarted() {
            int p_currentDay = GameManager.Instance.continuousDays;
            if (p_currentDay == 8) {
                StartUndeadAttack();
            }
        }

        #region Saving
        public override SaveDataWorldEvent Save() {
            SaveUndeadAttackEvent leaderEvent = new SaveUndeadAttackEvent();
            leaderEvent.Save(this);
            return leaderEvent;
        }
        #endregion
    }

    public class SaveUndeadAttackEvent : SaveDataWorldEvent {
        public override void Save(WorldEvent data) {
            base.Save(data);
            UndeadAttackEvent undeadAttackEvent = data as UndeadAttackEvent;
        }
        public override WorldEvent Load() {
            return new UndeadAttackEvent(this);
        }
    }
}