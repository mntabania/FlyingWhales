using System;
using UnityEngine;

[Serializable]
public struct IntRange {
    public int lowerBound;
    public int upperBound;
    
    public IntRange(int low, int high) {
        lowerBound = low;
        upperBound = high;
    }

    public void SetLower(int lower) {
        lowerBound = lower;
    }
    public void SetUpper(int upper) {
        upperBound = upper;
    }

    public int Random() {
        return UnityEngine.Random.Range(lowerBound, upperBound + 1);
    }

    public bool IsInRange(int value) {
        if (value >= lowerBound && value <= upperBound) {
            return true;
        }
        return false;
    }

    public bool IsNearUpperBound(int value) {
        int lowerBoundDifference = Mathf.Abs(value - lowerBound);
        int upperBoundDifference = Mathf.Abs(value - upperBound);
        if (upperBoundDifference < lowerBoundDifference) {
            return true;
        }
        return false;
    }
}