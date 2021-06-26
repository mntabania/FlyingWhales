using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using System.Linq;
using Traits;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UtilityScripts;

public class TraitManager : BaseMonoBehaviour {
    public static TraitManager Instance;

    private Dictionary<string, Trait> _allTraits;

    //Trait Override Function Identifiers
    public const string Collision_Trait = "Collision_Trait";
    public const string Enter_Grid_Tile_Trait = "Enter_Grid_Tile_Trait";
    public const string Initiate_Map_Visual_Trait = "Initiate_Map_Visual_Trait";
    public const string Destroy_Map_Visual_Trait = "Destroy_Map_Visual_Trait";
    public const string Execute_Pre_Effect_Trait = "Execute_Pre_Effect_Trait";
    public const string Execute_Per_Tick_Effect_Trait = "Execute_Pre_Effect_Trait";
    public const string Execute_After_Effect_Trait = "Execute_Pre_Effect_Trait";
    //public const string Execute_Expected_Effect_Trait = "Execute_Expected_Effect_Trait";
    public const string Start_Perform_Trait = "Start_Perform_Trait";
    public const string Death_Trait = "Death_Trait";
    public const string Tick_Ended_Trait = "Tick_Ended_Trait";
    public const string Tick_Started_Trait = "Tick_Started_Trait";
    public const string Hour_Started_Trait = "Hour_Started_Trait";
    public const string See_Poi_Trait = "See_Poi_Trait";
    public const string See_Poi_Cannot_Witness_Trait = "See_Poi_Cannot_Witness_Trait";
    public const string Before_Start_Flee = "Before_Start_Flee";
    public const string After_Exiting_Combat = "After_Exiting_Combat";
    public const string Per_Tick_While_Stationary_Unoccupied = "Per_Tick_While_Stationary_Unoccupied";
    public const string After_Death = "After_Death";
    public const string Villager_Reaction = "Villager_Reaction";


    public static string[] instancedTraitsAndStatuses = new string[] {
        "Restrained", "Injured", "Kleptomaniac", "Lycanthrope", "Vampire",
        "Poisoned", "Resting", "Sick", "Unconscious", "Zapped", "Spooked", "Cannibal", "Lethargic",
        "Dead", "Unfaithful", "Drunk", "Burning", "Burnt", "Agoraphobic", "Music Lover", "Music Hater", 
        "Psychopath", "Plagued", "Poisonous", "Diplomatic", "Wet", "Character Trait", "Nocturnal", "Glutton", 
        "Suspicious", "Narcoleptic", "Hothead", "Inspiring", "Pyrophobic", "Angry", "Alcoholic", "Pessimist", "Lazy", 
        "Coward", "Berserked", "Catatonic", "Griefstricken", "Heartbroken", "Chaste", "Lustful", "Edible", "Paralyzed", 
        "Malnourished", "Withdrawal", "Suicidal", "Criminal", "Dazed", "Hiding", "Bored", "Overheating",
        "Freezing", "Frozen", "Ravenous", "Feeble", "Forlorn", "Accident Prone", "Disoriented", "Consumable",
        "Fire Prone", "Electric", "Venomous", "Booby Trapped", "Betrayed", "Abomination Germ", "Ensnared", "Melting",
        "Fervor", "Tended", "Tending", "Cleansing", "Dousing", "Drying", "Patrolling", "Necromancer", "Mining", 
        "Webbed", "Cultist", "Stealthy", "Invisible", "Noxious Wanderer", "DeMooder", "Defender", "Invader", "Disabler", "Infestor",
        "Abductor", "Arsonist", "Hibernating", "Baby Infestor", "Tower", "Mighty", "Stoned", "Transforming", "Subterranean", "Petrasol",
        "Snatcher", "Agitated", "Hunting", "Chained Electric", "Prisoner", "Hemophiliac", "Hemophobic", "Burning At Stake",
        "Lycanphiliac", "Lycanphobic", "Interesting", "Pest", "Night Zombie", "Finite Zombie", "Plague Reservoir", "Quarantined", 
        "Plague Caring", "Plague Cared", "Enslaved", "Travelling", "Walker Zombie", "Boomer Zombie", "Protection", "Being Drained", "Empowered",
        "Monster Slayer", "Monster Ward", "Elf Slayer", "Elf Ward", "Human Slayer", "Human Ward", "Undead Slayer", "Undead Ward",
        "Demon Slayer", "Demon Ward", "Flying", "Enhanced Power", "Corn Fed", "Potato Fed", "Pineapple Fed", "Iceberry Fed", "Stocked Up", "Dirty",
        "Uncomfortable", "Recuperating", "Fish Fed", "Animal Fed"
    };

