﻿using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//[System.Serializable]
public class Citizen {
	public int id;
	public string name;
	public GENDER gender;
	public int age;
	public int generation;
	public int prestige;
	public City city;
	public ROLE role;
	public Role assignedRole;
	public HexTile assignedTile;
	public RACE race;
	public List<BEHAVIOR_TRAIT> behaviorTraits;
	public List<SKILL_TRAIT> skillTraits;
	public List<MISC_TRAIT> miscTraits;
	public List<Citizen> supportedCitizen;
	public Citizen supportedRebellion;
	public Citizen supportedHeir;
	public Citizen father;
	public Citizen mother;
	public Citizen spouse;
	public List<Citizen> children;
	public HexTile workLocation;
	public CitizenChances citizenChances;
	public CampaignManager campaignManager;
	public List<RelationshipKings> relationshipKings;
	public List<Citizen> successionWars;
	public List<Citizen> civilWars;
	public MONTH birthMonth;
	public int birthWeek;
	public int birthYear;
	public int prestige;
	public int previousConversionMonth;
	public int previousConversionYear;
	public bool isIndependent;
	public bool isMarried;
	public bool isDirectDescendant;
	public bool isGovernor;
	public bool isKing;
	public bool isPretender;
	public bool isHeir;
	public bool isBusy;
	public bool isDead;

	private List<Citizen> possiblePretenders = new List<Citizen>();
	private string history;

	public List<Citizen> dependentChildren{
		get{ return this.children.Where (x => x.age < 16 && !x.isMarried).ToList ();}
	}


	public Citizen(City city, int age, GENDER gender, int generation){
		this.id = Utilities.SetID (this);
		this.name = RandomNameGenerator.GenerateRandomName();
		this.age = age;
		this.gender = gender;
		this.generation = generation;
		this.prestige = 0;
		this.city = city;
		this.role = ROLE.UNTRAINED;
		this.assignedRole = null;
		this.assignedTile = null;
		this.race = city.kingdom.race;
		this.behaviorTraits = new List<BEHAVIOR_TRAIT> ();
		this.skillTraits = new List<SKILL_TRAIT> ();
		this.miscTraits = new List<MISC_TRAIT> ();
//		this.trait = GetTrait();
//		this.trait = new TRAIT[]{TRAIT.VICIOUS, TRAIT.NONE};
		this.supportedCitizen	= new List<Citizen>(); //initially to king
		this.supportedRebellion	= null; //initially to king
		this.supportedHeir = null; //initially to the first in line
		this.father = null;
		this.mother = null;
		this.spouse = null;
		this.children = new List<Citizen> ();
		this.workLocation = null;
		this.citizenChances = new CitizenChances ();
		this.campaignManager = new CampaignManager (this);
		this.relationshipKings = new List<RelationshipKings> ();
		this.successionWars = new List<Citizen> ();
		this.civilWars = new List<Citizen> ();
		this.birthMonth = (MONTH) GameManager.Instance.month;
		this.birthWeek = GameManager.Instance.week;
		this.birthYear = GameManager.Instance.year;
		this.prestige = 0;
		this.isIndependent = false;
		this.isMarried = false;
		this.isDirectDescendant = false;
		this.isGovernor = false;
		this.isKing = false;
		this.isPretender = false;
		this.isHeir = false;
		this.isBusy = false;
		this.isDead = false;
		this.history = string.Empty;
		this.city.citizens.Add (this);


		this.GenerateTraits();

		EventManager.Instance.onCitizenTurnActions.AddListener (TurnActions);
		EventManager.Instance.onMassChangeSupportedCitizen.AddListener (MassChangeSupportedCitizen);
		EventManager.Instance.onUnsupportCitizen.AddListener (UnsupportCitizen);
	}

