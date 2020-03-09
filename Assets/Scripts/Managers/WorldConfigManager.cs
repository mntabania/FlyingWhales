using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldConfigManager : MonoBehaviour {

    public static WorldConfigManager Instance;

    [Header("Map Settings")]
    public int minRegionWidthCount;
    public int maxRegionWidthCount;
    public int minRegionHeightCount;
    public int maxRegionHeightCount;

    [Header("Settlements")]
    [Tooltip("Minimum number of citizens to generate on the first npcSettlement")]
    public int minCitizenCount;
    [Tooltip("Maximum number of citizens to generate on the first npcSettlement")]
    public int maxCitizenCount;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(Instance.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
    }
    public Dictionary<LANDMARK_TYPE, int> GetLandmarksForGeneration(int regionCount) {
        //The world must have the following landmarks:
        Dictionary<LANDMARK_TYPE, int> landmarks = new Dictionary<LANDMARK_TYPE, int>() {
            { LANDMARK_TYPE.FARM, 1 },  //-3 farm regions
            // { LANDMARK_TYPE.MINES, 2 }, //-2 mine regions
            // { LANDMARK_TYPE.QUARRY, 1 }, 
            // { LANDMARK_TYPE.LUMBERYARD, 1 }, 
            // { LANDMARK_TYPE.BARRACKS, 1 },//-1 barracks region
            // { LANDMARK_TYPE.WORKSHOP, 1 }, //-1 workshop region
            // { LANDMARK_TYPE.TEMPLE, 1 }, //-1 temple region
        };

        // //-4 monster lair, cave or ancient ruin region
        // LANDMARK_TYPE[] choices = new LANDMARK_TYPE[] { LANDMARK_TYPE.MONSTER_LAIR, LANDMARK_TYPE.MAGE_TOWER };
        // for (int i = 0; i < 4; i++) {
        //     LANDMARK_TYPE chosenType = choices[Random.Range(0, choices.Length)];
        //     if (!landmarks.ContainsKey(chosenType)) {
        //         landmarks.Add(chosenType, 0);
        //     }
        //     landmarks[chosenType] += 1;
        // }
        //
        // int totalLandmarks = landmarks.Values.Sum();
        // int remaining = regionCount - totalLandmarks;
        // if (remaining > 0) {
        //     //-the rest should be randomly determined: farm / mine / barracks / lair / outpost / temple / bandit camp
        //     choices = new[] { LANDMARK_TYPE.FARM, LANDMARK_TYPE.QUARRY, LANDMARK_TYPE.LUMBERYARD, LANDMARK_TYPE.BARRACKS, LANDMARK_TYPE.MONSTER_LAIR, LANDMARK_TYPE.MAGE_TOWER, LANDMARK_TYPE.TEMPLE, LANDMARK_TYPE.BANDIT_CAMP };
        //     for (int i = 0; i < remaining; i++) {
        //         LANDMARK_TYPE chosenType = choices[Random.Range(0, choices.Length)];
        //         if (!landmarks.ContainsKey(chosenType)) {
        //             landmarks.Add(chosenType, 0);
        //         }
        //         landmarks[chosenType] += 1;
        //     }
        // }

        return landmarks;
    }
}