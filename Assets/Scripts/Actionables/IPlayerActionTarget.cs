using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface for classes that can be targeted by player actions.
/// </summary>
public interface IPlayerActionTarget {

    List<SPELL_TYPE> actions { get; }

    void ConstructDefaultActions();
    void AddPlayerAction(SPELL_TYPE action);
    void RemovePlayerAction(SPELL_TYPE action);
    void ClearPlayerActions();
}