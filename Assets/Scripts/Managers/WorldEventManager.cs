using System;
using System.Collections.Generic;
using Events.World_Events;
using UnityEngine;
namespace Managers {
    public class WorldEventManager : MonoBehaviour {

        public static WorldEventManager Instance;

        private List<WorldEvent> activeEvents;
        
        private void Awake() {
            Instance = this;
        }
        
        public void Initialize() {
            activeEvents = new List<WorldEvent>();
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
            activeEvents.Add(worldEvent);
        }
        private void ActivateEvents() {
            for (int i = 0; i < activeEvents.Count; i++) {
                WorldEvent worldEvent = activeEvents[i];
                worldEvent.InitializeEvent();
            }
        }
        #endregion
        
        
    }
}