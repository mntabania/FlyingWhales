﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[System.Serializable]
public struct Point {
	public int X, Y;

	public Point(int x, int y) {
		this.X = x;
		this.Y = y;
	}

    public Point Sum(Point otherPoint) {
        return new Point(otherPoint.X + X, otherPoint.Y + Y);
    }

    public int Product() {
        return X * Y;
    }
    public override string ToString() {
        return $"({X}, {Y})";
    }
}

[System.Serializable]
public struct PointFloat
{
    public float X, Y;

    public PointFloat(float x, float y) {
        this.X = x;
        this.Y = y;
    }

    public PointFloat Sum(PointFloat otherPoint) {
        return new PointFloat(otherPoint.X + X, otherPoint.Y + Y);
    }

    public float Product() {
        return X * Y;
    }
    public override string ToString() {
        return $"({X}, {Y})";
    }
}

