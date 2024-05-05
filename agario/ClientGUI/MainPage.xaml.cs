using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using AgarioModels;
using Communications;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Windows.System;

namespace ClientGUI;

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
///     The Main Page that handles all of the methods and functionality of the GUI.
/// </summary>
public partial class MainPage {
    private const string LEADER_BOARD_DEFAULT_TEXT = "Leaderboard";

    private readonly ILogger<MainPage> _logger;

    private readonly PointObject desiredPosition = new();
    private          bool        _connected;
    private          string      _host;
    private          string      _name;
    private          int         _port = 11000;

    private bool isAlive = false;

    private static string connectionString;

    private Stopwatch _stopwatch;

    private bool allowSpaceKey;

    private Networking client;

    // keep track of how many frames have passed since the last time the leaderboard was updated
    private int shouldUpdateLeaderboard;

    public World world;

    /// <summary>
    ///     The MainPage constructor that initializes the components, and starts the timer.
    /// </summary>
    /// <param name="logger"> The logger to use.</param>
    public MainPage(ILogger<MainPage> logger) {
        CreateConnectionString();

        _logger = logger;
        InitializeComponent();
        NameEntry.Text   = "Default";
        ServerEntry.Text = "";
        if (Application.Current == null) return; // Make sure that the application has started
        // Force dark theme for consistency https://www.telerik.com/blogs/handling-light-dark-mode-dotnet-maui
        Application.Current.UserAppTheme = AppTheme.Dark;
        IDispatcherTimer timer = Application.Current.Dispatcher.CreateTimer();
        timer.Interval =  TimeSpan.FromMilliseconds(16); // ~ 60 fps
        timer.Tick     += (_, _) => FrameUpdate();
        timer.Start();
    }

    private void CreateConnectionString()
    {
        ConfigurationBuilder builder = new ConfigurationBuilder();

        builder.AddUserSecrets<MainPage>();
        IConfigurationRoot SelectedSecrets = builder.Build();


        connectionString = new SqlConnectionStringBuilder
        {
            DataSource = SelectedSecrets["ServerURL"],
            InitialCatalog = SelectedSecrets["UserName"],
            UserID = SelectedSecrets["UserName"],
            Password = SelectedSecrets["DBPassword"],
            Encrypt = false
        }.ConnectionString;
    }

    /// <summary>
    ///     Method called 60 times every second. It redraws the Playsurface, moves the player,
    ///     allows for splitting, updates the stats, and updates the leaderboard.
    /// </summary>
    private void FrameUpdate() {
        if (!_connected) return;

        PlaySurface.Invalidate();
        int desiredX;
        int desiredY;
        lock (desiredPosition) {
            desiredX = (int)desiredPosition.X;
            desiredY = (int)desiredPosition.Y;
        }


        client.Send(string.Format(Protocols.CMD_Move, desiredX, desiredY));

        if (allowSpaceKey && !SpaceButton.IsFocused) SpaceButton.Focus(); // always keep the space bar focused

        UpdateLocationLabel();
        UpdateMassLabel();

        if (++shouldUpdateLeaderboard < 300) return; // Every 5 seconds
        UpdateLeaderboard();
        UpdateTime();
        shouldUpdateLeaderboard = 0;
    }

    /// <summary>
    /// Updates the Time table every 5 seconds with the current time.
    /// </summary>
    private void UpdateTime()
    {
        try
        {
            //create instance of database connection
            using SqlConnection con = new(connectionString);

            //
            // Open the SqlConnection.
            //
            con.Open();

            //
            // This code uses an SqlCommand based on the SqlConnection.
            //


            using SqlCommand command = new("INSERT INTO Time VALUES (GETDATE()); SELECT CAST(SCOPE_IDENTITY() as int)", con);

            using SqlDataReader reader = command.ExecuteReader();

            int timeID = 0;

            while (reader.Read())
            {
                timeID = reader.GetInt32(0);
            }

            con.Close();


            if (!isAlive) return;
            //Get Player ID

            using SqlConnection con2 = new(connectionString);

            con2.Open();

            using SqlCommand playerIdCommand = new($"SELECT Player.PlayerID FROM Player WHERE Player.Name = '{_name}'", con2);

            using SqlDataReader reader2 = playerIdCommand.ExecuteReader();

            int playerID = 0;

            while (reader2.Read())
            {
                playerID = reader2.GetInt32(0);
            }

            con2.Close();

            //Shove into Player_Time_Mass

            using SqlConnection con3 = new(connectionString);

            con3.Open();

            Player player = (Player) world.GameObjectList[world.PlayerID];

            using SqlCommand playerTimeMassCommand = new($"INSERT INTO Player_Time_Mass VALUES ({timeID}, {playerID}, {player.Mass})", con3);

            playerTimeMassCommand.ExecuteNonQuery();
        }
        catch (SqlException exception)
        {
            throw;
        }
    }

