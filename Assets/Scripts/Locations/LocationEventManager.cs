using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationEventManager {
    public NPCSettlement location { get; private set; }

    public LocationEventManager(NPCSettlement location) {
        this.location = location;
        // Messenger.AddListener(Signals.TICK_STARTED, ProcessEvents);
    }

    private void ProcessEvents() {
        List<LocationEvent> eventsData = LandmarkManager.Instance.locationEventsData;
        int chance = UnityEngine.Random.Range(0, 100);
        int currentTick = GameManager.Instance.tick;
        for (int i = 0; i < eventsData.Count; i++) {
            LocationEvent eventData = eventsData[i];
            if(currentTick == eventData.triggerTick && chance < eventData.triggerChance && eventData.triggerCondition != null) {
                if (eventData.triggerCondition(location)) {
                    eventData.TriggerEvent(location);
                }
            }
        }
    }
}
