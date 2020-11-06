using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[CreateAssetMenu(fileName = "New Race Data", menuName = "Scriptable Objects/Race Data")]
public class RaceData : ScriptableObject {
    public RACE race;
    public float hpMultiplier;
    public float attackMultiplier;
    public float attackSpeedMultiplier;
    public float staminaReductionMultiplier;
    public float walkSpeed;
    public float runSpeed;
    public string[] traitNames;
    public Sprite nameplateIcon;
}