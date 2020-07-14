using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interrupts;

public class ActionIntel : IIntel {
    public ActualGoapNode node { get; private set; }

    #region getters
    public IReactable reactable => node;
    public Log log => node.descriptionLog;
    public Character actor => node.actor;
    public IPointOfInterest target => node.target;
    #endregion

    public ActionIntel(ActualGoapNode node) {
        this.node = node;
    }
}
public class InterruptIntel : IIntel {
    //public Interrupt interrupt { get; private set; }
    //public Character actor { get; private set; }
    //public IPointOfInterest target { get; private set; }
    //public Log effectLog { get; private set; }
    public InterruptHolder interruptHolder { get; private set; }

    #region getters
    public IReactable reactable => interruptHolder;
    public Log log => interruptHolder.effectLog;
    public Character actor => interruptHolder.actor;
    public IPointOfInterest target => interruptHolder.target;
    #endregion

    public InterruptIntel(Interrupt interrupt, Character actor, IPointOfInterest target, Log effectLog) {
        interruptHolder = ObjectPoolManager.Instance.CreateNewInterrupt();
        interruptHolder.Initialize(interrupt, actor, target, string.Empty);
        interruptHolder.SetEffectLog(effectLog);
    }
}

public interface IIntel {
    IReactable reactable { get; }
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
