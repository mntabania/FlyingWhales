using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interrupts;

public class InterruptDatabase {
    public Dictionary<string, InterruptHolder> allInterrupts { get; }

    public InterruptDatabase() {
        allInterrupts = new Dictionary<string, InterruptHolder>();
    }

    public void AddInterrupt(InterruptHolder interrupt) {
        if (!allInterrupts.ContainsKey(interrupt.persistentID)) {
            allInterrupts.Add(interrupt.persistentID, interrupt);
        }
    }
    public void RemoveInterrupt(string interrupt) {
        if (allInterrupts.ContainsKey(interrupt)) {
            allInterrupts.Remove(interrupt);
        }
    }
    public InterruptHolder GetInterruptByPersistentID(string id) {
        if (allInterrupts.ContainsKey(id)) {
            return allInterrupts[id];
        } else {
            throw new System.NullReferenceException("Trying to get an interrupt from the database with id " + id + " but the interrupt is not loaded");
        }
    }
}
