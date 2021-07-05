using UnityEngine;
using UnityEngine.UI;

public class ChanceTheWrapper : MonoBehaviour {
    [SerializeField] private GameObject goChanceItemPrefab;
    [SerializeField] private ScrollRect scrollRectChanceItems;

    public void Initialize() {
        foreach (var kvp in ChanceData.integerChances) {
            GameObject go = GameObject.Instantiate(goChanceItemPrefab, Vector3.zero, Quaternion.identity, scrollRectChanceItems.content);
            ChanceItem item = go.GetComponent<ChanceItem>();
            item.Initialize(kvp.Key);
        }
    }
}
