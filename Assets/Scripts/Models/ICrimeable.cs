using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICrimeable : IReactable {
    //string name { get; }
    CRIME_TYPE crimeType { get; }


    //void SetCrimeType(Character actor, IPointOfInterest target, ICrimeable crime);
}
