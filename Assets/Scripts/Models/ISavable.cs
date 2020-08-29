using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISavable {
    string persistentID { get; }
    OBJECT_TYPE objectType { get; }
    System.Type serializedData { get; }
}
