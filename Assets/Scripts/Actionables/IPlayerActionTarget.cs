using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface for classes that can be targeted by player actions.
/// </summary>
public interface IPlayerActionTarget {

    List<PLAYER_SKILL_TYPE> actions { get; }

    void ConstructDefaultActions();
    void AddPlayerAction(PLAYER_SKILL_TYPE action);
    void RemovePlayerAction(PLAYER_SKILL_TYPE action);
    void ClearPlayerActions();
}