using System.Text.Json.Serialization;

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
///     This file contains a very basic food class that extends from GameObject. It
///     provides a constructor that writes the JSON into a Food object.
/// </summary>
public class Food : GameObject {
    /// <summary>
    ///     Constructor that sets all of the properties for the Food object.
    /// </summary>
    /// <param name="X"></param>
    /// <param name="Y"></param>
    /// <param name="ARGBColor"></param>
    /// <param name="ID"></param>
    /// <param name="Mass"></param>
    [JsonConstructor]
    public Food(float X, float Y, int ARGBColor, long ID, float Mass) : base(X, Y, ARGBColor, ID, Mass) { }
}