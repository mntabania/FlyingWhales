using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interrupts;

public class ActionIntel : IIntel {
    public ActualGoapNode node { get; private set; }
    public Log log { get { return node.descriptionLog; } }
    public Character actor { get { return node.actor; } }
    public IPointOfInterest target { get { return node.poiTarget; } }

    public ActionIntel(ActualGoapNode node) {
        this.node = node;
    }
}
public class InterruptIntel : IIntel {
    public Interrupt interrupt { get; private set; }
    public Character actor { get; private set; }
    public IPointOfInterest target { get; private set; }
    public Log effectLog { get; private set; }

    public Log log { get { return effectLog; } }

    public InterruptIntel(Interrupt interrupt, Character actor, IPointOfInterest target, Log effectLog) {
        this.interrupt = interrupt;
        this.actor = actor;
        this.target = target;
        this.effectLog = effectLog;
    }
}

public interface IIntel {
    Log log { get; }
    Character actor { get; }
    IPointOfInterest target { get; }
}

//[System.Serializable]
//public class SaveDataIntel {
//    public bool hasLog;
//    public SaveDataLog intelLog;
//    public string systemType;

//    public virtual void Save(Intel intel) {
//        hasLog = intel.intelLog != null;
//        systemType = intel.GetType().ToString();
//        if (hasLog) {
//            intelLog = new SaveDataLog();
//            intelLog.Save(intel.intelLog);
//        }
//    }

//    public virtual Intel Load() {
//        Intel intel = System.Activator.CreateInstance(System.Type.GetType(systemType), this) as Intel;
//        return intel;
//    }
//}
