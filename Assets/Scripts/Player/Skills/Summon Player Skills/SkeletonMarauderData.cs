using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonMarauderData : SummonPlayerSkill {
    public override SPELL_TYPE type => SPELL_TYPE.SKELETON_MARAUDER;
    public override string name { get { return "Skeleton Marauder"; } }
    public override string description { get { return "Skeleton Marauder"; } }

    public SkeletonMarauderData() {
        summonType = SUMMON_TYPE.Skeleton;
        className = "Marauder";
    }
}
