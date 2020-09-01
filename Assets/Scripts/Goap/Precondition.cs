using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Precondition {
    public GoapEffect goapEffect { get; private set; }
    public Func<Character, IPointOfInterest, OtherData[], JOB_TYPE, bool> condition { get; private set; }

    public Precondition(GoapEffect goapEffect, Func<Character, IPointOfInterest, OtherData[], JOB_TYPE, bool> condition) {
        this.goapEffect = goapEffect;
        this.condition = condition;
    }

    public bool CanSatisfyCondition(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType) {
        return condition(actor, target, otherData, jobType);
    }
}