    //public static string[] unhiddenInstancedTraits = new string[] {
    //    "Kleptomaniac", "Lycanthrope", "Vampire", "Cannibal", "Unfaithful", "Agoraphobic", "Music Lover", "Music Hater",
    //    "Psychopath", "Vigilant", "Diplomatic", "Nocturnal", "Glutton",
    //    "Suspicious", "Narcoleptic", "Hothead", "Inspiring", "Pyrophobic", "Alcoholic", "Pessimist", "Lazy",
    //    "Coward", "Chaste", "Lustful", "Edible", "Accident Prone",
    //    "Fire Prone", "Electric", "Venomous", "Necromancer", "Cultist", "Stealthy", "Noxious Wanderer", "Infestor",
    //    "Baby Infestor", "Tower", "Mighty", "Subterranean", "Petrasol",
    //};

    [FormerlySerializedAs("traitIconDictionary")] [SerializeField] private StringSpriteDictionary traitPortraitDictionary;
    [SerializeField] private StringSpriteDictionary traitIconDictionary;
    public GameObject traitIconPrefab;
    
    //Trait Processors
    public static TraitProcessor characterTraitProcessor;
    public static TraitProcessor tileObjectTraitProcessor;
    public static TraitProcessor defaultTraitProcessor;
    
    public List<string> buffTraitPool { get; private set; }
    public List<string> flawTraitPool { get; private set; }
    public List<string> neutralTraitPool { get; private set; }
    public List<string> unhiddenTraitsNotStatuses { get; private set; }

    public List<string> removeStatusTraits = new List<string> {
        "Unconscious", /*"Injured",*/ "Poisoned", "Freezing", "Frozen", "Burning", //Removed Injured since we expect that the Hospice worker will cure Injured statuses
        "Ensnared"
    };

    //This is for instanced traits that do not have unique data
    //In order to save some memory all instanced traits that do not have unique data must be stored here, they are identified by isSingleton
    //If isSingleton returns true, they are stored here, and will share only 1 instance of the trait class
    private Dictionary<string, Trait> instancedSingletonTraits;

    //private Dictionary<string, Status> instancedStackingStatuses;

    #region getters/setters
    public Dictionary<string, Trait> allTraits => _allTraits;
    #endregion

    void Awake() {
        Instance = this;
        CreateTraitProcessors();
    }

    public void Initialize() {
        instancedSingletonTraits = new Dictionary<string, Trait>();
        //instancedStackingStatuses = new Dictionary<string, Status>();
        _allTraits = new Dictionary<string, Trait>();
        unhiddenTraitsNotStatuses = new List<string>();

        string path = $"{UtilityScripts.Utilities.dataPath}Traits/";
        string[] files = Directory.GetFiles(path, "*.json");
        for (int i = 0; i < files.Length; i++) {
            Trait trait = JsonUtility.FromJson<Trait>(System.IO.File.ReadAllText(files[i]));
            if (trait.type == TRAIT_TYPE.STATUS) {
                trait = JsonUtility.FromJson<Status>(System.IO.File.ReadAllText(files[i]));
            }

            //Should not add trait if the trait is from a json file and it already exists in the dictionary
            if (!_allTraits.ContainsKey(trait.name)) {
                _allTraits.Add(trait.name, trait);
                if(trait.type != TRAIT_TYPE.STATUS && !trait.isHidden) {
                    unhiddenTraitsNotStatuses.Add(trait.name);
                }
            }
        }
        
        AddInstancedTraits(); //Traits with their own classes
        
        buffTraitPool = new List<string>();
        flawTraitPool = new List<string>();
        neutralTraitPool = new List<string>();

        string[] traitPool = GetTraitPoolForWorld();
        
        //Categorize traits from trait pool
        for (int i = 0; i < traitPool.Length; i++) {
            string currTraitName = traitPool[i];
            if (allTraits.ContainsKey(currTraitName)) {
                Trait trait = allTraits[currTraitName];
                if (trait.type == TRAIT_TYPE.BUFF) {
                    buffTraitPool.Add(currTraitName);
                } else if (trait.type == TRAIT_TYPE.FLAW) {
                    flawTraitPool.Add(currTraitName);
                } else {
                    neutralTraitPool.Add(currTraitName);
                }
            } else {
                throw new Exception($"There is no trait named: {currTraitName}");
            }
        }
    }

