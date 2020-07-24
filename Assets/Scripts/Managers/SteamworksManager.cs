using Steamworks;
using UnityEngine;
namespace Managers {
    public class SteamworksManager : MonoBehaviour {

        public static SteamworksManager Instance;

        [SerializeField] private bool _allowSteamworks;

#if UNITY_EDITOR
        public bool allowSteamworks => _allowSteamworks;
#else
        public bool allowSteamworks => true;
#endif
        

        #region Monobehaviours
        private void Awake() {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            } else {
                Destroy(gameObject);
            }
        }
        #endregion

        public string GetSteamName() {
            if (SteamManager.Initialized) {
                return SteamFriends.GetPersonaName();
            }
            return string.Empty;
        }

    }
}