	internal void GenerateTraits(){
		if (this.mother == null || this.father == null) {
			return;
		}

		//Generate Behaviour trait
		int firstItem = 1;
		int secondItem = 2;
		for (int j = 0; j < 4; j++) {
			BEHAVIOR_TRAIT[] behaviourPair = new BEHAVIOR_TRAIT[2]{(BEHAVIOR_TRAIT)firstItem, (BEHAVIOR_TRAIT)secondItem};
			int chanceForTrait = UnityEngine.Random.Range (0, 100);
			if (chanceForTrait <= 20) {
				//the behaviour pairs are always contradicting
				BEHAVIOR_TRAIT behaviourTrait1 = (BEHAVIOR_TRAIT)firstItem;
				BEHAVIOR_TRAIT behaviourTrait2 = (BEHAVIOR_TRAIT)secondItem;
				int chanceForTrait1 = 50;
				int chanceForTrait2 = 50;

				if (mother.behaviorTraits.Contains (behaviourTrait1)) { chanceForTrait1 += 15; chanceForTrait2 -= 15;}

				if (father.behaviorTraits.Contains (behaviourTrait1)) { chanceForTrait1 += 15; chanceForTrait2 -= 15;}

				if (mother.behaviorTraits.Contains (behaviourTrait2)) { chanceForTrait2 += 15; chanceForTrait1 -= 15;}

				if (father.behaviorTraits.Contains (behaviourTrait2)) { chanceForTrait2 += 15; chanceForTrait1 -= 15;}

				int traitChance = UnityEngine.Random.Range (0, (chanceForTrait1 + chanceForTrait2));
				if (traitChance <= chanceForTrait1) {
					this.behaviorTraits.Add (behaviourTrait1);
				} else {
					this.behaviorTraits.Add (behaviourTrait2);
				}
			}
			firstItem += 2;
			secondItem += 2;
		}

		//Generate Skill Trait
		int chanceForSkillTraitLength = UnityEngine.Random.Range (0, 100);
		int numOfSkillTraits = 0;
		if (chanceForSkillTraitLength <= 20) {
			numOfSkillTraits = 2;
		} else if (chanceForSkillTraitLength >= 21 && chanceForSkillTraitLength <= 40) {
			numOfSkillTraits = 1;
		}

		List<SKILL_TRAIT> skillTraits = new List<SKILL_TRAIT>();
		if (father.skillTraits.Count > 0 || mother.skillTraits.Count > 0) {
			int skillListChance = UnityEngine.Random.Range (0, 100);
			if (skillListChance < 50) {
				skillTraits.AddRange(father.skillTraits);
				skillTraits.AddRange(mother.skillTraits);
			} else {
				skillTraits = Utilities.GetEnumValues<SKILL_TRAIT>().ToList();
				skillTraits.Remove (SKILL_TRAIT.NONE);
			}
		} else {
			skillTraits = Utilities.GetEnumValues<SKILL_TRAIT>().ToList();
			skillTraits.Remove (SKILL_TRAIT.NONE);
		}
			
		for (int j = 0; j < numOfSkillTraits; j++) {
			SKILL_TRAIT chosenSkillTrait = skillTraits[UnityEngine.Random.Range(0, skillTraits.Count)];
			this.skillTraits.Add (chosenSkillTrait);
			if (numOfSkillTraits > 1) {
				skillTraits.Remove (chosenSkillTrait);
				if (chosenSkillTrait == SKILL_TRAIT.EFFICIENT) {
					skillTraits.Remove (SKILL_TRAIT.INEFFICIENT);
				} else if (chosenSkillTrait == SKILL_TRAIT.INEFFICIENT) {
					skillTraits.Remove (SKILL_TRAIT.EFFICIENT);
				} else if (chosenSkillTrait == SKILL_TRAIT.LAVISH) {
					skillTraits.Remove (SKILL_TRAIT.THRIFTY);
				} else if (chosenSkillTrait == SKILL_TRAIT.THRIFTY) {
					skillTraits.Remove (SKILL_TRAIT.LAVISH);
				}
			}
		}

		//misc traits
		int chanceForMiscTraitLength = UnityEngine.Random.Range (0, 100);
		int numOfMiscTraits = 0;
		if (chanceForMiscTraitLength <= 10) {
			numOfMiscTraits = 2;
		} else if (chanceForMiscTraitLength >= 11 && chanceForMiscTraitLength <= 21) {
			numOfMiscTraits = 1;
		}

		List<MISC_TRAIT> miscTraits = Utilities.GetEnumValues<MISC_TRAIT>().ToList();
		miscTraits.Remove (MISC_TRAIT.NONE);
		for (int j = 0; j < numOfMiscTraits; j++) {
			MISC_TRAIT chosenMiscTrait = miscTraits[UnityEngine.Random.Range(0, miscTraits.Count)];
			this.miscTraits.Add (chosenMiscTrait);
		}

	}