    #region Utilities
    private void AddInstancedTraits() {
        //TODO: REDO INSTANCED TRAITS, USE SCRIPTABLE OBJECTS for FIXED DATA
        for (int i = 0; i < instancedTraitsAndStatuses.Length; i++) {
            //Create new instances of instanced traits, but do not register them to the database.
            //This is because INSTANCED traits contained in the _allTraits list should only be used for inquiry
            //and should NEVER be used by any ITraitable
            string traitName = instancedTraitsAndStatuses[i];
            string noSpacesTraitName = UtilityScripts.Utilities.RemoveAllWhiteSpace(traitName);
            string typeName = $"Traits.{ noSpacesTraitName }, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            Type type = System.Type.GetType(typeName);
            Assert.IsNotNull(type, $"No instanced trait with type, {typeName}");
            Trait trait = System.Activator.CreateInstance(type) as Trait;
            Assert.IsNotNull(trait);

            //When the trait already exists in the dictionary, replace it with the instanced trait
            //We should always prioritize the instanced traits
            if (_allTraits.ContainsKey(traitName)) {
                _allTraits[traitName] = trait;
            } else {
                _allTraits.Add(traitName, trait);
                if (trait.type != TRAIT_TYPE.STATUS && !trait.isHidden) {
                    unhiddenTraitsNotStatuses.Add(traitName);
                }
            }
        }
    }
    public Sprite GetTraitPortrait(string traitName) {
        return traitPortraitDictionary.ContainsKey(traitName) ? traitPortraitDictionary[traitName] 
            : traitPortraitDictionary.Values.First();
    }
    public Sprite GetTraitIcon(string traitName) {
        return traitIconDictionary.ContainsKey(traitName) ? traitIconDictionary[traitName] 
            : traitIconDictionary.Values.First();
    }
    public bool HasTraitIcon(string traitName) {
        return traitPortraitDictionary.ContainsKey(traitName);
    }
    public bool IsInstancedTrait(string traitName) {
        for (int i = 0; i < instancedTraitsAndStatuses.Length; i++) {
            if (string.Equals(instancedTraitsAndStatuses[i], traitName, StringComparison.OrdinalIgnoreCase)) { //|| string.Equals(currTrait.GetType().ToString(), traitName, StringComparison.OrdinalIgnoreCase)
                return true;
            }
        }
        return false;
    }
    public bool IsTraitElemental(string traitName) {
        return traitName == "Burning" || traitName == "Freezing" || traitName == "Poisoned" || traitName == "Wet" || traitName == "Zapped" || traitName == "Overheating" || traitName == "Frozen";
    }
    public T CreateNewInstancedTraitClass<T>(string traitName) where T : Trait {
        if (instancedSingletonTraits.ContainsKey(traitName)) {
            return instancedSingletonTraits[traitName] as T;
        } else {
            string noSpacesTraitName = UtilityScripts.Utilities.RemoveAllWhiteSpace(traitName);
            string typeName = $"Traits.{ noSpacesTraitName }, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            Type type = System.Type.GetType(typeName);
            Assert.IsNotNull(type, $"No instanced trait with type, {typeName}");
            T trait = System.Activator.CreateInstance(type) as T;
            Assert.IsNotNull(trait);
            if (trait.isSingleton) {
                instancedSingletonTraits.Add(traitName, trait);
            }
            //if(trait is Status status && status.isStacking && !instancedStackingStatuses.ContainsKey(traitName)) {
            //    instancedStackingStatuses.Add(traitName, status);
            //}
            trait.InitializeInstancedTrait();
            return trait;
        }
    }
    public Status GetInstancedStackingStatus(string p_statusName) {
        //if(instancedStackingStatuses.ContainsKey(p_statusName)) {
        //    return instancedStackingStatuses[p_statusName];
        //}
        return null;
    }
    public Trait LoadTrait(SaveDataTrait saveDataTrait) {
        string noSpacesTraitName = UtilityScripts.Utilities.RemoveAllWhiteSpace(saveDataTrait.name);
        string typeName = $"Traits.{ noSpacesTraitName }, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        Type type = System.Type.GetType(typeName);
        Assert.IsNotNull(type, $"No instanced trait with type, {typeName}");
        Trait trait = System.Activator.CreateInstance(type) as Trait;
        Assert.IsNotNull(trait);
        if (trait.isSingleton) {
            if (instancedSingletonTraits.ContainsKey(saveDataTrait.name)) {
                Debug.LogError($"Singleton trait {saveDataTrait.name} was loaded more than once!");
            }
            instancedSingletonTraits.Add(saveDataTrait.name, trait);
        }
        trait.LoadFirstWaveInstancedTrait(saveDataTrait);
        return trait;
    }
    /// <summary>
    /// Utility function to determine if this character's flaws can still be activated
    /// </summary>
    /// <returns></returns>
    public bool CanStillTriggerFlaws(Character character) {
        if (character.isDead || character.faction.isPlayerFaction || UtilityScripts.GameUtilities.IsRaceBeast(character.race) || character is Summon 
            || character.hasBeenRaisedFromDead) {
            return false;
        }
        //if(doNotDisturb > 0) {
        //    return false;
        //}
        return true;
    }
    public void CopyTraitOrStatus(Trait trait, ITraitable from, ITraitable to) {
        if (from.traitContainer.HasTrait(trait.name)) {
            int numOfStacks = 1;
            if (from.traitContainer.stacks.ContainsKey(trait.name)) {
                numOfStacks = from.traitContainer.stacks[trait.name];    
            }
            //In the loop, override duration to zero so that it will not reset the trait's timer
            Trait duplicateTrait = null;
            for (int i = 0; i < numOfStacks; i++) {
                to.traitContainer.AddTrait(to, trait.name, out duplicateTrait, characterResponsible: trait.responsibleCharacter, overrideDuration: 0, bypassElementalChance: true);
                to.traitContainer.GetTraitOrStatus<Trait>(trait.name)?.SetGainedFromDoingAction(trait.gainedFromDoingType, trait.isGainedFromDoingStealth);
            }
            if (duplicateTrait != null) {
                if(duplicateTrait is Status statusCopy && trait is Status statusToCopy) {
                    statusCopy.OnCopyStatus(statusToCopy, from, to);
                }
                //Copy the trait's responsible characters and gainedFromDoing
                if(trait.responsibleCharacters != null && trait.responsibleCharacters.Count > 0) {
                    for (int i = 0; i < trait.responsibleCharacters.Count; i++) {
                        duplicateTrait.AddCharacterResponsibleForTrait(trait.responsibleCharacters[i]);
                    }
                }
                //duplicateTrait.SetGainedFromDoing(trait.gainedFromDoing);

                //Copy the trait's timer
                if (from.traitContainer.scheduleTickets.ContainsKey(trait.name)) {
                    List<TraitRemoveSchedule> traitRemoveSchedules = from.traitContainer.scheduleTickets[trait.name];
                    for (int i = 0; i < traitRemoveSchedules.Count; i++) {
                        TraitRemoveSchedule removeSchedule = traitRemoveSchedules[i];
                        string ticket = SchedulingManager.Instance.AddEntry(removeSchedule.removeDate, () => to.traitContainer.RemoveTraitOnSchedule(to, duplicateTrait), to.traitProcessor);
                        to.traitContainer.AddScheduleTicket(trait.name, ticket, removeSchedule.removeDate);
                    }
                }
            }

        } else {
            throw new Exception("Trying to copy trait " + trait.name + " of " + from.name + " to " + to.name + " but " + from.name + " does not have the trait!");
        }
    }
    public void CopyStatuses(ITraitable from, ITraitable to) {
        //List<Status> statuses = new List<Status>(from.traitContainer.statuses);
        for (int i = 0; i < from.traitContainer.statuses.Count; i++) {
            Status status = from.traitContainer.statuses[i];
            if (!to.traitContainer.HasTrait(status.name)) {
                CopyTraitOrStatus(status, from, to);
                if (status is AbominationGerm) {
                    //if copied status is abomination germ, then remove that trait from the object it came from,
                    //since it was transferred to the other object
                    if(from.traitContainer.RemoveTrait(from, status)) {
                        i--;
                    }
                }
            }
        }
    }
    public string GetNeutralizingTraitFor(TileObject tileObject) {
        if(tileObject is Tornado) {
            return "Wind Master";
        } else if (tileObject is LocustSwarm) {
            return "Beastmaster";
        } else if (tileObject is PoisonCloud) {
            return "Poison Expert";
        } else if (tileObject is BallLightning) {
            return "Thunder Master";
        } else if (tileObject is FireBall) {
            return "Fire Master";
        } else if (tileObject is Quicksand) {
            return "Earth Master";
        }
        return string.Empty;
    }
    private string[] GetTraitPoolForWorld() {
        if (WorldConfigManager.Instance.isTutorialWorld) {
            return new[] { "Inspiring", "Diplomatic", "Fast", "Persuasive", "Optimist", "Robust", "Suspicious", "Vigilant", 
                "Fire Resistant", "Music Lover", "Authoritative", "Nocturnal", "Lustful", "Chaste", "Music Hater", "Alcoholic",
                "Accident Prone", "Evil", "Treacherous", "Lazy", "Pessimist", "Unattractive", "Hothead", "Coward", "Hemophobic", "Hemophiliac",
                "Lycanphobic", "Lycanphiliac", "Ruthless"
            };
        } else {
            return new[] { "Inspiring", "Blessed", "Diplomatic", "Fast", "Persuasive", "Optimist", "Robust", "Suspicious", "Vigilant", 
                "Fire Resistant", "Music Lover", "Authoritative", "Nocturnal", "Lustful", "Chaste", "Music Hater", "Alcoholic",
                "Accident Prone", "Evil", "Treacherous", "Lazy", "Pessimist", "Unattractive", "Hothead", "Coward", "Hemophobic", "Hemophiliac",
                "Lycanphobic", "Lycanphiliac", "Ruthless"
            };;
        }
    }
    #endregion

