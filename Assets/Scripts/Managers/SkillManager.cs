using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using ECS;

public class SkillManager : MonoBehaviour {

    public static SkillManager Instance = null;

    public Dictionary<string, Skill> allSkills;
	public Dictionary<string, Skill> bodyPartSkills;
    public Dictionary<string, Skill> generalSkills;

    //public Dictionary<WEAPON_TYPE, List<Skill>> weaponTypeSkills = new Dictionary<WEAPON_TYPE, List<Skill>>();

    private void Awake() {
        Instance = this;

    }
	internal void Initialize(){
		ConstructAllSkillsList();
		//ConstructWeaponTypeSkills();
	}
    private void ConstructAllSkillsList() {
        allSkills = new Dictionary<string, Skill>();
		bodyPartSkills = new Dictionary<string, Skill> ();
        generalSkills = new Dictionary<string, Skill>();
        string path = Utilities.dataPath + "Skills/";
        string[] directories = Directory.GetDirectories(path); //Get first level skill types
        for (int i = 0; i < directories.Length; i++) {
            string currDirectory = directories[i];
            string[] subDirectories = Directory.GetDirectories(currDirectory);
            for (int j = 0; j < subDirectories.Length; j++) {
                string currSubDirectory = subDirectories[j];
                string skillType = new DirectoryInfo(currSubDirectory).Name;
                string[] files = Directory.GetFiles(currSubDirectory, "*.json");
                for (int k = 0; k < files.Length; k++) {
                    string currFilePath = files[k];
                    string dataAsJson = File.ReadAllText(currFilePath);
                    SKILL_TYPE currSkillType = (SKILL_TYPE)Enum.Parse(typeof(SKILL_TYPE), skillType);
					Skill skill = null;
                    switch (currSkillType) {
						case SKILL_TYPE.ATTACK:
							AttackSkill attackSkill = JsonUtility.FromJson<AttackSkill> (dataAsJson);
							skill = attackSkill;
                            allSkills.Add(attackSkill.skillName, attackSkill);
                            break;
                        case SKILL_TYPE.HEAL:
                            HealSkill healSkill = JsonUtility.FromJson<HealSkill>(dataAsJson);
							skill = healSkill;
                            allSkills.Add(healSkill.skillName, healSkill);
                            break;
                        case SKILL_TYPE.OBTAIN_ITEM:
                            ObtainSkill obtainSkill = JsonUtility.FromJson<ObtainSkill>(dataAsJson);
							skill = obtainSkill;
                            allSkills.Add(obtainSkill.skillName, obtainSkill);
                            break;
                        case SKILL_TYPE.FLEE:
                            FleeSkill fleeSkill = JsonUtility.FromJson<FleeSkill>(dataAsJson);
							skill = fleeSkill;
                            allSkills.Add(fleeSkill.skillName, fleeSkill);
                            break;
                        case SKILL_TYPE.MOVE:
                            MoveSkill moveSkill = JsonUtility.FromJson<MoveSkill>(dataAsJson);
							skill = moveSkill;
                            allSkills.Add(moveSkill.skillName, moveSkill);
                            break;
                        default:
                            break;
                    }
					if(skill != null){
						if(skill.skillCategory == SKILL_CATEGORY.BODY_PART){
							bodyPartSkills.Add(skill.skillName, skill);
						}else if (skill.skillCategory == SKILL_CATEGORY.GENERAL) {
                            generalSkills.Add(skill.skillName, skill);
                        }
                    }
                }
            }
        }
    }

    public Skill CreateNewSkillInstance(string skillName) {
        if (allSkills.ContainsKey(skillName)) {
            return allSkills[skillName].CreateNewCopy();
        }
        throw new System.Exception("There is no skill called " + skillName);
    }
  //  private void ConstructWeaponTypeSkills() {
  //      string path = Utilities.dataPath + "WeaponTypes/";
  //      string[] weaponTypesJson = System.IO.Directory.GetFiles(path, "*.json");
		//for (int i = 0; i < weaponTypesJson.Length; i++) {
		//	string file = weaponTypesJson[i];
  //          string dataAsJson = System.IO.File.ReadAllText(file);
		//	WeaponType weapType = JsonUtility.FromJson<WeaponType>(dataAsJson);
		//	weapType.ConstructWeaponSkillsList();
		//	weaponTypeSkills.Add(weapType.weaponType, new List<Skill>(weapType.skills));
  //      }
  //  }
}
