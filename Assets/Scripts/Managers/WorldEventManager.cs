using System;
using System.Collections.Generic;
using Events.World_Events;
using UnityEngine;
namespace Managers {
    public class WorldEventManager : BaseMonoBehaviour {

        public static WorldEventManager Instance;

        private List<WorldEvent> _activeEvents;

        #region getters
        public List<WorldEvent> activeEvents => _activeEvents;
        #endregion
        
        private void Awake() {
            Instance = this;
        }
        protected override void OnDestroy() {
            base.OnDestroy();
            Instance = null;
        }
        
        public void Initialize() {
            _activeEvents = new List<WorldEvent>();
            Messenger.AddListener(Signals.GAME_LOADED, OnGameLoaded);
        }
        private void OnGameLoaded() {
            Messenger.RemoveListener(Signals.GAME_LOADED, OnGameLoaded);
            ActivateEvents();
        }

        #region Events
        /// <summary>
        /// Add event as active. This event will then be activated after the game loads.
        /// </summary>
        /// <param name="worldEvent">The new World Event.</param>
        public void AddActiveEvent(WorldEvent worldEvent) {
            _activeEvents.Add(worldEvent);
        }
        private void ActivateEvents() {
            for (int i = 0; i < _activeEvents.Count; i++) {
                WorldEvent worldEvent = _activeEvents[i];
                worldEvent.InitializeEvent();
            }
        }
        #endregion

        #region Loading
        public void LoadEvent(WorldEvent worldEvent) {
            _activeEvents.Add(worldEvent);
        }
        #endregion
    }
}