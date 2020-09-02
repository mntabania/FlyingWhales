using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICrimeable : IReactable {
    //string name { get; }
    string persistentID { get; }
    CRIME_TYPE crimeType { get; }
    CRIMABLE_TYPE crimableType { get; }

    //void SetCrimeType(Character actor, IPointOfInterest target, ICrimeable crime);
}
