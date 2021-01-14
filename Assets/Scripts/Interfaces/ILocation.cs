using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public interface ILocation : ISavable {
    string locationName { get; }
}

public struct ILocationSaveData {
    public string persistentID;
    public OBJECT_TYPE objectType;
}