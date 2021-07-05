using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISavable {
    string persistentID { get; }
    OBJECT_TYPE objectType { get; }
    System.Type serializedData { get; }
}

public interface ISavableCounterpart {
    string persistentID { get; }
    OBJECT_TYPE objectType { get; }
}

public interface IObjectPoolTester {
    bool isAssigned { get; }
}