﻿using System.Linq;
using Tutorial;
using UnityEngine.Profiling;
namespace Quests {
    public class IsAtTime : QuestCriteria {

        private readonly int[] _validTimes;
        
        public IsAtTime(int[] validTimes) {
            _validTimes = validTimes;
        }

        public override void Enable() {
            Messenger.AddListener(Signals.TICK_STARTED, CheckCriteria);
        }
        public override void Disable() {
            Messenger.RemoveListener(Signals.TICK_STARTED, CheckCriteria);
        }

        private void CheckCriteria() {
            Profiler.BeginSample($"Is At Time Quest");
            if (_validTimes.Contains(GameManager.Instance.Today().tick)) {
                if (hasCriteriaBeenMet == false) {
                    SetCriteriaAsMet();    
                }
            } else {
                if (hasCriteriaBeenMet) {
                    SetCriteriaAsUnMet();    
                }
            }
            Profiler.EndSample();
        }
    }
}