    /// <summary>
    ///     Helper method that gets the number of connected players, and the 10 players with
    ///     the highest mass to put on the leaderboard.
    /// </summary>
    private void UpdateLeaderboard() {
        if (!_connected) return;
        int leaderboardCounter = 0;

        // this will be the buffer for the updated leaderboard text
        StringBuilder leaderboardText = new();

        leaderboardText.Append(LEADER_BOARD_DEFAULT_TEXT + '\n');
        leaderboardText.Append($"Players connected: {world.SortedPlayers.Count}\n");

        StringBuilder                    players = new();
        List<KeyValuePair<long, Player>> sortedPlayersList;
        lock (world.SortedPlayers) {
            sortedPlayersList = world.SortedPlayers.ToList();
        }

        GameObjectComparator gameObjectComparator = new();

        // Dictionary sort https://stackoverflow.com/a/298
        sortedPlayersList.Sort((pair1, pair2) => gameObjectComparator.Compare(pair1.Value, pair2.Value));
        // by default, the list is sorted with smallest first https://www.techiedelight.com/sort-list-in-descending-order-in-csharp/
        sortedPlayersList.Reverse();

        int topTen = 0; // only adds the first 10 players to the leaderboard buffer

        // foreach iterates over Lists in order
        foreach (KeyValuePair<long, Player> keyValuePair in sortedPlayersList) {
            if (++topTen > 10) {
                leaderboardCounter++;
                continue;
            }

            Player player = keyValuePair.Value;
            players.Append($"{++leaderboardCounter}. {player.Name} - {Math.Round(player.Mass)}\n");
        }


        leaderboardText.Append(players.ToString());
        LeaderBoardLabel.Text = leaderboardText.ToString();
    }