	internal int GetCampaignLimit(){
		if(this.miscTraits.Contains(MISC_TRAIT.TACTICAL)){
			return 3;
		}
		return 2;
	}
	internal void AddParents(Citizen father, Citizen mother){
		this.father = father;
		this.mother = mother;
	}

	internal void AddChild(Citizen child){
		this.children.Add (child);
	}
	internal void AssignBirthday(MONTH month, int week, int year){
		this.birthMonth = month;
		this.birthWeek = week;
		this.birthYear = year;
	}
	internal void TurnActions(){
		AttemptToAge();
		DeathReasons();

	}

	protected void AttemptToAge(){
		if((MONTH)GameManager.Instance.month == this.birthMonth && GameManager.Instance.week == this.birthWeek && GameManager.Instance.year > this.birthYear){
			this.age += 1;
			if (this.age >= 16) {
				this.citizenChances.marriageChance += 2;
				this.AttemptToMarry();
			}
		}
	}

	protected void AttemptToMarry(){
		int chanceToMarry = Random.Range (0, 100);
		if (chanceToMarry < this.citizenChances.marriageChance) {
			MarriageInvitation marriageInvitation = new MarriageInvitation (GameManager.Instance.week, GameManager.Instance.month, GameManager.Instance.year, this);
		}
	}


