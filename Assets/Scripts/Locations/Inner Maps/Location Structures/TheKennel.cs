using System;
using System.Collections.Generic;
using System.Linq;
using Actionables;
using UnityEngine;
using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public class TheKennel : DemonicStructure {
        public override Vector2 selectableSize { get; }
        public override string nameplateName => $"{name} ({MaxCapacity - _remainingCapacity}/{MaxCapacity})";
        private const int BreedingDuration = GameManager.ticksPerHour;
        
        private const int MaxCapacity = 10;
        private int _remainingCapacity;
        private bool _isCurrentlyBreeding;
        private int _remainingBreedingTicks;
        private RaceClass _currentlyBreeding;
        private LocationGridTile targetTile;

        private readonly HashSet<Summon> _ownedSummons;

        private MarkerDummy _markerDummy;

        public TheKennel(Region location) : base(STRUCTURE_TYPE.THE_KENNEL, location){
            selectableSize = new Vector2(10f, 10f);
            _ownedSummons = new HashSet<Summon>();
        }

        #region Overrides
        public override void Initialize() {
            base.Initialize();
            _remainingCapacity = MaxCapacity;
            AddBreedMonsterAction();
        }
        public override void SetStructureObject(LocationStructureObject structureObj) {
            base.SetStructureObject(structureObj);
            _markerDummy = ObjectPoolManager.Instance
                .InstantiateObjectFromPool("MarkerDummy", Vector3.zero, Quaternion.identity, structureObj.objectsParent)
                .GetComponent<MarkerDummy>();
            _markerDummy.Deactivate();
        }
        protected override void DestroyStructure() {
            base.DestroyStructure();
            RemoveBreedMonsterAction();
            Messenger.RemoveListener(Signals.TICK_STARTED, PerSummonTick);
            if (_markerDummy != null) {
                ObjectPoolManager.Instance.DestroyObject(_markerDummy.gameObject);
            }
            Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        }
        #endregion
        
        #region Breed Monster
        private SUMMON_TYPE GetMonsterType(RaceClass raceClass) {
            if (raceClass.race == RACE.SKELETON) {
                return SUMMON_TYPE.Skeleton;
            } else if (raceClass.race == RACE.WOLF) {
                return SUMMON_TYPE.Wolf;
            } else if (raceClass.race == RACE.GOLEM) {
                return SUMMON_TYPE.Golem;
            } else if (raceClass.race == RACE.DEMON) {
                if (raceClass.className == Incubus.ClassName) {
                    return SUMMON_TYPE.Incubus;
                } else if (raceClass.className == Succubus.ClassName) {
                    return SUMMON_TYPE.Succubus;
                }
            } else if (raceClass.race == RACE.ELEMENTAL) {
                if (raceClass.className == FireElemental.ClassName) {
                    return SUMMON_TYPE.FireElemental;
                }
            } else if (raceClass.race == RACE.KOBOLD) {
                return SUMMON_TYPE.Kobold;
            } else if (raceClass.race == RACE.SPIDER) {
                if (raceClass.className == GiantSpider.ClassName) {
                    return SUMMON_TYPE.GiantSpider;
                }
            } 
            throw new Exception($"No summon type for monster {raceClass.ToString()}");
        }
        private int GetMonsterCapacityCost(SUMMON_TYPE summon) {
            switch (summon) {
                case SUMMON_TYPE.Skeleton:
                    return 1;
                case SUMMON_TYPE.GiantSpider:
                    return 1;
                case SUMMON_TYPE.Wolf:
                    return 2;
                case SUMMON_TYPE.FireElemental:
                    return 3;
                case SUMMON_TYPE.Golem:
                    return 3;
                case SUMMON_TYPE.Incubus:
                    return 2;
                case SUMMON_TYPE.Succubus:
                    return 2;
                case SUMMON_TYPE.Kobold:
                    return 2;
                default:
                    throw new Exception($"No capacity for monster {summon.ToString()}");
            }
        }
        private void AddBreedMonsterAction() {
            PlayerAction action = new PlayerAction(PlayerDB.Breed_Monster_Action, CanDoBreedMonster, null, OnClickBreedMonster);
            AddPlayerAction(action);
        }
        private void RemoveBreedMonsterAction() {
            RemovePlayerAction(GetPlayerAction(PlayerDB.Breed_Monster_Action));
        }
        private bool CanDoBreedMonster() {
            return _remainingCapacity > 0 && _isCurrentlyBreeding == false;
        }
        private void OnClickBreedMonster() {
            List<RaceClass> monsters = PlayerManager.Instance.player.archetype.monsters;
            UIManager.Instance.ShowClickableObjectPicker(monsters, OnChooseBreedMonster, null, CanBreedMonster, "Choose Monster to Breed." );
        }
        private void OnChooseBreedMonster(object obj) {
            RaceClass raceClass = (RaceClass)obj;
            StartBreedingMonster(raceClass);
            UIManager.Instance.HideObjectPicker();
            Messenger.Broadcast(Signals.RELOAD_PLAYER_ACTIONS, this as IPlayerActionTarget);
        }
        private bool CanBreedMonster(RaceClass raceClass) {
            return _remainingCapacity >= GetMonsterCapacityCost(GetMonsterType(raceClass));
        }
        private void StartBreedingMonster(RaceClass raceClass) {
            _isCurrentlyBreeding = true;
            _remainingBreedingTicks = 0;
            _currentlyBreeding = raceClass;

            //TODO: Make this better!
            var asset = CharacterManager.Instance.GetMarkerAsset(raceClass.race, raceClass.className == Succubus.ClassName ? GENDER.FEMALE : GENDER.MALE, raceClass.className);

            targetTile = CollectionUtilities.GetRandomElement(unoccupiedTiles);
            _markerDummy.InitialSetup(asset.animationSprites[0], targetTile);
            _markerDummy.Activate();
            Messenger.AddListener(Signals.TICK_STARTED, PerSummonTick);
        }
        private void PerSummonTick() {
            _remainingBreedingTicks++;
            _markerDummy.SetProgress((float)_remainingBreedingTicks/(float)BreedingDuration);
            if (_remainingBreedingTicks == BreedingDuration) {
                SpawnMonster(_currentlyBreeding);
                Messenger.RemoveListener(Signals.TICK_STARTED, PerSummonTick);
            }
        }
        private void SpawnMonster(RaceClass raceClass) {
            _isCurrentlyBreeding = false;
            _markerDummy.Deactivate();
            Summon summon = CharacterManager.Instance.CreateNewSummon(GetMonsterType(raceClass),
                PlayerManager.Instance.player.playerFaction, settlementLocation, location as Region);
            CharacterManager.Instance.PlaceSummon(summon, targetTile);
            summon.AddTerritory(occupiedHexTile.hexTileOwner);
            summon.combatComponent.SetCombatMode(COMBAT_MODE.Defend);
            AddOwnedSummon(summon);
            PlayerManager.Instance.player.AddSummon(summon);
            Messenger.Broadcast(Signals.RELOAD_PLAYER_ACTIONS, this as IPlayerActionTarget);
        }
        private void AddOwnedSummon(Summon summon) {
            if (_ownedSummons.Contains(summon) == false) {
                _ownedSummons.Add(summon);
                DecreaseCapacityBasedOn(summon.summonType);
                if (_ownedSummons.Count == 1) {
                    Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
                }
            }
        }
        private void RemoveOwnedSummon(Summon summon) {
            if (_ownedSummons.Remove(summon)) {
                IncreaseCapacityBasedOn(summon.summonType);
                if (_ownedSummons.Count == 0) {
                    Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
                }
            }
        }
        private void DecreaseCapacityBasedOn(SUMMON_TYPE summonType) {
            _remainingCapacity -= GetMonsterCapacityCost(summonType);
            _remainingCapacity = Mathf.Max(_remainingCapacity, 0);
        }
        private void IncreaseCapacityBasedOn(SUMMON_TYPE summonType) {
            _remainingCapacity += GetMonsterCapacityCost(summonType);
            _remainingCapacity = Mathf.Min(_remainingCapacity, MaxCapacity);
        }
        private void OnCharacterDied(Character character) {
            if (character is Summon summon && _ownedSummons.Contains(character)) {
                RemoveOwnedSummon(summon);
                PlayerManager.Instance.player.RemoveSummon(summon);
            }
        }
        #endregion
    }
}