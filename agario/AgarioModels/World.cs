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
///     This file contains a representation of the game's world. It will take in messages from the server
///     and add it to their respective lists.
/// </summary>
public class World {
    public Dictionary<long, GameObject> GameObjectList { get; } = new();

    public long PlayerID { get; set; }

    public Dictionary<long, Player> SortedPlayers { get; set; } = new();

    /// <summary>
    ///     Adds a list of food to the GameObject dictionary.
    /// </summary>
    /// <param name="foods"> A list of foods to add to the dictionary.</param>
    public void AddToFood(Food[] foods) {
        lock (GameObjectList) {
            foreach (Food food in foods) GameObjectList[food.ID] = food;
        }
    }

    /// <summary>
    ///     Adds a list of players to the GameObject Dictionary and the
    ///     SortedPlayers dictionary.
    /// </summary>
    /// <param name="players"> A list of players to add.</param>
    public void AddToPlayers(Player[] players) {
        lock (GameObjectList) {
            foreach (Player player in players) {
                lock (SortedPlayers) {
                    SortedPlayers[player.ID] = player;
                }
                GameObjectList[player.ID] = player;
            }
        }
    }

    /// <summary>
    ///     Removes the specified IDs from the GameObject dictionary.
    /// </summary>
    /// <param name="gameObjectIndices">A list of IDs to remove.</param>
    public void RemoveFromGameObjects(long[] gameObjectIndices) {
        lock (GameObjectList) {
            foreach (long index in gameObjectIndices) {
                // https://stackoverflow.com/questions/3561202/check-if-instance-is-of-a-type
                if (GameObjectList.ContainsKey(index) && GameObjectList[index].GetType() == typeof(Player))
                    lock (SortedPlayers) {
                        SortedPlayers.Remove(GameObjectList[index].ID);
                    }
                GameObjectList.Remove(index);
            }
        }
    }
}