	internal void DeathReasons(){
		if(isDead){
			return;
		}
		float accidents = UnityEngine.Random.Range (0f, 99f);
		if(accidents <= this.citizenChances.accidentChance){
			Death ();
			string accidentCause = Utilities.accidentCauses[UnityEngine.Random.Range(0, Utilities.accidentCauses.Length)];
			if(this.gender == GENDER.FEMALE){
				StringBuilder stringBuild = new StringBuilder (accidentCause);
				stringBuild.Replace ("He", "She");
				stringBuild.Replace ("he", "she");
				stringBuild.Replace ("him", "her");
				stringBuild.Replace ("his", "her");
			}
			this.history += accidentCause;
			Debug.Log(this.name + ": " + accidentCause);
//			Debug.Log (PoliticsPrototypeManager.Instance.month + "/" + PoliticsPrototypeManager.Instance.week + "/" + PoliticsPrototypeManager.Instance.year + ": " + this.name + " DIED OF ACCIDENT!");
		}else{
			if(this.age >= 60){
				float oldAge = UnityEngine.Random.Range (0f, 99f);
				if(oldAge <= this.citizenChances.oldAgeChance){
					Death ();
					if(this.gender == GENDER.FEMALE){
						this.history += "She died of old age";
					}else{
						this.history += "He died of old age";
					}
					Debug.Log(this.name + " DIES OF OLD AGE");
//					Debug.Log (PoliticsPrototypeManager.Instance.month + "/" + PoliticsPrototypeManager.Instance.week + "/" + PoliticsPrototypeManager.Instance.year + ": " + this.name + " DIED OF OLD AGE!");
				}else{
					this.citizenChances.oldAgeChance += 0.05f;
				}
			}
		}
	}
	internal void DeathByStarvation(){
		Death ();
		if(this.gender == GENDER.FEMALE){
			this.history += "She died of starvation";
		}else{
			this.history += "He died of starvation";
		}
		Debug.Log(this.name + " DIES OF STARVATION");
	}
	internal void Death(bool isDethroned = false, Citizen newKing = null){
//		this.kingdom.royaltyList.allRoyalties.Remove (this);
		if(isDethroned){
			this.isPretender = true;
			this.city.kingdom.AddPretender (this);
		}
		if(this.isPretender){
			Citizen possiblePretender = GetPossiblePretender (this);
			if(possiblePretender != null){
				possiblePretender.isPretender = true;
				this.city.kingdom.AddPretender (possiblePretender);
			}
		}
		this.city.kingdom.successionLine.Remove (this);
		this.isDead = true;
		EventManager.Instance.onCitizenTurnActions.RemoveListener (TurnActions);
		EventManager.Instance.onMassChangeSupportedCitizen.RemoveListener (MassChangeSupportedCitizen);
		EventManager.Instance.onUnsupportCitizen.Invoke (this);
		EventManager.Instance.onUnsupportCitizen.RemoveListener (UnsupportCitizen);

		if(this.role == ROLE.GENERAL){
			if(((General)this.assignedRole).army.hp <= 0){
				EventManager.Instance.onCitizenMove.RemoveListener (((General)this.assignedRole).Move);
				EventManager.Instance.onRegisterOnCampaign.RemoveListener (((General)this.assignedRole).RegisterOnCampaign);
				EventManager.Instance.onDeathArmy.RemoveListener (((General)this.assignedRole).DeathArmy);
				this.city.citizens.Remove (this);
			}
		}
		if(this.role != ROLE.GENERAL){
			this.city.citizens.Remove (this);
		}
		EventManager.Instance.onCitizenDiedEvent.Invoke ();

		if (this.workLocation != null) {
			this.workLocation.occupant = null;
			this.workLocation.isOccupied = false;
		}

//		RoyaltyEventDelegate.onIncreaseIllnessAndAccidentChance -= IncreaseIllnessAndAccidentChance;
//		RoyaltyEventDelegate.onChangeIsDirectDescendant -= ChangeIsDirectDescendant;
//		RoyaltyEventDelegate.onMassChangeLoyalty -= MassChangeLoyalty;
//		PoliticsPrototypeManager.Instance.turnEnded -= TurnActions;

		this.isHeir = false;
		if (this.id == this.city.kingdom.king.id) {
			//ASSIGN NEW LORD, SUCCESSION
			KingdomManager.Instance.RemoveRelationshipToOtherKings (this.city.kingdom.king);
			if (isDethroned) {
				if (newKing != null) {
					this.city.kingdom.AssignNewKing (newKing);
				}
				//END SUCCESSION WAR
			} else{ 
				if (this.city.kingdom.successionLine.Count <= 0) {
					this.city.kingdom.AssignNewKing (null);
				} else {
					if(this.successionWars.Count > 0){
						if(this.successionWars.Count == 1){
							this.city.kingdom.AssignNewKing (this.successionWars [0]);
						}else{
							this.successionWars [0].isHeir = true;
							List<Citizen> claimants = new List<Citizen> ();
							if(newKing != null && newKing.id != this.successionWars[0].id){
								for(int i = 0; i < this.successionWars.Count; i++){
									if(this.successionWars[i].id != newKing.id){
										claimants.Add (this.successionWars [i]);
									}
								}
								this.city.kingdom.SuccessionWar (newKing, claimants);
							}else{
								claimants.AddRange (this.successionWars.GetRange (1, this.successionWars.Count));
								this.city.kingdom.SuccessionWar (this.successionWars [0], claimants);
							}
						}
					}else{
						List<Citizen> claimants = new List<Citizen> ();
						if(this.city.kingdom.successionLine.Count > 2){
							claimants.Clear ();
							if(this.city.kingdom.successionLine[1].prestige > this.city.kingdom.successionLine[0].prestige){
								claimants.Add (this.city.kingdom.successionLine [1]);
							}
							if(this.city.kingdom.successionLine[2].prestige > this.city.kingdom.successionLine[0].prestige){
								claimants.Add (this.city.kingdom.successionLine [2]);
							}
							claimants.AddRange (this.city.kingdom.GetPretenderClaimants (this.city.kingdom.successionLine [0]));
							claimants = claimants.Distinct ().ToList ();
							if(claimants.Count > 0){
								//START SUCCESSION WAR
								this.city.kingdom.successionLine [0].isHeir = true;
								this.city.kingdom.SuccessionWar (this.city.kingdom.successionLine [0], claimants);
							}else{
								this.city.kingdom.AssignNewKing (this.city.kingdom.successionLine [0]);
							}
						}else if(this.city.kingdom.successionLine.Count > 1){
							claimants.Clear ();
							if(this.city.kingdom.successionLine[1].prestige > this.city.kingdom.successionLine[0].prestige){
								claimants.Add (this.city.kingdom.successionLine [1]);
							}
							claimants.AddRange (this.city.kingdom.GetPretenderClaimants (this.city.kingdom.successionLine [0]));
							if(claimants.Count > 0){
								//START SUCCESSION WAR
								this.city.kingdom.successionLine [0].isHeir = true;
								this.city.kingdom.SuccessionWar (this.city.kingdom.successionLine [0], claimants);
							}else{
								this.city.kingdom.AssignNewKing (this.city.kingdom.successionLine [0]);
							}
						}else{
							this.city.kingdom.AssignNewKing (this.city.kingdom.successionLine [0]);
						}
					}

				}
				this.RemoveSuccessionAndCivilWars ();
			}
		}


	}
	internal void UnsupportCitizen(Citizen citizen){
		if(this.supportedRebellion != null){
			if(citizen.id == this.supportedRebellion.id){
				this.supportedRebellion = null;
			}
		}
		if(this.supportedHeir != null){
			if(citizen.id == this.supportedHeir.id){
				this.supportedHeir = null;
			}
		}
//		if(this.isGovernor || this.isKing){
//			if(this.id != citizen.id){
//				this.supportedCitizen.Remove (citizen);
//				if(this.isGovernor){
//					for(int i = 0; i < this.city.citizens.Count; i++){
//						if(this.city.citizens[i].assignedRole != null && this.city.citizens[i].role == ROLE.GENERAL){
//							if(((General)this.city.citizens[i].assignedRole).warLeader.id == citizen.id){
//								((General)this.city.citizens [i].assignedRole).UnregisterThisGeneral (null);
//							}
//						}
//					}
//				}
//				if(this.isKing){
//					for(int i = 0; i < this.city.kingdom.cities.Count; i++){
//						for(int j = 0; j < this.city.kingdom.cities[i].citizens.Count; j++){
//							if(this.city.kingdom.cities[i].citizens[j].assignedRole != null && this.city.kingdom.cities[i].citizens[j].role == ROLE.GENERAL){
//								if(((General)this.city.kingdom.cities[i].citizens[j].assignedRole).warLeader.id == citizen.id){
//									((General)this.city.kingdom.cities[i].citizens[j].assignedRole).UnregisterThisGeneral (null);
//								}
//							}
//						}
//					}
//				}
//			}
//		}
	}
	internal void RemoveSuccessionAndCivilWars(){
//		for(int i = 0; i < this.civilWars.Count; i++){
//			this.civilWars [i].civilWars.Remove (this);
//		}
		for(int i = 0; i < this.successionWars.Count; i++){
			this.successionWars [i].RemoveSuccessionWar(this);
		}
//		this.civilWars.Clear ();
		this.successionWars.Clear ();
	}
	internal void MassChangeSupportedCitizen(Citizen newSupported, Citizen previousSupported){
		if (this.supportedCitizen.Contains(previousSupported)) {
			this.supportedCitizen.Remove (previousSupported);
			if(newSupported != null){
				if (this.city.kingdom.id != newSupported.city.kingdom.id) {
					this.supportedCitizen.Add(this.city.kingdom.king);
				} else {
					this.supportedCitizen.Add(newSupported);
				}
			}
		}
	}
	private Citizen GetPossiblePretender(Citizen citizen){
		this.possiblePretenders.Clear ();
		ChangePossiblePretendersRecursively (citizen);
		this.possiblePretenders.RemoveAt (0);
		this.possiblePretenders.AddRange (GetSiblings (citizen));

		List<Citizen> orderedMaleRoyalties = this.possiblePretenders.Where (x => x.gender == GENDER.MALE && x.generation > citizen.generation).OrderBy(x => x.generation).ThenByDescending(x => x.age).ToList();
		if(orderedMaleRoyalties.Count > 0){
			return orderedMaleRoyalties [0];
		}else{
			List<Citizen> orderedFemaleRoyalties = this.possiblePretenders.Where (x => x.gender == GENDER.FEMALE && x.generation > citizen.generation).OrderBy(x => x.generation).ThenByDescending(x => x.age).ToList();
			if(orderedFemaleRoyalties.Count > 0){
				return orderedFemaleRoyalties [0];
			}else{
				List<Citizen> orderedBrotherRoyalties = this.possiblePretenders.Where (x => x.gender == GENDER.MALE && x.father.id == citizen.father.id && x.id != citizen.id).OrderByDescending(x => x.age).ToList();
				if(orderedBrotherRoyalties.Count > 0){
					return orderedBrotherRoyalties [0];
				}else{
					List<Citizen> orderedSisterRoyalties = this.possiblePretenders.Where (x => x.gender == GENDER.FEMALE && x.father.id == citizen.id && x.id != citizen.id).OrderByDescending(x => x.age).ToList();
					if(orderedSisterRoyalties.Count > 0){
						return orderedSisterRoyalties [0];
					}
				}
			}
		}
		return null;
	}
	internal List<Citizen> GetSiblings(Citizen citizen){
		List<Citizen> siblings = new List<Citizen> ();
		for(int i = 0; i < citizen.mother.children.Count; i++){
			if(citizen.mother.children[i].id != citizen.id){
				if(!citizen.mother.children[i].isDead){
					siblings.Add (citizen.mother.children [i]);
				}
			}
		}

		return siblings;
	}
	private void ChangePossiblePretendersRecursively(Citizen citizen){
		if(!citizen.isDead){
			this.possiblePretenders.Add (citizen);
		}

		for(int i = 0; i < citizen.children.Count; i++){
			if(citizen.children[i] != null){
				this.ChangePossiblePretendersRecursively (citizen.children [i]);
			}
		}
	}
	internal void CreateInitialRelationshipsToKings(){
		for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
			Kingdom otherKingdom = KingdomManager.Instance.allKingdoms[i];
			if (otherKingdom.id != this.city.kingdom.id) {
				this.relationshipKings.Add (new RelationshipKings (otherKingdom.king, 0));
			}
		}
	}
	internal bool CheckForSpecificWar(Citizen citizen){
		for(int i = 0; i < this.relationshipKings.Count; i++){
			if(this.relationshipKings[i].king.id == citizen.id){
				if(this.relationshipKings[i].isAtWar){
					return true;
				}
			}
		}
		return false;
	}

	internal void AssignRole(ROLE role){
		this.role = role;
		if (role == ROLE.FOODIE) {
			this.assignedRole = new Foodie (this); 
		} else if (role == ROLE.GATHERER) {
			this.assignedRole = new Gatherer (this); 
		} else if (role == ROLE.MINER) {
			this.assignedRole = new Miner (this);
		} else if (role == ROLE.GENERAL) {
			this.assignedRole = new General (this);
		} else if (role == ROLE.ENVOY) {
			this.assignedRole = new Envoy (this);
		} else if (role == ROLE.GUARDIAN) {
			this.assignedRole = new Guardian (this);
		} else if (role == ROLE.SPY) {
			this.assignedRole = new Spy (this);
		} else if (role == ROLE.TRADER) {
			this.assignedRole = new Trader (this);
		} else if (role == ROLE.GOVERNOR) {
			this.assignedRole = new Governor (this);
		} else if (role == ROLE.KING) {
			this.assignedRole = new King (this);
		}
		this.UpdatePrestige ();
	}

	internal bool IsRoyaltyCloseRelative(Citizen otherCitizen){
		if (otherCitizen.id == this.father.id || otherCitizen.id == this.mother.id) {
			//royalty is father or mother
			return true;
		}

		if (this.father.father != null && this.father.mother != null && this.mother.father != null && this.mother.mother != null) {
			if (otherCitizen.id == this.father.father.id || otherCitizen.id == this.father.mother.id ||
				otherCitizen.id == this.mother.father.id || otherCitizen.id == this.mother.mother.id) {
				//royalty is grand parent
				return true;
			}
		}

		for (int i = 0; i < this.father.children.Count; i++) {
			if(otherCitizen.id == this.father.children[i].id){
				//royalty is sibling
				return true;
			}
		}

		if (this.father.father != null) {
			for (int i = 0; i < this.father.father.children.Count; i++) {
				if (otherCitizen.id == this.father.father.children [i].id) {
					//royalty is uncle or aunt from fathers side
					return true;
				}
				for (int j = 0; j < this.father.father.children[i].children.Count; j++) {
					if (otherCitizen.id == this.father.father.children[i].children[j].id) {
						//citizen is cousin from father's side
						return true;
					}
				}
			}
		}

		if (this.mother.father != null) {
			for (int i = 0; i < this.mother.father.children.Count; i++) {
				if (otherCitizen.id == this.mother.father.children [i].id) {
					//royalty is uncle or aunt from mothers side
					return true;
				}
				for (int j = 0; j < this.mother.father.children[i].children.Count; j++) {
					if (otherCitizen.id == this.mother.father.children[i].children[j].id) {
						//citizen is cousin from mother's side
						return true;
					}
				}
			}
		}

		return false;
	}

	internal RelationshipKings GetRelationshipWithCitizen(Citizen citizen){
		for (int i = 0; i < this.relationshipKings.Count; i++) {
			if (relationshipKings [i].king.id == citizen.id) {
				return relationshipKings[i];
			}
		}
		return null;
	}

	internal void UpdatePrestige(){
		int prestige = 0;

		//compute prestige for role
		if (this.isKing) {
			prestige += 500;
			for (int i = 0; i < this.relationshipKings.Count; i++) {
				if (this.relationshipKings [i].lordRelationship == RELATIONSHIP_STATUS.FRIEND) {
					prestige += 10;
				} else if (this.relationshipKings [i].lordRelationship == RELATIONSHIP_STATUS.ALLY) {
					prestige += 30;
				} else if (this.relationshipKings [i].lordRelationship == RELATIONSHIP_STATUS.ENEMY) {
					prestige -= 10;
				} else if (this.relationshipKings [i].lordRelationship == RELATIONSHIP_STATUS.RIVAL) {
					prestige -= 30;
				}
			}

			for (int i = 0; i < this.supportedCitizen.Count; i++) {
				if (this.supportedCitizen [i].role == ROLE.GOVERNOR) {
					prestige += 20;
				}
			}

			for (int i = 0; i < ((King)this.assignedRole).ownedKingdom.cities.Count; i++) {
				prestige += 15;
			}
		}
		if (this.isGovernor) {
			prestige += 350;

			for (int i = 0; i < ((Governor)this.assignedRole).ownedCity.ownedTiles.Count; i++) {
				prestige += 5;
			}

			for (int i = 0; i < this.supportedCitizen.Count; i++) {
				if (this.supportedCitizen [i].role == ROLE.GOVERNOR) {
					prestige += 20;
				} else if (this.supportedCitizen [i].role == ROLE.KING) {
					prestige += 60;
				}
			}
		}
		if (this.isMarried && this.spouse != null) {
			if (this.spouse.isKing || this.spouse.isGovernor) {
				prestige += 150;
			}
		}
		if (this.role == ROLE.GENERAL) {
			prestige += 200;
		} else if (this.role == ROLE.SPY || this.role == ROLE.ENVOY || this.role == ROLE.GUARDIAN) {
			prestige += 150;
			for (int i = 0; i < ((Spy)this.assignedRole).successfulMissions; i++) {
				prestige += 20;
			}
			for (int i = 0; i < ((Spy)this.assignedRole).unsuccessfulMissions; i++) {
				prestige -= 5;
			}
		}  else {
			if (this.role != ROLE.UNTRAINED) {
				prestige += 100;
			} else {
				prestige += 50;
			}
		} 
		//Add prestige for successors
		this.prestige = prestige;

	}
	internal void AddSuccessionWar(Citizen enemy){
		this.successionWars.Add (enemy);
		this.campaignManager.successionWarCities.Add (new CityWar (enemy.city, false, WAR_TYPE.SUCCESSION));
	}
	internal void RemoveSuccessionWar(Citizen enemy){
		List<Campaign> campaign = this.campaignManager.activeCampaigns.FindAll (x => x.targetCity.id == enemy.city.id);
		for(int i = 0; i < campaign.Count; i++){
			for(int j = 0; j < campaign[i].registeredGenerals.Count; j++){
				((General)campaign[i].registeredGenerals [j].assignedRole).UnregisterThisGeneral (campaign[i]);
			}
			this.campaignManager.activeCampaigns.Remove (campaign[i]);
		}
		this.campaignManager.successionWarCities.RemoveAll (x => x.city.id == enemy.city.id);
		this.successionWars.Remove (enemy);
	}

}
