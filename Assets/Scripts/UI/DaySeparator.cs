using EZObjectPools;
using TMPro;
using UnityEngine;

public class DaySeparator : PooledObject {
    [SerializeField] private TextMeshProUGUI mainLbl;

    public void SetDay(int day) {
        mainLbl.text = $"DAY {day.ToString()}";
    }
}
