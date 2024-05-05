# Agario
| Field     | Value                                                        |
| --------- | ------------------------------------------------------------ |
| Author    | Aidan Spendlove                                              |
| Partner   | Blake Lawlor                                                 |
| Team Name | Shut up MOM, I'm CODING                                      |
| Course    | CS 3500, University of Utah, School of Computing             |
| GitHub ID | aspendlove and BioDragon10                                   |
| Repo      | https://github.com/uofu-cs3500-spring23/assignment8agario-shut-up-mom-i-m-coding |
| Date      | 14 April 2023                                                |
| Solution  | Agario                                                       |
| Copyright | CS 3500, Aidan Spendlove, and Blake Lawlor - This work may not be copied for use in Academic Coursework. |

## Overview of Functionality

This solution contains a client that is able to connect to any agario server that implements the correct protocols. It is able to handle all of the functions needed for the game, including movement, splitting, eating food, eating other players, etc. It does this by implementing the Networking object made in assignment 7 to communicate with the server. When player and food update messages are received from the server, the messages are parsed and then deserialized into objects which follow a strict api. These objects represent Players and Food in the world, and these objects are used to update the positions of food and players in the GUI. Messages are also used to request a player split, to get the ID of the current player, and to deserialize the list of dead players and eaten food. We use MAUI's Canvas to draw players and food on the screen, and we do not render anything that it offscreen in order to increase performance. Our client runs at a stable 60fps, but is reliant on the server to get new data on each frame. The server often updates at less than 60fps, so it becomes a bottleneck, but we still update frames even if no new information has come in just for consistency. It also helps time certain actions suck as the leaderboard updating every 5 seconds. The GUI only looks at a certain portion of the entire 5000x5000 (game units) board, and percentage scaling is applied to interpolate the game units to pixels on the screen. Food and player radii to match this percentage scaling. We lock the draw canvas to an aspect ratio of 1:1 to help with this scaling. We implement a leaderboard on the GUI of the top ten players by sorting our list of players by mass and then displaying only the top ten. We send move requests to the server by getting the mouse position relative to the draw canvas, and set that as our desired location. Every frame, we lock that desired location object, and send a request to the server to move to that position. The server then handles the movement by sending us location information.

## User Interface and Game Design Decisions

In the middle of our interface is our draw canvas, which is locked to an aspect ratio of 1:1. Unless you have a square monitor or vertical monitor, there will likely be extra space on the side when maximized, and we used that space to place our game stats and leaderboard. If there is no extra space, the draw canvas is squashed to make space. As mentioned we implemented a leaderboard on the right side with a label, and it displays the top ten players and refreshes every 5 seconds. Along with being a User Interface choice, it also lends an air of competitiveness to the game. Our scaling of the player was fine tuned to give a sense of scale when you eat more food, but is capped at a certain level in order to not zoom out so far that you cannot see anything else. We include stats in the top left corner so you can keep an eye on your stats as you play.

## Partnership Info - All code was completed side by side.

## Branching - All code was pushed to the main branch

## Testing

We continually tested our client by playing the game and connecting the provided server. We also often compared it against the example client and played games with both the example client and our client in order to ensure compatibility and that it was a stable experience. We did not unit test our GUI or any associated computational classes. We do not know for certain that our code is correct, but when we ship it, we will. We will work on any bug reports we get. It's called testing in production. :smile:

# Time Expenditures

Overall we spent 20 hours on this assignment. Our estimate was 25 hours.

- April 6, we setup the basic file structure for the project - 2 hours
- April 8, we hooked up the communications class and started working on drawing objects on the screen - 4 hours
- April 11, we started implementing scaling math and players on the screen - 3 hours
- April 12, we implemented player movement - 2.5 hours
- April 13, we finalized scaling, added splitting and player death handling - 4 hours
- April 14, we finished the leaderboard and statistics, implemented GUI logging, and documented classes. 



### How are estimates going?

Our estimates were more accurate this time, as our estimate on assignment 7 was way off. We were able to work more efficiently this time and finish before our target. We worked well as a team and completed objectives quickly.

## Notes:

We hosted the example server at agario.aidanspendlove.dev:11000 and allowed other students to use it.

# Common References

- [Microsoft C# Documentation](https://learn.microsoft.com/en-us/dotnet/csharp/)

Specific resources are listed in each project level readme.