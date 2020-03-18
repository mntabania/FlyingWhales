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

    /// <summary>
    /// Return a random number within this range.
    /// NOTE: Lower and Upper bounds are inclusive.
    /// </summary>
    /// <returns>Random Integer.</returns>
    public int Random() {
        return UnityEngine.Random.Range(lowerBound, upperBound + 1);
    }

    /// <summary>
    /// Is the given value withing this range.
    /// NOTE: Lower and Upper bounds are inclusive.
    /// </summary>
    /// <param name="value">The value to check</param>
    /// <returns>True or false.</returns>
    public bool IsInRange(int value) {
        if (value >= lowerBound && value <= upperBound) {
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Is the given value outside this ranges bounds
    /// </summary>
    /// <param name="value">The value to check</param>
    /// <returns>True or false.</returns>
    public bool IsOutsideRange(int value) {
        if (value >= upperBound || value <= lowerBound) {
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