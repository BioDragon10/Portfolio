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
///     This file is essentially the same as the Food object, but also allows for a Name to
///     be specified for the Player.
/// </summary>
public class Player : GameObject {
    /// <summary>
    ///     Constructor that creates the GameObject and sets the Name of the Player.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="X"></param>
    /// <param name="Y"></param>
    /// <param name="ARGBColor"></param>
    /// <param name="ID"></param>
    /// <param name="Mass"></param>
    [JsonConstructor]
    public Player(string name, float X, float Y, int ARGBColor, long ID, float Mass) : base(X, Y, ARGBColor, ID, Mass) {
        Name = name;
    }

    public string Name { get; set; }
}