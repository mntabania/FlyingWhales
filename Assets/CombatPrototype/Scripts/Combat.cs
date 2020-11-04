using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ECS{
	public enum SIDES{
		A,
		B,
	}

	public class Combat : Multithread {
//		public Dictionary<SIDES, List<ICharacter>> allCharactersAndSides;
		internal List<ICharacter> charactersSideA;
		internal List<ICharacter> charactersSideB;
		internal List<ICharacter> deadCharacters;
		internal List<ICharacter> faintedCharacters;
		internal List<ICharacter> fledCharacters;
		internal List<ICharacter> characterSideACopy;
		internal List<ICharacter> characterSideBCopy;

		internal SIDES winningSide;
		internal SIDES losingSide;

		internal List<string> resultsLog;
        internal ILocation location;
		internal bool isDone;
        internal bool hasStarted;

		public Combat(ILocation location){
//			this.allCharactersAndSides = new Dictionary<SIDES, List<ICharacter>> ();
			this.charactersSideA = new List<ICharacter> ();
			this.charactersSideB = new List<ICharacter> ();
            this.characterSideACopy = new List<ICharacter>();
            this.characterSideBCopy = new List<ICharacter>();
            this.deadCharacters = new List<ICharacter> ();
			this.faintedCharacters = new List<ICharacter> ();
			this.fledCharacters = new List<ICharacter> ();
			this.location = location;
			this.isDone = false;
            this.hasStarted = false;

			this.resultsLog = new List<string> ();
//			Messenger.AddListener<ICharacter> ("CharacterDeath", CharacterDeath);
		}

        #region ICharacter Management
        //Add a character to a side
        internal void AddCharacter(SIDES side, ICharacter character) {
            if(!this.charactersSideA.Contains(character) && !this.charactersSideB.Contains(character)) {
                int rowNumber = 1;
                if (side == SIDES.A) {
                    this.charactersSideA.Add(character);
                    this.characterSideACopy.Add(character);
                } else {
                    this.charactersSideB.Add(character);
                    this.characterSideBCopy.Add(character);
                    rowNumber = 5;
                }
                character.SetSide(side);
                character.currentCombat = this;
                character.SetRowNumber(rowNumber);
                character.actRate = character.agility * 5;
                character.battleOnlyTracker.Reset();
                if (hasStarted && !isDone) {
                    string log = character.coloredUrlName + " joins the battle on Side " + side.ToString();
                    Debug.Log(log);
                    AddCombatLog(log, side);
                }
                if (CombatPrototypeUI.Instance != null) {
                    CombatPrototypeUI.Instance.UpdateCharactersList(side);
                }
            }
        }
		internal void AddCharacters(SIDES side, List<ICharacter> characters) {
            int rowNumber = 1;
            if (side == SIDES.A) {
				this.charactersSideA.AddRange(characters);
				this.characterSideACopy.AddRange(characters);
			} else {
				this.charactersSideB.AddRange(characters);
                this.characterSideBCopy.AddRange(characters);
                rowNumber = 5;
            }
            for (int i = 0; i < characters.Count; i++) {
				characters[i].SetSide (side);
				characters[i].currentCombat = this;
                characters[i].SetRowNumber(rowNumber);
                characters[i].actRate = characters[i].agility * 5;
                if (hasStarted && !isDone) {
                    string log = characters[i].coloredUrlName + " joins the battle on Side " + side.ToString();
                    Debug.Log(log);
                    AddCombatLog(log, side);
                }
            }
            if (CombatPrototypeUI.Instance != null){
				CombatPrototypeUI.Instance.UpdateCharactersList(side);
			}
		}
        //Remove a character from a side
        internal void RemoveCharacter(SIDES side, ICharacter character) {
            if (side == SIDES.A) {
                this.charactersSideA.Remove(character);
            } else {
                this.charactersSideB.Remove(character);
            }
			character.currentCombat = null;
			if (CombatPrototypeUI.Instance != null) {
				CombatPrototypeUI.Instance.UpdateCharactersList (side);
			}
        }
        //Remove character without specifying a side
        internal void RemoveCharacter(ICharacter character) {
            if (this.charactersSideA.Remove(character)) {
				character.currentCombat = null;
				if (CombatPrototypeUI.Instance != null) {
					CombatPrototypeUI.Instance.UpdateCharactersList (SIDES.A);
				}
            } else {
                this.charactersSideB.Remove(character);
				character.currentCombat = null;
				if (CombatPrototypeUI.Instance != null) {
					CombatPrototypeUI.Instance.UpdateCharactersList (SIDES.B);
				}
            }
        }
        internal List<ICharacter> GetCharactersOnSide(SIDES side) {
            if (side == SIDES.A) {
                return charactersSideA;
            } else {
                return charactersSideB;
            }
        }
        #endregion

        #region Overrides
        public override void DoMultithread() {
            base.DoMultithread();
            CombatSimulation();
        }
        public override void FinishMultithread() {
            base.FinishMultithread();
            ReturnCombatResults();
        }
        #endregion
        public void ReturnCombatResults(){
			CombatManager.Instance.CombatResults(this);
            //if (attacker != null) {
            //    attacker.ReturnCombatResults(this);
            //}
            //if (defender != null) {
            //    defender.ReturnCombatResults(this);
            //}
		}

        //This simulates the whole combat system
		public void CombatSimulation(){
            CombatManager.Instance.StartCoroutine(CombatSimulationCoroutine());
        }

        private IEnumerator CombatSimulationCoroutine() {
            ClearCombatLogs();
            AddCombatLog("Combat starts", SIDES.A);
            hasStarted = true;
            string sideAChars = string.Empty;
            string sideBChars = string.Empty;
            for (int i = 0; i < charactersSideA.Count; i++) {
                sideAChars += charactersSideA[i].name + "\n";
            }
            for (int i = 0; i < charactersSideB.Count; i++) {
                sideBChars += charactersSideB[i].name + "\n";
            }
            Debug.Log("Starting Side A : \n" + sideAChars);
            Debug.Log("Starting Side B : \n" + sideBChars);
            Dictionary<ICharacter, int> characterActivationWeights = new Dictionary<ICharacter, int>();
            //SetRowNumber (this.charactersSideA, 1);
            //SetRowNumber (this.charactersSideB, 5);

            int rounds = 1;
            while (this.charactersSideA.Count > 0 && this.charactersSideB.Count > 0) {
                Debug.Log("========== Round " + rounds.ToString() + " ==========");
                ICharacter characterThatWillAct = GetCharacterThatWillAct(characterActivationWeights, this.charactersSideA, this.charactersSideB);
                ICharacter targetCharacter = GetTargetCharacter(characterThatWillAct, null);

                Character actingCharacter = null;
                if(characterThatWillAct is Character) {
                    actingCharacter = characterThatWillAct as Character;
                }
                Debug.Log((actingCharacter != null ? actingCharacter.characterClass.className : "") + characterThatWillAct.name + " will act. (hp lost: " + characterThatWillAct.battleOnlyTracker.hpLostPercent
                    + ", last damage taken: " + characterThatWillAct.battleOnlyTracker.lastDamageTaken);
                Debug.Log((targetCharacter is Character ? (targetCharacter as Character).characterClass.className : "") + targetCharacter.name + " is the target. (hp lost: " + targetCharacter.battleOnlyTracker.hpLostPercent
                    + ", last damage taken: " + targetCharacter.battleOnlyTracker.lastDamageTaken);

                if (actingCharacter != null) {
                    actingCharacter.EnableDisableSkills(this);
                }
                //Debug.Log("Available Skills: ");
                //for (int i = 0; i < characterThatWillAct.skills.Count; i++) {
                //    Skill currSkill = characterThatWillAct.skills[i];
                //    if (currSkill.isEnabled) {
                //        Debug.Log(currSkill.skillName);
                //    }
                //}
                Skill skillToUse = GetSkillToUse(characterThatWillAct, targetCharacter);
                if (skillToUse != null) {
                    Debug.Log(characterThatWillAct.name + " decides to use " + skillToUse.skillName);
                    if (actingCharacter != null) {
                        actingCharacter.CureStatusEffects();
                    }
                    //ICharacter targetCharacter = GetTargetCharacter(characterThatWillAct, skillToUse);
                    Debug.Log(characterThatWillAct.name + " decides to use it on " + targetCharacter.name);
                    DoSkill(skillToUse, characterThatWillAct, targetCharacter);
                }
                if (CombatPrototypeUI.Instance != null) {
                    CombatPrototypeUI.Instance.UpdateCharacterSummary();
                }
                Debug.Log("========== End Round " + rounds.ToString() + " ==========");
                rounds++;
                yield return new WaitWhile(() => GameManager.Instance.isPaused == true);
                yield return new WaitForSeconds(CombatManager.Instance.updateIntervals);
            }
            if (this.charactersSideA.Count > 0) {
                winningSide = SIDES.A;
                losingSide = SIDES.B;
            } else {
                winningSide = SIDES.B;
                losingSide = SIDES.A;
            }
            AddCombatLog("Combat Ends", SIDES.A);
            sideAChars = string.Empty;
            sideBChars = string.Empty;
            for (int i = 0; i < charactersSideA.Count; i++) {
                sideAChars += charactersSideA[i].name + "\n";
            }
            for (int i = 0; i < charactersSideB.Count; i++) {
                sideBChars += charactersSideB[i].name + "\n";
            }
            Debug.Log("Side A : \n" + sideAChars);
            Debug.Log("Side B : \n" + sideBChars);
            isDone = true;
            ReturnCombatResults();
        }

		//Set row number to a list of characters
		private void SetRowNumber(List<ICharacter> characters, int rowNumber){
			for (int i = 0; i < characters.Count; i++) {
				characters [i].SetRowNumber(rowNumber);
			}
		}

		//Return a character that will act from a pool of characters based on their act rate
		private ICharacter GetCharacterThatWillAct(Dictionary<ICharacter, int> characterActivationWeights, List<ICharacter> charactersSideA, List<ICharacter> charactersSideB){
			characterActivationWeights.Clear();
            for (int i = 0; i < charactersSideA.Count; i++) {
                characterActivationWeights.Add(charactersSideA[i], charactersSideA[i].actRate);
            }
            for (int i = 0; i < charactersSideB.Count; i++) {
                characterActivationWeights.Add(charactersSideB[i], charactersSideB[i].actRate);
            }

            ICharacter chosenCharacter = Utilities.PickRandomElementWithWeights<ICharacter>(characterActivationWeights);
			foreach (ICharacter character in characterActivationWeights.Keys) {
				character.actRate += character.baseAgility;
			}
			chosenCharacter.actRate = chosenCharacter.agility * 5;
			return chosenCharacter;
		}

		//Get a random character from the opposite side to be the target
		private ICharacter GetTargetCharacter(ICharacter sourceCharacter, Skill skill){
			List<ICharacter> possibleTargets = new List<ICharacter>();

            if (skill is AttackSkill) {
                List<ICharacter> oppositeTargets = this.charactersSideB;
                if (sourceCharacter.currentSide == SIDES.B) {
                    oppositeTargets = this.charactersSideA;
                }
                possibleTargets.AddRange(oppositeTargets);
            } else {
                possibleTargets.Add(sourceCharacter);
            }

                //            if (skill is AttackSkill) {
                //				List<ICharacter> oppositeTargets = this.charactersSideB;
                //				if(sourceCharacter.currentSide == SIDES.B){
                //					oppositeTargets = this.charactersSideA;
                //				}

                //				//int chance = Utilities.rng.Next(0, 100);
                //				//if(sourceCharacter.HasTag(CHARACTER_TAG.MILD_PSYTOXIN)){
                //				//	if(chance < 10){
                //				//		if(sourceCharacter.currentSide == SIDES.A){
                //				//			oppositeTargets = this.charactersSideA;
                //				//		}else{
                //				//			oppositeTargets = this.charactersSideB;
                //				//		}
                //				//	}
                //				//}else if(sourceCharacter.HasTag(CHARACTER_TAG.MODERATE_PSYTOXIN)){
                //				//	if(chance < 20){
                //				//		if(sourceCharacter.currentSide == SIDES.A){
                //				//			oppositeTargets = this.charactersSideA;
                //				//		}else{
                //				//			oppositeTargets = this.charactersSideB;
                //				//		}
                //				//	}
                //				//}

                //				for (int i = 0; i < oppositeTargets.Count; i++) {
                //					ICharacter targetCharacter = oppositeTargets [i];
                //					int rowDistance = GetRowDistanceBetweenTwoCharacters (sourceCharacter, targetCharacter);
                //					if (skill.range >= rowDistance) {
                //						possibleTargets.Add (targetCharacter);
                //					}
                //				}
                //			} else if (skill is HealSkill) {
                //				List<ICharacter> sameTargets = this.charactersSideB;
                //				if(sourceCharacter.currentSide == SIDES.A){
                //					sameTargets = this.charactersSideA;
                //				}

                //				//int chance = Utilities.rng.Next (0, 100);
                //				//if(sourceCharacter.HasTag(CHARACTER_TAG.MILD_PSYTOXIN)){
                //				//	if(chance < 10){
                //				//		if(sourceCharacter.currentSide == SIDES.B){
                //				//			sameTargets = this.charactersSideA;
                //				//		}else{
                //				//			sameTargets = this.charactersSideB;
                //				//		}
                //				//	}
                //				//}else if(sourceCharacter.HasTag(CHARACTER_TAG.MODERATE_PSYTOXIN)){
                //				//	if(chance < 20){
                //				//		if(sourceCharacter.currentSide == SIDES.B){
                //				//			sameTargets = this.charactersSideA;
                //				//		}else{
                //				//			sameTargets = this.charactersSideB;
                //				//		}
                //				//	}
                //				//}

                //				for (int i = 0; i < sameTargets.Count; i++) {
                //					ICharacter targetCharacter = sameTargets [i];
                //					int rowDistance = GetRowDistanceBetweenTwoCharacters (sourceCharacter, targetCharacter);
                //					if (skill.range >= rowDistance) {
                //						possibleTargets.Add (targetCharacter);
                //					}
                //				}
                ////				if (sourceCharacter.currentSide == SIDES.A) {
                ////					for (int i = 0; i < this.charactersSideA.Count; i++) {
                ////						ICharacter targetCharacter = this.charactersSideA [i];
                ////						int rowDistance = GetRowDistanceBetweenTwoCharacters (sourceCharacter, targetCharacter);
                ////						if (skill.range >= rowDistance) {
                ////							possibleTargets.Add (targetCharacter);
                ////						}
                ////					}
                ////				} else {
                ////					for (int i = 0; i < this.charactersSideB.Count; i++) {
                ////						ICharacter targetCharacter = this.charactersSideB [i];
                ////						int rowDistance = GetRowDistanceBetweenTwoCharacters (sourceCharacter, targetCharacter);
                ////						if (skill.range >= rowDistance) {
                ////							possibleTargets.Add (targetCharacter);
                ////						}
                ////					}
                ////				}
                //			}else{
                //				possibleTargets.Add (sourceCharacter);
                //			}

                return possibleTargets [Utilities.rng.Next (0, possibleTargets.Count)];
		}

		//Get Skill that the character will use based on activation weights, target character must be within skill range
		private Skill GetSkillToUse(ICharacter sourceCharacter, ICharacter targetCharacter = null){
            Debug.Log("Available Skills: ");
            Dictionary<Skill, int> skillActivationWeights = new Dictionary<Skill, int> ();
            for (int i = 0; i < sourceCharacter.skills.Count; i++) { //These are general skills like move, flee, drink potion
                Skill skill = sourceCharacter.skills[i];
                if (skill.isEnabled) {
                    Debug.Log(skill.skillName);
                    skillActivationWeights.Add(skill, skill.activationWeight);
                }
            }

            if (sourceCharacter.battleOnlyTracker.lastDamageTaken < sourceCharacter.currentHP) { //sourceCharacter last damage taken must not be >= current health
                float weaponAttack = 0f;
                float missingHP = (1f - ((float) sourceCharacter.currentHP / (float) sourceCharacter.maxHP)) * 100f;
                int levelDiff = (sourceCharacter.level - targetCharacter.level) * 10;
                if (sourceCharacter is Character) {
                    Character character = sourceCharacter as Character;
                    if (character.equippedWeapon != null) {//character must have a weapon
                        weaponAttack = character.equippedWeapon.attackPower;
                        for (int i = 0; i < character.level; i++) {
                            for (int j = 0; j < character.characterClass.skillsPerLevel[i].Length; j++) {
                                Skill skill = character.characterClass.skillsPerLevel[i][j];
                                if (skill.isEnabled && skill.skillType == SKILL_TYPE.ATTACK) {
                                    Debug.Log(skill.skillName);
                                    AttackSkill attackSkill = skill as AttackSkill;
                                    float initialWeight = GetSkillInitialWeight(sourceCharacter, targetCharacter, attackSkill, weaponAttack, missingHP, levelDiff, character.battleTracker);
                                    float specialModifier = GetSpecialModifier(sourceCharacter, targetCharacter, attackSkill, character.battleTracker);
                                    float finalWeight = initialWeight * (specialModifier / 100f);
                                    skillActivationWeights.Add(attackSkill, Mathf.CeilToInt(finalWeight));
                                }
                            }
                        }
                    }
                } else if (sourceCharacter is Monster) {
                    Monster monster = sourceCharacter as Monster;
                    weaponAttack = monster.attackPower;
                    for (int i = 0; i < monster.skills.Count; i++) {
                        Skill skill = monster.skills[i];
                        if (skill.isEnabled && skill.skillType == SKILL_TYPE.ATTACK) {
                            Debug.Log(skill.skillName);
                            AttackSkill attackSkill = skill as AttackSkill;
                            float initialWeight = GetSkillInitialWeight(sourceCharacter, targetCharacter, attackSkill, weaponAttack, missingHP, levelDiff);
                            float specialModifier = GetSpecialModifier(sourceCharacter, targetCharacter, attackSkill);
                            float finalWeight = initialWeight * (specialModifier / 100f);
                            skillActivationWeights.Add(attackSkill, Mathf.CeilToInt(finalWeight));
                        }
                    }
                }
            }
            if (skillActivationWeights.Count > 0) {
                Skill chosenSkill = Utilities.PickRandomElementWithWeights<Skill>(skillActivationWeights);
                return chosenSkill;
            }
            return null;

            //Dictionary<object, int> categoryActivationWeights = new Dictionary<object, int> ();

            ////First step: pick from general skills or body part skill or weapon skill
            //int bodyPartWeight = 10;
            ////if(sourceCharacter.HasActivatableWeaponSkill()){
            ////	categoryActivationWeights.Add("weapon", 100);
            ////}else{
            ////	bodyPartWeight += 100;
            ////}
            ////if (sourceCharacter.HasActivatableBodyPartSkill ()) {
            ////	categoryActivationWeights.Add ("bodypart", bodyPartWeight);
            ////}

            //for (int i = 0; i < sourceCharacter.skills.Count; i++) {
            //	Skill skill = sourceCharacter.skills [i];
            //	if(skill.isEnabled && skill.skillCategory == SKILL_CATEGORY.GENERAL){
            //		int activationWeight = GetActivationWeightOfSkill (sourceCharacter, skill);
            //		if(skill is MoveSkill && HasTargetInRangeForSkill(SKILL_TYPE.ATTACK, sourceCharacter)){
            //			activationWeight /= 2;
            //		}else if(skill is FleeSkill){
            //			activationWeight = 50 - (sourceCharacter.currentHP / ((int)(sourceCharacter.maxHP * 0.01f)));
            //			if(activationWeight == 0){
            //				activationWeight = 1;
            //			}
            //			activationWeight *= 2;
            //		}
            //		if(activationWeight > 0){
            //			categoryActivationWeights.Add (skill, activationWeight);
            //		}
            //	}
            //}
            //if(categoryActivationWeights.Count > 0){
            //	object chosenObject = Utilities.PickRandomElementWithWeights<object> (categoryActivationWeights);
            //	if(chosenObject is string){
            //		string chosenCategory = chosenObject.ToString ();
            //		if(chosenCategory == "bodypart"){
            //			for (int i = 0; i < sourceCharacter.skills.Count; i++) {
            //				Skill skill = sourceCharacter.skills [i];
            //				if(skill.isEnabled && skill.skillCategory == SKILL_CATEGORY.BODY_PART && HasTargetInRangeForSkill(skill, sourceCharacter)){
            //					int activationWeight = GetActivationWeightOfSkill (sourceCharacter, skill);
            //					skillActivationWeights.Add (skill, activationWeight);
            //				}
            //			}
            //		}else if(chosenCategory == "weapon"){
            //			for (int i = 0; i < sourceCharacter.skills.Count; i++) {
            //				Skill skill = sourceCharacter.skills [i];
            //				if(skill.isEnabled && skill.skillCategory == SKILL_CATEGORY.WEAPON && HasTargetInRangeForSkill(skill, sourceCharacter)){
            //					int activationWeight = GetActivationWeightOfSkill (sourceCharacter, skill);
            //					skillActivationWeights.Add (skill, activationWeight);
            //				}
            //			}
            //		}
            //		if(skillActivationWeights.Count > 0){
            //			Skill chosenSkill = Utilities.PickRandomElementWithWeights<Skill> (skillActivationWeights);
            //			return chosenSkill;
            //		}
            //	}else{
            //		return (Skill)chosenObject;
            //	}
            //}
            //return null;
        }

        private float GetSkillInitialWeight(ICharacter sourceCharacter, ICharacter targetCharacter, AttackSkill attackSkill, float weaponAttack, float missingHP, int levelDiff, CharacterBattleTracker battleTracker = null) {
            int statUsed = attackSkill.attackCategory == ATTACK_CATEGORY.PHYSICAL ? sourceCharacter.strength : sourceCharacter.intelligence;
            int finalAttack = GetFinalAttack(statUsed, sourceCharacter.level, weaponAttack);
            float rawDamage = ((float) finalAttack * ((float) attackSkill.power / 100f)) * 1; //Subject to change: the *1 is the targets that will hit, this will change in the future

            float modifier = GetModifier(sourceCharacter, targetCharacter, attackSkill, rawDamage, missingHP, levelDiff, battleTracker);
            float spModifier = sourceCharacter.currentSP - attackSkill.spCost;
            if (spModifier <= 0f) { spModifier = 1f; }
            return (rawDamage * (1f + (modifier / 100f))) / spModifier;
        }
        private float GetModifier(ICharacter sourceCharacter, ICharacter targetCharacter, AttackSkill attackSkill, float rawDamage, float missingHP, int levelDiff, CharacterBattleTracker battleTracker = null) {
            //Elemental Weakness
            float elementalWeakness = 0f;
            if (attackSkill.element != ELEMENT.NONE && battleTracker != null) {
                if (battleTracker.tracker.ContainsKey(targetCharacter.name)) {
                    if (battleTracker.tracker[targetCharacter.name].elementalWeaknesses.Contains(attackSkill.element)) {
                        elementalWeakness = targetCharacter.elementalWeaknesses[attackSkill.element];
                    }
                }
            }

            //Missing HP from parameter
            //Level Difference from parameter

            //HP Lost
            float hpLostPercent = sourceCharacter.battleOnlyTracker.hpLostPercent;

            //Expected Value
            float previousActualDamage = battleTracker != null && battleTracker.tracker.ContainsKey(targetCharacter.name) ? battleTracker.tracker[targetCharacter.name].lastDamageDealt : 0f;
            float expectedValue = previousActualDamage / (rawDamage + previousActualDamage);

            return elementalWeakness + missingHP + levelDiff + hpLostPercent + expectedValue;
        }
        private float GetSpecialModifier(ICharacter sourceCharacter, ICharacter targetCharacter, AttackSkill attackSkill, CharacterBattleTracker battleTracker = null) {
            //Attack misses per skill
            float attackMissPercent = (-33.4f) * (float)(sourceCharacter.battleOnlyTracker.consecutiveAttackMisses.ContainsKey(attackSkill.skillName) ? sourceCharacter.battleOnlyTracker.consecutiveAttackMisses[attackSkill.skillName] : 0);

            //Elemental Resist
            float resistance = 0f;
            if (attackSkill.element != ELEMENT.NONE && battleTracker != null) {
                if (battleTracker.tracker.ContainsKey(targetCharacter.name)) {
                    if (battleTracker.tracker[targetCharacter.name].elementalResistances.Contains(attackSkill.element)) {
                        resistance = targetCharacter.elementalResistances[attackSkill.element];
                    }
                }
            }
            float elementalResistance = (100f - resistance) / 100f;

            //Will die next hit
            float dieNextHit = sourceCharacter.currentHP - sourceCharacter.battleOnlyTracker.lastDamageTaken;

            return attackMissPercent + resistance + elementalResistance + dieNextHit;
        }

        //Returns activation weight of a certain skill that is already modified
   //     private int GetActivationWeightOfSkill(ICharacter sourceCharacter, Skill skill){
			//int activationWeight = skill.activationWeight;
			////if(skill.actWeightType == ACTIVATION_WEIGHT_TYPE.CURRENT_HEALTH){
			////	activationWeight *= ((int)(((float)sourceCharacter.currentHP / (float)sourceCharacter.maxHP) * 100f));
			////}else if(skill.actWeightType == ACTIVATION_WEIGHT_TYPE.MISSING_HEALTH){
			////	int missingHealth = sourceCharacter.maxHP - sourceCharacter.currentHP;
			////	int weight = (int)(((float)missingHealth / (float)sourceCharacter.maxHP) * 100f);
			////	activationWeight *=  (weight > 0f ? weight : 1);
			////}else if(skill.actWeightType == ACTIVATION_WEIGHT_TYPE.ALLY_MISSING_HEALTH){
			////	int highestMissingHealth = 0;
			////	ICharacter chosenCharacter = null;
			////	if(sourceCharacter.currentSide == SIDES.A){
			////		for (int j = 0; j < charactersSideA.Count; j++) {
			////			ICharacter character = charactersSideA [j];
			////			int missingHealth = character.maxHP - character.currentHP;
			////			if(chosenCharacter == null){
			////				highestMissingHealth = missingHealth;
			////				chosenCharacter = character;
			////			}else{
			////				if(missingHealth > highestMissingHealth){
			////					highestMissingHealth = missingHealth;
			////					chosenCharacter = character;
			////				}
			////			}
			////		}
			////	}else{
			////		for (int j = 0; j < charactersSideB.Count; j++) {
			////			ICharacter character = charactersSideB [j];
			////			int missingHealth = character.maxHP - character.currentHP;
			////			if(chosenCharacter == null){
			////				highestMissingHealth = missingHealth;
			////				chosenCharacter = character;
			////			}else{
			////				if(missingHealth > highestMissingHealth){
			////					highestMissingHealth = missingHealth;
			////					chosenCharacter = character;
			////				}
			////			}
			////		}
			////	}
			////	if(chosenCharacter != null){
			////		int weight = (int)((((float)highestMissingHealth / (float)chosenCharacter.maxHP) * 100f) * 2f);
			////		activationWeight *= (weight > 0f ? weight : 1);
			////	}
			////}
			//return activationWeight;
		//}
		//Check if there are targets in range for the specific skill so that the character can know if the skill can be activated 
		internal bool HasTargetInRangeForSkill(Skill skill, ICharacter sourceCharacter){
			if(skill is AttackSkill){
				if(sourceCharacter.currentSide == SIDES.A){
					for (int i = 0; i < this.charactersSideB.Count; i++) {
						ICharacter targetCharacter = this.charactersSideB [i];
						int rowDistance = GetRowDistanceBetweenTwoCharacters (sourceCharacter, targetCharacter);
						if(skill.range >= rowDistance){
							return true;
						}
					}
				}else{
					for (int i = 0; i < this.charactersSideA.Count; i++) {
						ICharacter targetCharacter = this.charactersSideA [i];
						int rowDistance = GetRowDistanceBetweenTwoCharacters (sourceCharacter, targetCharacter);
						if(skill.range >= rowDistance){
							return true;
						}
					}
				}
				return false;
			}else{
				return true;
			}

		}

		internal bool HasTargetInRangeForSkill(SKILL_TYPE skillType, ICharacter sourceCharacter){
			if(skillType == SKILL_TYPE.ATTACK){
				for (int i = 0; i < sourceCharacter.skills.Count; i++) {
					Skill skill = sourceCharacter.skills [i];
					if(skill is AttackSkill){
                        return HasTargetInRangeForSkill(skill, sourceCharacter);
					}
				}
                if (sourceCharacter is Character) {
                    Character character = sourceCharacter as Character;
                    for (int i = 0; i < character.level; i++) {
                        for (int j = 0; j < character.characterClass.skillsPerLevel[i].Length; j++) {
                            Skill skill = character.characterClass.skillsPerLevel[i][j];
                            if (skill is AttackSkill) {
                                return HasTargetInRangeForSkill(skill, sourceCharacter);
                            }
                        }
                    }
                }
			}
			return true;
		}
		//Returns the row distance/difference of two characters
		private int GetRowDistanceBetweenTwoCharacters(ICharacter sourceCharacter, ICharacter targetCharacter){
			int distance = targetCharacter.currentRow - sourceCharacter.currentRow;
			if(distance < 0){
				distance *= -1;
			}
			return distance;
		}
		//ICharacter will do the skill specified, but its success will be determined by the skill's accuracy
		private void DoSkill(Skill skill, ICharacter sourceCharacter, ICharacter targetCharacter){
            //If skill is attack, reduce sp
            if (skill.skillType == SKILL_TYPE.ATTACK) {
                AttackSkill attackSkill = skill as AttackSkill;
                sourceCharacter.AdjustSP(-attackSkill.spCost);
            }
            SuccessfulSkill(skill, sourceCharacter, targetCharacter);

   //         int chance = Utilities.rng.Next (0,100);
			//if(chance < skill.accuracy){
			//	//Successful
			//	SuccessfulSkill(skill, sourceCharacter, targetCharacter);
			//}else{
			//	//Fail
			//	FailedSkill(skill, sourceCharacter, targetCharacter);
			//}
		}

		//Go here if skill is accurate and is successful
		private void SuccessfulSkill(Skill skill, ICharacter sourceCharacter, ICharacter targetCharacter){
			if (skill is AttackSkill) {
				AttackSkill (skill, sourceCharacter, targetCharacter);
			} else if (skill is HealSkill) {
				HealSkill (skill, sourceCharacter, targetCharacter);
			} else if (skill is FleeSkill) {
				FleeSkill (sourceCharacter, targetCharacter);
			} else if (skill is ObtainSkill) {
				ObtainItemSkill (sourceCharacter, targetCharacter);
			} else if (skill is MoveSkill) {
				MoveSkill (skill, sourceCharacter, targetCharacter);
			}
		}

		//Skill is not accurate and therefore has failed to execute
		private void FailedSkill(Skill skill, ICharacter sourceCharacter, ICharacter targetCharacter){
			//TODO: What happens when a skill has failed?
			if(skill is FleeSkill){
				AddCombatLog(sourceCharacter.coloredUrlName + " tried to flee but got tripped over and fell down!", sourceCharacter.currentSide);
//				CounterAttack (targetCharacter);
			}else if(skill is AttackSkill){
                sourceCharacter.battleOnlyTracker.AddAttackMiss(skill.skillName, 1);
				AddCombatLog (sourceCharacter.coloredUrlName + " tried to " + skill.skillName.ToLower() + " " + targetCharacter.coloredUrlName + " but missed!", sourceCharacter.currentSide);
			}else if(skill is HealSkill){
				if(sourceCharacter == targetCharacter){
					AddCombatLog (sourceCharacter.coloredUrlName + " tried to use " + skill.skillName.ToLower() + " to heal himself/herself but it is already expired!", sourceCharacter.currentSide);
				}else{
					AddCombatLog (sourceCharacter.coloredUrlName + " tried to use " + skill.skillName.ToLower() + " to heal " + targetCharacter.coloredUrlName + " but it is already expired!", sourceCharacter.currentSide);
				}
			}
		}

		//Get DEFEND_TYPE for the attack skill, if DEFEND_TYPE is NONE, then target character has not defend successfully, therefore, the target character will be damaged
		//private DEFEND_TYPE CanTargetCharacterDefend(ICharacter sourceCharacter, ICharacter targetCharacter){
		//	if(sourceCharacter.HasStatusEffect(STATUS_EFFECT.CONFUSED)){
		//		return DEFEND_TYPE.NONE;
		//	}
		//	int dodgeChance = Utilities.rng.Next (0, 100);
		//	if(dodgeChance < targetCharacter.dodgeRate){
		//		return DEFEND_TYPE.DODGE;
		//	}else{
		//		int parryChance = Utilities.rng.Next (0, 100);
		//		if(parryChance < targetCharacter.parryRate){
		//			return DEFEND_TYPE.PARRY;
		//		}else{
		//			int blockChance = Utilities.rng.Next (0, 100);
		//			if(blockChance < targetCharacter.blockRate){
		//				return DEFEND_TYPE.BLOCK;
		//			}else{
		//				return DEFEND_TYPE.NONE;
		//			}
		//		}
		//	}
		//}

		private void CounterAttack(ICharacter character){
			//TODO: Counter attack
			AddCombatLog (character.coloredUrlName + " counterattacked!", character.currentSide);
		}

		private void InstantDeath(ICharacter character){
			character.FaintOrDeath();
		}

		#region Attack Skill
		private void AttackSkill(Skill skill, ICharacter sourceCharacter, ICharacter targetCharacter){
			AttackSkill attackSkill = skill as AttackSkill;
            HitTargetCharacter(attackSkill, sourceCharacter, targetCharacter); //, attackSkill.weapon
   //         DEFEND_TYPE defendType = CanTargetCharacterDefend (sourceCharacter, targetCharacter);
			//if(defendType == DEFEND_TYPE.NONE){
			//	//Successfully hits the target character
			//	HitTargetCharacter(attackSkill, sourceCharacter, targetCharacter); //, attackSkill.weapon
			//}else{
			//	//Target character has defend successfully and will roll for counter attack
			//	if(defendType == DEFEND_TYPE.DODGE){
			//		AddCombatLog(targetCharacter.coloredUrlName + " dodged " + sourceCharacter.coloredUrlName + "'s " + attackSkill.skillName.ToLower() + ".", targetCharacter.currentSide);
			//	} else if(defendType == DEFEND_TYPE.BLOCK){
			//		AddCombatLog(targetCharacter.coloredUrlName + " blocked " + sourceCharacter.coloredUrlName + "'s " + attackSkill.skillName.ToLower() + ".", targetCharacter.currentSide);
			//	} else if(defendType == DEFEND_TYPE.PARRY){
			//		AddCombatLog(targetCharacter.coloredUrlName + " parried " + sourceCharacter.coloredUrlName + "'s " + attackSkill.skillName.ToLower() + ".", targetCharacter.currentSide);
			//	}
			//	CounterAttack(targetCharacter);
			//}
		}
			
		//Hits the target with an attack skill
		private void HitTargetCharacter(AttackSkill attackSkill, ICharacter sourceCharacter, ICharacter targetCharacter){
            //Total Damage = [Weapon Power + (Int or Str)] - [Base Damage Mitigation] - [Bonus Attack Type Mitigation] + [Bonus Attack Type Weakness]
            //if(sourceCharacter.HasStatusEffect(STATUS_EFFECT.CONFUSED)){
            //	targetCharacter = sourceCharacter;
            //	string confusedLog = sourceCharacter.coloredUrlName + " is so confused that " + (sourceCharacter.gender == GENDER.MALE ? "he" : "she") + " targeted " 
            //		+ (sourceCharacter.gender == GENDER.MALE ? "himself" : "herself");
            //	AddCombatLog (confusedLog, sourceCharacter.currentSide);
            //}

            //Reset attack miss
            sourceCharacter.battleOnlyTracker.ResetAttackMiss(attackSkill.skillName);

            string log = string.Empty;
            Character attacker = null;
            Weapon weapon = null;
            float weaponAttack = 0f;
            float damageRange = 0f;
            int statMod = sourceCharacter.strength;
            int def = targetCharacter.GetPDef(sourceCharacter);
            float critDamage = 100f;
            if (sourceCharacter is Character) {
                attacker = sourceCharacter as Character;
                weapon = attacker.equippedWeapon;
                weaponAttack = weapon.attackPower;
            } else if (sourceCharacter is Monster) {
                weaponAttack = (sourceCharacter as Monster).attackPower;
            }
            if (attackSkill.attackCategory == ATTACK_CATEGORY.MAGICAL) {
                statMod = sourceCharacter.intelligence;
                def = targetCharacter.GetMDef(sourceCharacter);
            }
            int critChance = Utilities.rng.Next(0, 100);
            if(critChance < sourceCharacter.critChance) {
                //CRITICAL HIT!
                critDamage = 200f + sourceCharacter.critDamage;
            }
            BodyPart chosenBodyPart = GetRandomBodyPart(targetCharacter);
            if (chosenBodyPart == null) {
                Debug.LogError("NO MORE BODY PARTS!");
                return;
            }
            Armor armor = chosenBodyPart.GetArmor();
            log += sourceCharacter.coloredUrlName + " " + attackSkill.skillName.ToLower() + " " + targetCharacter.coloredUrlName + " in the " + chosenBodyPart.name.ToLower();

            if (weapon != null) {
                damageRange = ItemManager.Instance.weaponTypeData[weapon.weaponType].damageRange;

                log += " with " + (sourceCharacter.gender == GENDER.MALE ? "his" : "her") + " " + weapon.itemName + ".";

            } else {
                log += ".";
            }

            int finalAttack = GetFinalAttack(statMod, sourceCharacter.level, weaponAttack);
            int damage = (int) (((float)finalAttack * (attackSkill.power / 100f)) * (critDamage / 100f));
            int computedDamageRange = (int) ((float) damage * (damageRange / 100f));
            int minDamageRange = damage - computedDamageRange;
            int maxDamageRange = damage + computedDamageRange;
            damage = Utilities.rng.Next((minDamageRange < 0 ? 0 : minDamageRange), maxDamageRange + 1);

            //Reduce damage by defense of target
            damage -= def;

            //Calculate elemental weakness and resistance
            //Use element of skill if it has one, if not, use weapon element instead if it has one
            ELEMENT elementUsed = ELEMENT.NONE;
            if(attackSkill.element != ELEMENT.NONE) {
                elementUsed = attackSkill.element;
            } else {
                if(weapon != null && weapon.element != ELEMENT.NONE) {
                    elementUsed = weapon.element;
                }
            }
            float elementalDiff = targetCharacter.elementalWeaknesses[elementUsed] - targetCharacter.elementalResistances[elementUsed];
            float elementModifier = 1f + ((elementalDiff < 0f ? 0f : elementalDiff) / 100f);
            //Add elemental weakness and resist to sourceCharacter battle tracker
            if(attacker != null) {
                if(targetCharacter.elementalWeaknesses[elementUsed] > 0f) {
                    attacker.battleTracker.AddEnemyElementalWeakness(targetCharacter.name, elementUsed);
                }
                if (targetCharacter.elementalResistances[elementUsed] > 0f) {
                    attacker.battleTracker.AddEnemyElementalResistance(targetCharacter.name, elementUsed);
                }
            }

            //Calculate total damage
            damage = (int) ((float) damage * elementModifier);

            log += "(" + damage.ToString() + ")";

            DealDamageToBodyPart(attackSkill, targetCharacter, sourceCharacter, chosenBodyPart, ref log);

            AddCombatLog(log, sourceCharacter.currentSide);

            int previousCurrentHP = targetCharacter.currentHP;
            targetCharacter.AdjustHP(-damage);

            //Add HP Lost
            int lastDamageTaken = previousCurrentHP - targetCharacter.currentHP;
            float hpLost = ((float) lastDamageTaken / (float) targetCharacter.maxHP) * 100f;
            targetCharacter.battleOnlyTracker.hpLostPercent += hpLost;
            targetCharacter.battleOnlyTracker.lastDamageTaken = lastDamageTaken;

            //Add previous actual damage
            if (attacker != null) {
                attacker.battleTracker.SetLastDamageDealt(targetCharacter.name, damage);
            }

                //if (attackSkill.attackType != ATTACK_TYPE.STATUS) {
                //    string log = string.Empty;
                //    float weaponPower = 0f;
                //    float damageRange = 5f;

                //    BodyPart chosenBodyPart = GetRandomBodyPart(targetCharacter);
                //    if (chosenBodyPart == null) {
                //        Debug.LogError("NO MORE BODY PARTS!");
                //        return;
                //    }
                //    Armor armor = chosenBodyPart.GetArmor();
                //    log += sourceCharacter.coloredUrlName + " " + attackSkill.skillName.ToLower() + " " + targetCharacter.coloredUrlName + " in the " + chosenBodyPart.name.ToLower();

                //    if (weapon != null) {
                //        damageRange = weapon.damageRange;
                //        weaponPower = weapon.weaponPower;

                //        //reduce weapon durability by durability cost of skill
                //        weapon.AdjustDurability(-attackSkill.durabilityCost);
                //        log += " with " + (sourceCharacter.gender == GENDER.MALE ? "his" : "her") + " " + weapon.itemName + ".";

                //    } else {
                //        log += ".";
                //    }

                //    int damage = (int) (weaponPower + (attackSkill.attackType == ATTACK_TYPE.MAGIC ? sourceCharacter.intelligence : sourceCharacter.strength));
                //    int computedDamageRange = (int) ((float) damage * (damageRange / 100f));
                //    int minDamageRange = damage - computedDamageRange;
                //    int maxDamageRange = damage + computedDamageRange;
                //    damage = Utilities.rng.Next((minDamageRange < 0 ? 0 : minDamageRange), maxDamageRange + 1);

                //    if (armor != null) {
                //        if (attackSkill.attackType != ATTACK_TYPE.PIERCE) {
                //            int damageNullChance = Utilities.rng.Next(0, 100);
                //            if (damageNullChance < armor.damageNullificationChance) {
                //                log += " The attack was fully absorbed by the " + armor.itemName + ".";
                //                return;
                //            }
                //            damage -= (int) ((float) damage * (armor.baseDamageMitigation / 100f));
                //        } else {
                //            damage -= (int) ((float) damage * ((armor.baseDamageMitigation / 2f) / 100f));
                //        }
                //        if (armor.ineffectiveAttackTypes.Contains(attackSkill.attackType)) {
                //            damage -= (int) ((float) damage * 0.2f);
                //        }
                //        if (armor.effectiveAttackTypes.Contains(attackSkill.attackType)) {
                //            damage += (int) ((float) damage * 0.2f);
                //        }
                //        armor.AdjustDurability(-attackSkill.durabilityDamage);
                //    }
                //    log += "(" + damage.ToString() + ")";

                //    DealDamageToBodyPart(attackSkill, targetCharacter, sourceCharacter, chosenBodyPart, ref log);

                //    AddCombatLog(log, sourceCharacter.currentSide);

                //    targetCharacter.AdjustHP(-damage);
                //} else {
                //    string log = sourceCharacter.coloredUrlName + " used " + attackSkill.skillName.ToLower() + " on " + targetCharacter.coloredUrlName + ".";
                //    int chance = Utilities.rng.Next(0, 100);
                //    if (attackSkill.statusEffectRates != null && attackSkill.statusEffectRates.Count > 0) {
                //        for (int i = 0; i < attackSkill.statusEffectRates.Count; i++) {
                //            int value = attackSkill.statusEffectRates[i].ratePercentage;
                //            if (chance < value) {
                //                targetCharacter.AddStatusEffect(attackSkill.statusEffectRates[i].statusEffect);
                //                log += " " + StatusEffectLog(sourceCharacter, targetCharacter, attackSkill.statusEffectRates[i].statusEffect);
                //            }
                //        }
                //    }
                //    AddCombatLog(log, sourceCharacter.currentSide);
                //}
            }

		//This will select, deal damage, and apply status effect to a body part if possible 
		private void DealDamageToBodyPart(AttackSkill attackSkill, ICharacter targetCharacter, ICharacter sourceCharacter, BodyPart chosenBodyPart, ref string log){
			//int chance = Utilities.rng.Next (0, 100);

//			if(attackSkill.statusEffectRates != null && attackSkill.statusEffectRates.Count > 0){
//				for (int i = 0; i < attackSkill.statusEffectRates.Count; i++) {
//					int value = attackSkill.statusEffectRates[i].ratePercentage;
//					if(attackSkill.statusEffectRates[i].statusEffect == STATUS_EFFECT.INJURED){
//						if(attackSkill.attackType == ATTACK_TYPE.CRUSH){
//							value += 7;
//						}else{
//						}

//					}else if(attackSkill.statusEffectRates[i].statusEffect == STATUS_EFFECT.BLEEDING){
//						if(attackSkill.attackType == ATTACK_TYPE.PIERCE){
//							value += 10;
//						}else{
//						}
//					}else if(attackSkill.statusEffectRates[i].statusEffect == STATUS_EFFECT.DECAPITATED){
//						if(attackSkill.attackType == ATTACK_TYPE.SLASH){
//							value += 5;
//						}else{
//						}
//					}else if(attackSkill.statusEffectRates[i].statusEffect == STATUS_EFFECT.BURNING){
//						if(attackSkill.attackType == ATTACK_TYPE.MAGIC){
//							value += 5;
//						}
//					}

//					if(chance < value){
//						chosenBodyPart.AddStatusEffect(attackSkill.statusEffectRates[i].statusEffect);
//						chosenBodyPart.ApplyStatusEffectOnSecondaryBodyParts (attackSkill.statusEffectRates[i].statusEffect);
////						AddCombatLog (log);
//					}
//				}
//			}else{
//				if (attackSkill.attackType == ATTACK_TYPE.CRUSH){
//					if(chance < 7){
//						chosenBodyPart.AddStatusEffect(STATUS_EFFECT.INJURED);
//						chosenBodyPart.ApplyStatusEffectOnSecondaryBodyParts (STATUS_EFFECT.INJURED);

//						int logChance = Utilities.rng.Next (0, 2);
//						if(logChance == 0){
//							string[] predicate = new string[]{ "battered", "crippled", "mangled", "brokened" };
//							log += " " + targetCharacter.coloredUrlName + "'s " + chosenBodyPart.name.ToLower() + " is " + predicate[Utilities.rng.Next(0, predicate.Length)] + "!";
//						}else{
//							SecondaryBodyPart secondaryBodPart = chosenBodyPart.GetRandomSecondaryBodyPart ();
//							if(secondaryBodPart != null){
//								log += " " + targetCharacter.coloredUrlName + "'s " + secondaryBodPart.name.ToLower() + " makes a crunching noise!";
//							}
//						}

//					}
//				}else if(attackSkill.attackType == ATTACK_TYPE.PIERCE){
//					if(chance < 10){
//						chosenBodyPart.AddStatusEffect(STATUS_EFFECT.BLEEDING);
//						chosenBodyPart.ApplyStatusEffectOnSecondaryBodyParts (STATUS_EFFECT.BLEEDING);

//						int logChance = Utilities.rng.Next (0, 2);
//						if(logChance == 0){
//							SecondaryBodyPart secondaryBodPart = chosenBodyPart.GetRandomSecondaryBodyPart ();
//							if(secondaryBodPart != null){
//								string[] adjective = new string[]{ "deep", "light", "painful", "fresh", "deadly" };
//								string[] noun = new string[]{ "gash", "wound", "lesion", "tear" };

//								log += " A " + adjective[Utilities.rng.Next(0, adjective.Length)] + " " + noun[Utilities.rng.Next(0, noun.Length)] + " forms near " + targetCharacter.coloredUrlName + "'s " + secondaryBodPart.name.ToLower() + ".";
//							}
//						}else{
//							SecondaryBodyPart secondaryBodPart = chosenBodyPart.GetRandomSecondaryBodyPart ();
//							if(secondaryBodPart != null){
//								log += " Blood erupts from " + targetCharacter.coloredUrlName + "'s " + secondaryBodPart.name.ToLower() + "!";
//							}
//						}
//					}
//				}else if(attackSkill.attackType == ATTACK_TYPE.SLASH){
//					if (chosenBodyPart.HasAttribute (IBodyPart.ATTRIBUTE.NONDECAPITATABLE)){
//						return;
//					}
//					if(chance < 5){
//						chosenBodyPart.AddStatusEffect(STATUS_EFFECT.DECAPITATED);
//						chosenBodyPart.ApplyStatusEffectOnSecondaryBodyParts (STATUS_EFFECT.DECAPITATED);

//						string[] verb = new string[]{ "severed", "decapitated", "sliced off", "lopped off" };
//						log += targetCharacter.coloredUrlName + "'s " + chosenBodyPart.name.ToLower() + " has been " + verb[Utilities.rng.Next(0, verb.Length)] + " by the attack!";

//						int logChance = Utilities.rng.Next (0, 2);
//						if(logChance == 0){
//							log += " It drops to the floor lifelessly.";
//						}else{
//							log += " It flew away!";
//						}

//						string allWeaponDropped = string.Empty;
//						for (int i = 0; i < targetCharacter.equippedItems.Count; i++) {
//							Item item = targetCharacter.equippedItems [i];
//							if(item is Weapon){
//								Weapon weapon = (Weapon)item;
//								for (int j = 0; j < weapon.bodyPartsAttached.Count; j++) {
//									if(weapon.bodyPartsAttached[j].statusEffects.Contains(STATUS_EFFECT.DECAPITATED)){
//										if(allWeaponDropped != string.Empty){
//											allWeaponDropped += ", ";
//										}
//										allWeaponDropped += item.itemName;
//										targetCharacter.ThrowItem (item);
//										break;
//									}
//								}
//							}
//						}
//						if(allWeaponDropped != string.Empty){
//							log += " " + targetCharacter.coloredUrlName + " drops " + allWeaponDropped + ".";
//						}

//						//If body part is essential, instant death to the character
//						if (chosenBodyPart.importance == IBodyPart.IMPORTANCE.ESSENTIAL){
//							CheckBodyPart (chosenBodyPart.name, targetCharacter);
//						}
//					}
//				}else if(attackSkill.attackType == ATTACK_TYPE.MAGIC){
//					if(chance < 5){
//						chosenBodyPart.AddStatusEffect(STATUS_EFFECT.BURNING);
//						chosenBodyPart.ApplyStatusEffectOnSecondaryBodyParts (STATUS_EFFECT.BURNING);
//						int logChance = Utilities.rng.Next (0, 2);
//						if(logChance == 0){
//							log += " A burnt smell emanates from " + targetCharacter.coloredUrlName + "'s " + chosenBodyPart.name.ToLower() + "!";
//						}else{
//							string[] verb = new string[]{ "charred", "burning", "roasting" };
//							log += " " + targetCharacter.coloredUrlName + "'s " + chosenBodyPart.name.ToLower() + " is " + verb[Utilities.rng.Next(0, verb.Length)] + "!";
//						}
//					}
//				}
//			}
        }


		private string StatusEffectLog(ICharacter sourceCharacter, ICharacter targetCharacter, STATUS_EFFECT statusEffect){
			string log = string.Empty;
			if(statusEffect == STATUS_EFFECT.CONFUSED){
				string[] possibleLogs = new string[] {
					targetCharacter.coloredUrlName + "'s mind got corrupted!",
					targetCharacter.coloredUrlName + " started hallucinating!",
					targetCharacter.coloredUrlName + " started curling in pain and screaming 'Get out of my head!'."
				};

				log = possibleLogs[Utilities.rng.Next(0, possibleLogs.Length)];
			}
			return log;
		}
		//Returns a random body part of a character
		private BodyPart GetRandomBodyPart(ICharacter character){
			List<BodyPart> allBodyParts = character.bodyParts.Where(x => !x.statusEffects.Contains(STATUS_EFFECT.DECAPITATED)).ToList();
			if(allBodyParts.Count > 0){
				return allBodyParts [Utilities.rng.Next (0, allBodyParts.Count)];
			}else{
				return null;
			}
		}
       
		#endregion

		#region Heal Skill
		private void HealSkill(Skill skill, ICharacter sourceCharacter, ICharacter targetCharacter){
			HealSkill healSkill = (HealSkill)skill;	
			targetCharacter.AdjustHP (healSkill.healPower);
			if(sourceCharacter == targetCharacter){
				AddCombatLog(sourceCharacter.coloredUrlName + " used " + healSkill.skillName + " and healed himself/herself for " + healSkill.healPower.ToString() + ".", sourceCharacter.currentSide);
			}else if(sourceCharacter == targetCharacter){
				AddCombatLog(sourceCharacter.coloredUrlName + " used " + healSkill.skillName + " and healed " + targetCharacter.coloredUrlName + " for " + healSkill.healPower.ToString() + ".", sourceCharacter.currentSide);
			}

		}
		#endregion

		#region Flee Skill
		private void FleeSkill(ICharacter sourceCharacter, ICharacter targetCharacter){
			//TODO: ICharacter flees
			RemoveCharacter(targetCharacter);
			fledCharacters.Add (targetCharacter);
			//targetCharacter.SetIsDefeated (true);
            if(targetCharacter is Character) {
                CombatManager.Instance.CharacterContinuesAction(targetCharacter as Character);
            }
            AddCombatLog(targetCharacter.coloredUrlName + " chickened out and ran away!", targetCharacter.currentSide);
		}
		#endregion

		#region Obtain Item Skill
		private void ObtainItemSkill(ICharacter sourceCharacter, ICharacter targetCharacter){
			//TODO: ICharacter obtains an item
			AddCombatLog(targetCharacter.coloredUrlName + " obtained an item.", targetCharacter.currentSide);
		}
		#endregion

		#region Move Skill
		private void MoveSkill(Skill skill, ICharacter sourceCharacter, ICharacter targetCharacter){
			if(skill.skillName == "MoveLeft"){
				if (targetCharacter.currentRow != 1) {
					targetCharacter.SetRowNumber(targetCharacter.currentRow - 1);
				}
				AddCombatLog(targetCharacter.coloredUrlName + " moved to the left. (" + targetCharacter.currentRow + ")", targetCharacter.currentSide);
			}else if(skill.skillName == "MoveRight"){
				if (targetCharacter.currentRow != 5) {
					targetCharacter.SetRowNumber(targetCharacter.currentRow + 1);
				}
				AddCombatLog(targetCharacter.coloredUrlName + " moved to the right.(" + targetCharacter.currentRow + ")", targetCharacter.currentSide);
			}
		}
		#endregion

		//This will receive the "CharacterDeath" signal when broadcasted, this is a listener
		internal void CharacterDeath(ICharacter character){
			RemoveCharacter (character);
			//deadCharacters.Add (character);
			//character.SetIsDefeated (true);
			AddCombatLog(character.coloredUrlName + " died horribly!", character.currentSide);
		}

		internal void CharacterFainted(ICharacter character){
			RemoveCharacter (character);
			faintedCharacters.Add (character);
			//character.SetIsDefeated (true);
			AddCombatLog(character.coloredUrlName + " fainted!", character.currentSide);
		}

		//Check essential body part quantity, if all are decapitated, instant death
		private void CheckBodyPart(string bodyPart, ICharacter character){
			for (int i = 0; i < character.bodyParts.Count; i++) {
				BodyPart characterBodyPart = character.bodyParts [i];
				if(characterBodyPart.name == bodyPart && !characterBodyPart.statusEffects.Contains(STATUS_EFFECT.DECAPITATED)){
					return;
				}

				for (int j = 0; j < characterBodyPart.secondaryBodyParts.Count; j++) {
					SecondaryBodyPart secondaryBodyPart = characterBodyPart.secondaryBodyParts [j];
					if(secondaryBodyPart.name == bodyPart && !secondaryBodyPart.statusEffects.Contains(STATUS_EFFECT.DECAPITATED)){
						return;
					}
				}
			}
			InstantDeath (character);
		}

		#region Logs
		public void AddCombatLog(string combatLog, SIDES side) {
			resultsLog.Add(combatLog);
			Debug.Log (combatLog);
			if (CombatPrototypeUI.Instance != null) {
				CombatPrototypeUI.Instance.AddCombatLog(combatLog, side);
			}
		}

		public void ClearCombatLogs() {
			resultsLog.Clear();
			if (CombatPrototypeUI.Instance != null) {
				CombatPrototypeUI.Instance.ClearCombatLogs();
			}
		}
		#endregion

		public ICharacter GetAliveCharacterByID(int id, string type){
			for (int i = 0; i < this.characterSideACopy.Count; i++) {
                if(type == "character") {
                    if (!this.characterSideACopy[i].isDead && this.characterSideACopy[i].id == id && this.characterSideACopy[i] is Character) {
                        return this.characterSideACopy[i];
                    }
                } else if (type == "monster") {
                    if (!this.characterSideACopy[i].isDead && this.characterSideACopy[i].id == id && this.characterSideACopy[i] is Monster) {
                        return this.characterSideACopy[i];
                    }
                }
				
			}
			for (int i = 0; i < this.characterSideBCopy.Count; i++) {
                if (type == "character") {
                    if (!this.characterSideBCopy[i].isDead && this.characterSideBCopy[i].id == id && this.characterSideBCopy[i] is Character) {
                        return this.characterSideBCopy[i];
                    }
                } else if (type == "monster") {
                    if (!this.characterSideBCopy[i].isDead && this.characterSideBCopy[i].id == id && this.characterSideBCopy[i] is Monster) {
                        return this.characterSideBCopy[i];
                    }
                }
            }
			return null;
		}

        private int GetFinalAttack(int stat, int level, float weaponAttack) {
            return (int) (((weaponAttack + stat) * (1f + ((float)stat / 2f))) * (1f + ((float) level / 100f)));
        }

        //public Character GetOpposingCharacters(ICharacter character) {
        //    if (attacker is ICharacter) {
        //        if ((attacker as Character).id == character.id) {
        //            return defender;
        //        }
        //    } else if (attacker is Party) {
        //        if ((attacker as Party).partyMembers.Contains(character)) {
        //            return defender;
        //        }
        //    }

        //    if (defender is ICharacter) {
        //        if ((defender as Character).id == character.id) {
        //            return attacker;
        //        }
        //    } else if (defender is Party) {
        //        if ((defender as Party).partyMembers.Contains(character)) {
        //            return attacker;
        //        }
        //    }

        //    return null;
        //}
    }
}

