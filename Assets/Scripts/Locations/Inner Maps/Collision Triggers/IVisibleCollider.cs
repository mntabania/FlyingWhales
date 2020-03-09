using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface to be used on collision triggers if the collider it is attached to should be seen by characters.
/// In other words, if a character needs to react to an object, make its collision trigger implement this.
/// Also make sure to place the collider object on the NPCSettlement Maps Collision Layer.
/// </summary>
public interface IVisibleCollider { 
    IPointOfInterest poi { get; }

    bool IgnoresStructureDifference();
}
