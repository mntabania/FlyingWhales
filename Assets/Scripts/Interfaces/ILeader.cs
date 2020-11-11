using System.Collections;
using System.Collections.Generic;
using Logs;
using UnityEngine;

public interface ILeader : ISavable {
    
    int id { get; }
    string name { get; }
    RACE race { get; }
    GENDER gender { get; }
    Region currentRegion { get; }
    Region homeRegion { get; }

    //void LevelUp();
}
