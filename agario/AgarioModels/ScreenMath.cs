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
///     This file contains all of the helper methods for handling the screen math.
/// </summary>
public static class ScreenMath {
    private const float ZOOM_SCALAR = 0.1f;
    private const float START_SCALE = 250;

    /// <summary>
    ///     Method that calculates the size of the Portal, based on the Player's mass.
    /// </summary>
    /// <param name="mass"> The mass of the Player object.</param>
    /// <returns> The size of the Portal. </returns>
    public static double CalculatePortalLength(float mass) {
        return Math.Min(mass, 18_000) * ZOOM_SCALAR + START_SCALE;
    }

    /// <summary>
    ///     Calculates the location of the object in the portal in percentages.
    /// </summary>
    /// <param name="objectX">The object to calculate the location for.</param>
    /// <param name="portalLeft">The left coordinate of the portal.</param>
    /// <param name="portalWidth">The width of the portal.</param>
    /// <returns> A percentage representation of the object's location.</returns>
    public static float CalculatePortalPercentage(float objectX, float portalLeft, double portalWidth) {
        objectX -= portalLeft;
        return (float)(objectX / portalWidth);
    }

    /// <summary>
    ///     Helper method that calculates an object's radius based on the portal's zoom.
    /// </summary>
    /// <param name="portalWidth">The width of the Portal.</param>
    /// <param name="screenWidth">The width of the Screen.</param>
    /// <param name="radius">The radius of the object to calculate..</param>
    /// <returns>The object's radius based on the portal's zoom. </returns>
    public static float CalculateRadius(double portalWidth, float screenWidth, float radius) {
        return (float)(radius * (screenWidth / portalWidth));
    }

    /// <summary>
    ///     Helper method that calculates an object's screen coordinates.
    /// </summary>
    /// <param name="screenWidth">The width of the screen.</param>
    /// <param name="screenHeight">The height of the screen.</param>
    /// <param name="widthPercentage">The percentage of the width of the screen that the object takes up. </param>
    /// <param name="heightPercentage">The percentage of the height of the screen that the object takes up.</param>
    /// <returns> A Vector with the location of the object in Screen Coordinates.</returns>
    public static Vector2 CalculateScreenCoordinates(float screenWidth, float screenHeight, float widthPercentage,
                                                     float heightPercentage) {
        Vector2 coordinates = new() {
            X = screenWidth * widthPercentage,
            Y = screenHeight * heightPercentage
        };

        return coordinates;
    }
}