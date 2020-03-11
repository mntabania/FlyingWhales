using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intel  {
    public ActualGoapNode node { get; private set; }
    public Intel(ActualGoapNode node) {
        this.node = node;
    }
    //public Log intelLog;

    //public Intel() { }
    //public Intel(Character actor, GoapAction action) : this() { }
    //public Intel(Character actor, GoapPlan plan) : this() { }
    //public Intel(IPointOfInterest obj) : this() { }

    //public Intel(SaveDataIntel data) : this() {
    //    if (data.hasLog) {
    //        SetIntelLog(data.intelLog.Load());
    //    }
    //}
    //public void SetIntelLog(Log log) {
    //    intelLog = log;
    //}

    /// <summary>
    /// Function called when an intel reaches it's expiry date and the player has not obtained it.
    /// </summary>
    //public virtual void OnIntelExpire() { } 
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
