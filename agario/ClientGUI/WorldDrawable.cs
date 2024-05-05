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
///     This file contains all of the code necessary to draw the GameObjects on the screen.
/// </summary>
public class WorldDrawable : IDrawable {
    public readonly World World;

    /// <summary>
    ///     A basic constructor that will take in the world in order to have
    ///     access to it.
    /// </summary>
    /// <param name="world"> The world to take in.</param>
    public WorldDrawable(World world) {
        World = world;
    }

    /// <summary>
    ///     The draw method that draws all of the GameObjects onto the GraphicsView.
    ///     It heavily utilizes the ScreenMMath class in order to do the calculations.
    /// </summary>
    /// <param name="canvas"> The canvas to paint on.</param>
    /// <param name="dirtyRect">The size of the GraphicsView.</param>
    /// <exception cref="InvalidOperationException"></exception>
    public void Draw(ICanvas canvas, RectF dirtyRect) {
        // Draw background
        canvas.FillColor = Colors.MediumPurple;
        canvas.FillRectangle(0, 0, dirtyRect.Width, dirtyRect.Height);

        // Do not display anything if there is no active player
        if (!World.GameObjectList.ContainsKey(World.PlayerID)) return;

        // make sure that the GameObject at playerId is a player
        if (!(World.GameObjectList[World.PlayerID].GetType() == typeof(Player)))
            throw new InvalidOperationException("PlayerID is not an instance of Player");

        Player ourPlayer = (Player)World.GameObjectList[World.PlayerID];

        // aspect ratio is locked to 1:1
        double portalLength =
            ScreenMath.CalculatePortalLength(ourPlayer.Mass);

        Vector2 topLeftPortal  = new((float)(ourPlayer.X - portalLength / 2), (float)(ourPlayer.Y - portalLength / 2));
        Vector2 botRightPortal = new((float)(ourPlayer.X + portalLength / 2), (float)(ourPlayer.Y + portalLength / 2));

        foreach (GameObject gameObject in World.GameObjectList.Values) {
            // Do not draw objects that are offscreen
            if (!InBounds(gameObject.X, gameObject.Y, topLeftPortal.X, topLeftPortal.Y, botRightPortal.X,
                    botRightPortal.Y)) continue;

            canvas.FillColor = Color.FromInt(gameObject.ARGBColor);
            float radius = (float)Math.Sqrt(gameObject.Mass / Math.PI);

            // will be the same as using the width because the aspect ratio is locked
            float portalPercentageWidth =
                ScreenMath.CalculatePortalPercentage(gameObject.X, topLeftPortal.X, portalLength);
            float portalPercentageHeight =
                ScreenMath.CalculatePortalPercentage(gameObject.Y, topLeftPortal.Y, portalLength);

            Vector2 location = ScreenMath.CalculateScreenCoordinates(dirtyRect.Width, dirtyRect.Height,
                portalPercentageWidth, portalPercentageHeight);

            float calculatedRadius = ScreenMath.CalculateRadius(portalLength, dirtyRect.Width, radius);

            canvas.FillCircle(location.X, location.Y, calculatedRadius);

            if (gameObject.GetType() != typeof(Player)) continue;

            PlayerOnlyOperations((Player)gameObject, canvas, location, calculatedRadius);
        }
    }

    /// <summary>
    ///     Draws the Player's name only if the GameObject is a player.
    /// </summary>
    /// <param name="player">The Player to draw the name for.</param>
    /// <param name="canvas">The canvas to paint on.</param>
    /// <param name="location">The location of the player in world coordinates.</param>
    /// <param name="calculatedRadius">The calculated radius of the Player.</param>
    private static void PlayerOnlyOperations(Player player, ICanvas canvas, Vector2 location, float calculatedRadius) {
        canvas.DrawString(player.Name, location.X, location.Y - calculatedRadius - calculatedRadius / 10,
            HorizontalAlignment.Center);
    }

    /// <summary>
    ///     Helper method that ensures that the object being drawn is in the bounds of the portal.
    /// </summary>
    /// <param name="x">The x of the object to draw.</param>
    /// <param name="y">The y of the object to draw.</param>
    /// <param name="minX">The minimum X of the portal.</param>
    /// <param name="minY">The minimum Y of the portal.</param>
    /// <param name="maxX">The maximum X of the portal.</param>
    /// <param name="maxY">The maximum Y of the portal.</param>
    /// <returns>True iff the object is in bounds.</returns>
    private static bool InBounds(float x, float y, float minX, float minY, float maxX, float maxY) {
        return !(x > maxX ||
                 y > maxY ||
                 x < minX ||
                 y < minY);
    }
}