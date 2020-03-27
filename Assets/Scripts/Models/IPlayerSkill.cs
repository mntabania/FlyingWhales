using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerSkill {
    int charges { get; }
    int cooldown { get; }
    int manaCost { get; }
    int currentCooldownTick { get; }
    bool hasCharges { get; }
    bool hasCooldown { get; }
    bool hasManaCost { get; }
}
