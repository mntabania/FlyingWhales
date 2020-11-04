using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ECS {
    public class RaceSetting {
        public RACE race;
        public List<BodyPart> bodyParts;

        public int restRegenAmount;
        public List<CHARACTER_TAG> tags;

        internal RaceSetting CreateNewCopy() {
            RaceSetting newRaceSetting = new RaceSetting();
            newRaceSetting.race = this.race;
            newRaceSetting.bodyParts = new List<BodyPart>();
            for (int i = 0; i < this.bodyParts.Count; i++) {
                BodyPart currBodyPart = this.bodyParts[i];
                newRaceSetting.bodyParts.Add(currBodyPart.CreateNewCopy());
            }
   //         newRaceSetting.baseStr = this.baseStr;
   //         newRaceSetting.baseInt = this.baseInt;
   //         newRaceSetting.baseAgi = this.baseAgi;
   //         newRaceSetting.baseHP = this.baseHP;
			//newRaceSetting.statAllocationPoints = this.statAllocationPoints;
			//newRaceSetting.strWeightAllocation = this.strWeightAllocation;
			//newRaceSetting.intWeightAllocation = this.intWeightAllocation;
			//newRaceSetting.agiWeightAllocation = this.agiWeightAllocation;
			//newRaceSetting.hpWeightAllocation = this.hpWeightAllocation;
            newRaceSetting.restRegenAmount = this.restRegenAmount;
            newRaceSetting.tags = new List<CHARACTER_TAG>(this.tags);
            return newRaceSetting;
        }
    }
}