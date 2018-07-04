﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReleaseCharacterQuestData : CharacterQuestData {

    public enum Gain_Power_Type {
        None,
        Mentor,
        Equipment,
        Hunt
    }

    public ECS.Character targetCharacter { get; private set; }
    public int requiredPower { get; private set; }
    public Gain_Power_Type gainPowerType { get; private set; }
    public List<Vector3> vectorPathToTarget { get; private set; }

    public ReleaseCharacterQuestData(Quest parentQuest, ECS.Character owner, ECS.Character targetCharacter) : base(parentQuest, owner) {
        this.targetCharacter = targetCharacter;
        //UpdateVectorPath();
    }

    public void UpdateVectorPath() {
        PathfindingManager.Instance.GetPath(_owner.specificLocation.tileLocation, targetCharacter.specificLocation.tileLocation, OnVectorPathComputed);
    }
    private void OnVectorPathComputed(List<Vector3> path) {
        vectorPathToTarget = path;
    }

    public void SetRequiredPower(int power) {
        requiredPower = power;
    }
    public void SetGainPowerType(Gain_Power_Type gainPowerType) {
        this.gainPowerType = gainPowerType;
    }
}