    #region Trait Processors
    private void CreateTraitProcessors() {
        characterTraitProcessor = new CharacterTraitProcessor();
        tileObjectTraitProcessor = new TileObjectTraitProcessor();
        defaultTraitProcessor = new DefaultTraitProcessor();
    }
    public void ProcessBurningTrait(ITraitable traitable, Trait trait, ref BurningSource burningSource) {
        if (trait is Burning burning && traitable.gridTileLocation != null) {
            if (burningSource == null) {
                burningSource = new BurningSource();
            }
            burning.SetSourceOfBurning(burningSource, traitable);
        }
    }
    #endregion

    #region Clean Up
    protected override void OnDestroy() {
        characterTraitProcessor = null;
        tileObjectTraitProcessor = null;
        defaultTraitProcessor = null;
        _allTraits?.Clear();
        traitPortraitDictionary?.Clear();
        traitIconDictionary?.Clear();
        buffTraitPool?.Clear();
        buffTraitPool = null;
        flawTraitPool?.Clear();
        flawTraitPool = null;
        neutralTraitPool?.Clear();
        neutralTraitPool = null;
        removeStatusTraits?.Clear();
        removeStatusTraits = null;
        instancedSingletonTraits?.Clear();
        instancedSingletonTraits = null;
        base.OnDestroy();
        Instance = null;
    }
    #endregion
}
