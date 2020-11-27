﻿public static class SettlementSignals {
    public static string SETTLEMENT_CREATED = "OnSettlementCreated"; //Parameters (NPCSettlement newNpcSettlement)
    /// <summary>
    /// Parameters (NPCSettlement)
    /// </summary>
    public static string SETTLEMENT_CHANGE_STORAGE = "OnSettlementChangeStorage";
    /// <summary>
    /// Parameters (NPCSettlement affectedSettlement, bool siegeState)
    /// </summary>
    public static string SETTLEMENT_UNDER_SIEGE_STATE_CHANGED = "OnSettlementSiegeStateChanged";
}