    /// <summary>
    ///     Sets the Player's name when enter is pressed on the NameEntry.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void NameEntryCompleted(object sender, EventArgs e) {
        NameChanged();
    }

    /// <summary>
    ///     Sets the Player's name when enter is pressed on the NameEntry.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void NameEntryUnfocused(object sender, FocusEventArgs e) {
        NameChanged();
    }

    /// <summary>
    ///     Update the player name to the current text of NameEntry
    /// </summary>
    private void NameChanged() {
        _name = NameEntry.Text;
    }

    /// <summary>
    ///     Sets the server's address when enter is pressed on the ServerEntry.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ServerEntryCompleted(object sender, EventArgs e) {
        HostChanged();
    }

    /// <summary>
    ///     Sets the server's address when ServerEntry is unfocused.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ServerEntryUnfocused(object sender, FocusEventArgs e) {
        HostChanged();
    }

    /// <summary>
    ///     Helper method that changes the server address. to ServerEntry's text.
    /// </summary>
    private void HostChanged() {
        _host = ServerEntry.Text;
    }

    /// <summary>
    ///     Method called when the Port Entry is completed. It attempts to parse the
    ///     port's text to a number.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void PortEntryCompleted(object sender, EventArgs e) {
        if (int.TryParse(PortEntry.Text, out int temp)) {
            _port = temp;
        }
        else {
            await DisplayAlert("Port not Valid", "Check that your port is correct and try again.", "OK");
            PortEntry.Text = "11000";
        }
    }

    /// <summary>
    ///     When the connection button is pressed we start the connection process to the server
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ConnectButtonPressed(object sender, EventArgs e) {
        _host                = ServerEntry.Text;
        _name                = NameEntry.Text;
        client               = new Networking(_logger, OnConnect, OnDisconnect, OnMessageReceived, '\n');
        world                = new World();
        PlaySurface.Drawable = new WorldDrawable(world);
        try {
            client.Connect(_host, _port);
        }
        catch {
            await DisplayAlert("Connection Failed",
                "The Connection Failed. Ensure that the Server Address was typed correctly and try again.", "OK");
        }
    }

    /// <summary>
    ///     When we get confirmation of connection we start the game
    /// </summary>
    /// <param name="channel"></param>
    private void OnConnect(Networking channel) {
        _connected = true;
        _logger.LogInformation("Agario connected");
        channel.Send(string.Format(Protocols.CMD_Start_Game, _name));

        isAlive = true;

        AddPlayerToDatabase();

        ConnectionView.IsVisible = false;
        GameView.IsVisible       = true;
        SpaceButton.IsVisible    = true;
        SpaceButton.Focus();
        SpaceButton.IsEnabled = true;
        LeaderBoardLabel.Text = "Loading Leaderboard"; // Leaderboard will take 5 seconds to update

        allowSpaceKey = true;

        StatsLabels.IsVisible = true;

        _stopwatch = new Stopwatch();
        _stopwatch.Start();
    }

    /// <summary>
    ///  Adds a Player to the database when they log in.
    /// </summary>
    private void AddPlayerToDatabase()
    {
        try
        {
            using SqlConnection con = new(connectionString);

            con.Open();

            using SqlCommand command = new($"SELECT Player.Name FROM Player WHERE Player.Name = '{_name}'", con);

            using SqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows) return;

            con.Close();

            using SqlConnection con2 = new(connectionString);

            con2.Open();

            using SqlCommand command2 = new($"INSERT INTO Player VALUES ('{_name}')", con2);
            command2.ExecuteNonQuery();

            //Get Player ID

            using SqlConnection con3 = new(connectionString);

            con3.Open();

            using SqlCommand playerIdCommand = new($"SELECT Player.PlayerID FROM Player WHERE Player.Name = '{_name}'", con3);

            using SqlDataReader reader2 = playerIdCommand.ExecuteReader();

            int playerID = 0;

            while (reader2.Read())
            {
                playerID = reader2.GetInt32(0);
            }

            con3.Close();

            //Add to Player_Team

            using SqlConnection con4 = new(connectionString);

            con4.Open();

            using SqlCommand command4 = new($"INSERT INTO Player_Team VALUES ({1}, {playerID})", con4);

            command4.ExecuteNonQuery();


        }
        catch (SqlException exception)
        {
            throw;
        }

    }

    /// <summary>
    ///     When we receive the disconnect from server callback we reset the game and display stats
    /// </summary>
    /// <param name="channel"></param>
    private void OnDisconnect(Networking channel) {
        _connected = false;
        _logger.LogInformation("Agario Disconnected");
        world = new World();
    }

    /// <summary>
    ///     Handle events from the server
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="message"></param>
    private void OnMessageReceived(Networking channel, string message) {
        if (message.StartsWith(Protocols.CMD_Food)) { // Update the player list from the server
            message = message[Protocols.CMD_Food.Length..];
            Food[] foods = JsonSerializer.Deserialize<Food[]>(message);
            world.AddToFood(foods);
            _logger.LogDebug("Food received from server and added to world");
        }
        else if (message.StartsWith(Protocols.CMD_Update_Players)) { // Update the player list from the server
            message = message[Protocols.CMD_Update_Players.Length..];
            Player[] players = JsonSerializer.Deserialize<Player[]>(message);
            if (players.Length > 0) {
                world.AddToPlayers(players);
                _logger.LogDebug("Players received from server and added to world. Existing Players Updated.");
            }
        }
        else if (message.StartsWith(Protocols.CMD_Dead_Players)) { // Get who has died
            message = message[Protocols.CMD_Dead_Players.Length..];
            long[] players = JsonSerializer.Deserialize<long[]>(message);
            // If you look in the obituary, and find yourself, you are probably dead
            // (unless you faked your own death and are on the run from the IRS)
            if (players.Contains(world.PlayerID)) {
                long playerID;
                float mass;
                lock (world.GameObjectList) {
                    playerID = world.PlayerID;
                    mass     = world.GameObjectList[world.PlayerID].Mass;
                }
                PlayerDied(playerID, mass);
            }


            if (players.Length > 0) {
                world.RemoveFromGameObjects(players);
                _logger.LogDebug("List of dead players received from server and removed from world.");
            }
        }
        else if (message.StartsWith(Protocols.CMD_Eaten_Food)) { // Update what food has been eaten
            message = message[Protocols.CMD_Eaten_Food.Length..];
            long[] foods = JsonSerializer.Deserialize<long[]>(message);
            world.RemoveFromGameObjects(foods);
        }
        else if (message.StartsWith(Protocols.CMD_Player_Object)) { // Get our player's ID from the server
            message = message[Protocols.CMD_Player_Object.Length..];
            long ID = long.Parse(message);
            world.PlayerID = ID;
            _logger.LogDebug("Client's specific player returned from server. Saved to world.");
        }
    }

    /// <summary>
    ///     Handle player death
    /// </summary>
    private async void PlayerDied(long playerID, float mass) {
        isAlive = false;
        _stopwatch.Stop();
        //https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.stopwatch?view=net-8.0
        // Get the elapsed time as a TimeSpan value.
        TimeSpan ts = _stopwatch.Elapsed;

        // Format and display the TimeSpan value.
        string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);

        AddToLife(playerID, elapsedTime, mass);
        allowSpaceKey = false;

        _logger.LogDebug($"Player: {playerID} died.");

        await DisplayAlert("You died.",
            $"You survived for {elapsedTime}. Your mass was {mass}.", "OK");

        world = new World();
        ResetToConnectScreen(); // Reset UI
    }

    /// <summary>
    /// Adds data to the life table on a Player's death.
    /// </summary>
    /// <param name="originalPlayerID"> The player's ID in the world.</param>
    /// <param name="elapsedTime"> The time that the player lasted. </param>
    /// <param name="mass"> The mass of the player when they died. </param>
    private void AddToLife(long originalPlayerID, string elapsedTime, float mass)
    {
        using SqlConnection con = new(connectionString); 
        con.Open();

        int intMass = (int)mass;

        Player player = (Player)world.GameObjectList[originalPlayerID];

        using SqlCommand cmd = new SqlCommand($"INSERT INTO Life Values ({intMass}, {player.ARGBColor}, '{elapsedTime}');" +
            $" SELECT CAST(SCOPE_IDENTITY() as int) ", con);

        using SqlDataReader reader = cmd.ExecuteReader();

        int lifeID = 0;

        while (reader.Read())
        {
            lifeID = reader.GetInt32(0);
        }

        con.Close();

        //Get Player ID

        using SqlConnection con2 = new(connectionString);

        con2.Open();

        using SqlCommand playerIdCommand = new($"SELECT Player.PlayerID FROM Player WHERE Player.Name = '{_name}'", con2);

        using SqlDataReader reader2 = playerIdCommand.ExecuteReader();

        int playerID = 0;

        while (reader2.Read())
        {
            playerID = reader2.GetInt32(0);
        }

        con2.Close();

        //Update Player_Life

        using SqlConnection con3 = new(connectionString);

        con3.Open();

        using SqlCommand updatePlayerLife = new SqlCommand($"INSERT INTO Player_Life VALUES ({playerID}, {lifeID})", con3);

        updatePlayerLife.ExecuteNonQuery();


    }

    /// <summary>
    ///     Flip the visibility of UI elements
    /// </summary>
    private void ResetToConnectScreen() {
        _connected               = false;
        ConnectionView.IsVisible = true;
        GameView.IsVisible       = false;
        SpaceButton.IsVisible    = false;
        SpaceButton.IsEnabled    = false;

        StatsLabels.IsVisible = false;

        allowSpaceKey = false;
    }

    /// <summary>
    ///     Handle mouse events
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args">Mouse position</param>
    /// <exception cref="InvalidOperationException">PlayerID is not a player</exception>
    private void MouseMoved(object sender, PointerEventArgs args) {
        if (!_connected) return;
        Point? mousePosition = args.GetPosition(PlaySurface);


        if (mousePosition is null) return;
        lock (world.GameObjectList) {
            if (!world.GameObjectList.ContainsKey(world.PlayerID)) return;
            // make sure that the GameObject at playerId is a player
            // https://stackoverflow.com/questions/3561202/check-if-instance-is-of-a-type
            if (!(world.GameObjectList[world.PlayerID].GetType() == typeof(Player)))
                throw new InvalidOperationException("PlayerID is not an instance of Player");
        }

        Player currentPlayer = (Player)world.GameObjectList[world.PlayerID];
        double x             = mousePosition.Value.X - PlaySurface.Width / 2;
        double y             = mousePosition.Value.Y - PlaySurface.Height / 2;

        lock (desiredPosition) {
            desiredPosition.X = x + currentPlayer.X;
            desiredPosition.Y = y + currentPlayer.Y;
        }
    }

    /// <summary>
    ///     Lock the aspect ratio to 1:1
    ///     This makes the scaling math more consistent.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void GameViewSizeChanged(object sender, EventArgs e) {
        if (GameView.Height > GameView.Width)
            PlaySurface.HeightRequest = GameView.Width;
        else
            PlaySurface.WidthRequest = GameView.Height;
    }

    /// <summary>
    ///     Handle the space button being clicked to activate this button
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SpaceButton_Clicked(object sender, EventArgs e) {
        string message = string.Format(Protocols.CMD_Split, (int)desiredPosition.X, (int)desiredPosition.Y);
        client.Send(message);
    }

    /// <summary>
    ///     Update the current player's mass
    /// </summary>
    private void UpdateMassLabel() {
        if (!_connected) return;

        lock (world.GameObjectList) {
            if (!world.GameObjectList.ContainsKey(world.PlayerID)) return;

            Player ourPlayer = (Player)world.GameObjectList[world.PlayerID];

            MassLabel.Text = $"Mass: {Math.Round(ourPlayer.Mass)}";
        }
    }

    /// <summary>
    ///     Update the current player's location
    /// </summary>
    private void UpdateLocationLabel() {
        if (!_connected) return;

        lock (world.GameObjectList) {
            if (!world.GameObjectList.ContainsKey(world.PlayerID)) return;

            Player ourPlayer = (Player)world.GameObjectList[world.PlayerID];

            LocationLabel.Text = $"Location: {Math.Round(ourPlayer.X)}, {Math.Round(ourPlayer.Y)}";
        }
    }
}