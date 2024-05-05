using System.Numerics;

namespace AgarioModels;

/// <summary>
///     Author:    Aidan Spendlove
///     Partner:   Blake Lawlor
///     Date:      4/14/2023
///     Course:    CS 3500, University of Utah, School of Computing
///     Copyright: CS 3500 and Aidan Spendlove and Blake Lawlor- This work may not
///     be copied for use in Academic Coursework.
///     I, Aidan Spendlove and Blake Lawlor, certify that I wrote this code from scratch and
///     did not copy it in part or whole from another source.  All
///     references used in the completion of the assignments are cited
///     in my README file.
///     File Contents
///     This file contains all of the properties for GameObjects and the GameObjectComparator.
/// </summary>
public class GameObject {
    private readonly Vector2 Location;

    /// <summary>
    ///     Constructor that sets all of the basic properties of the game objects including
    ///     the location, color, ID, and Mass.
    /// </summary>
    /// <param name="X">X Location.</param>
    /// <param name="Y">Y Location.</param>
    /// <param name="ARGBColor">Color as an integer.</param>
    /// <param name="ID">The unique ID of the GameObject.</param>
    /// <param name="Mass">The Mass of the GameObject.</param>
    public GameObject(float X, float Y, int ARGBColor, long ID, float Mass) {
        Location       = new Vector2(X, Y);
        this.ARGBColor = ARGBColor;
        this.ID        = ID;
        this.Mass      = Mass;
    }

    public long ID { get; set; }

    public float X => Location.X;

    public float Y => Location.Y;

    public int ARGBColor { get; set; }

    public float Mass { get; set; }
}

/// <summary>
///     Comparator that provides a definition for how to compare GameObjects.
///     Taken from https://stackoverflow.com/questions/28067196/sortedset-custom-order-when-storing-a-class-object
/// </summary>
public class GameObjectComparator : IComparer<GameObject> {
    public int Compare(GameObject? x, GameObject? y) {
        if (x == null || y == null || x.ID == y.ID) return 0;
        if (x.Mass < y.Mass) return -1;

        return 1;
    }
}