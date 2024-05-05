using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Communications;

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
///     This class abstracts the complexities of TcpClient and provides an easy basis
///     for a chat type program. It supports incoming and outgoing connections, as well
///     as handling sending and receiving messages.
/// </summary>
public class Networking {
    public delegate void ReportConnectionEstablished(Networking channel);

    public delegate void ReportDisconnect(Networking channel);

    public delegate void ReportMessageArrived(Networking channel, string message);

    private readonly ILogger _logger;

    private readonly char _terminationChar;

    private readonly ReportConnectionEstablished onConnect;
    private readonly ReportDisconnect            onDisconnect;
    private readonly ReportMessageArrived        onMessage;

    private TcpClient _self;

    private CancellationTokenSource? _waitForCancellation;

    /// <summary>
    ///     Creates a new networking object. The ID is set to the current IP by default.
    /// </summary>
    /// <param name="logger">ILogger to log to</param>
    /// <param name="onConnect">Called after a client is connected to this object</param>
    /// <param name="onDisconnect">Called after a client disconnects from this object</param>
    /// <param name="onMessage">Called when this object receives a message while awaiting messages</param>
    /// <param name="terminationCharacter">The character that separates messages</param>
    public Networking(ILogger                     logger,
                      ReportConnectionEstablished onConnect,
                      ReportDisconnect            onDisconnect,
                      ReportMessageArrived        onMessage,
                      char                        terminationCharacter) {
        _self             = new TcpClient();
        _logger           = logger;
        ID                = "default";
        this.onConnect    = onConnect;
        this.onDisconnect = onDisconnect;
        this.onMessage    = onMessage;
        _terminationChar  = terminationCharacter;

        // taken from https://stackoverflow.com/questions/6803073/get-local-ip-address
        string hostName = Dns.GetHostName();

        IPHostEntry host = Dns.GetHostEntry(hostName);

        foreach (IPAddress ip in host.AddressList)
            if (ip.AddressFamily == AddressFamily.InterNetwork) {
                ID = ip.ToString();
                break;
            }
    }

    public string ID { get; set; }

    /// <summary>
    ///     Connects to a given host and port, and begins awaiting messages.
    ///     Calls the OnConnected callback if successful
    /// </summary>
    /// <param name="host">DNS resolvable hostname</param>
    /// <param name="port">Port</param>
    /// <exception cref="SocketException">Could not connect to remote</exception>
    public void Connect(string host, int port) {
        try {
            _self.Connect(host, port);
            if (_self.Connected) {
                lock (_logger) {
                    _logger.LogInformation($"Connected to {host}:{port}");
                }

                onConnect(this);

                AwaitMessagesAsync();
            }
            else {
                throw new SocketException();
            }
        }
        catch {
            lock (_logger) {
                _logger.LogError($"Could not connect to {host}:{port}");
            }

            throw;
        }
    }

    /// <summary>
    ///     Wait continuously or once for incoming messages, depending on the value of infinite.
    ///     Calls the OnMessage callback when a message is received.
    ///     If connection is lost, the disconnect callback is called.
    /// </summary>
    /// <param name="infinite"></param>
    /// <exception cref="SocketException">Connection closed while awaiting messages</exception>
    public async void AwaitMessagesAsync(bool infinite = true) {
        try {
            StringBuilder messageBacklog = new();
            byte[]        buffer         = new byte[4096];
            NetworkStream stream         = _self.GetStream();

            do {
                // wait for a message at least once
                while (true) {
                    // receive bytes until a full message is received
                    int total = await stream.ReadAsync(buffer);
                    if (total == 0) {
                        lock (_logger) {
                            _logger.LogError(
                                "End of Stream Reached while reading message, Connection must be closed");
                        }

                        throw new SocketException();
                    }

                    string currentData = Encoding.UTF8.GetString(buffer, 0, total);
                    messageBacklog.Append(currentData);
                    currentData = "";

                    if (!messageBacklog.ToString().EndsWith(_terminationChar)) continue; // message not complete

                    lock (_logger) {
                        _logger.LogTrace(
                            $"Received message\"{messageBacklog.ToString().Substring(0, messageBacklog.ToString().Length - 1)}\"");
                    }

                    string[] messages = messageBacklog.ToString().Split(_terminationChar);

                    foreach (string message in messages)
                        if (!string.IsNullOrEmpty(message))
                            onMessage(this, message + _terminationChar);

                    messageBacklog = new StringBuilder(); // flush the backlog

                    break;
                }
            } while (infinite);
        }
        catch (Exception e) {
            lock (_logger) {
                _logger.LogError(
                    "Connection error while waiting for messages");
            }

            Disconnect();
        }
    }

    /// <summary>
    ///     Begin accepting new connections. When a connection is established, a new networking object is created,
    ///     the new object begins awaiting messages, and then the OnConnect callback is called with that object.
    /// </summary>
    /// <param name="port"></param>
    /// <param name="infinite"></param>
    /// <exception cref="SocketException"></exception>
    public async void WaitForClients(int port, bool infinite) {
        lock (_logger) {
            _logger.LogDebug("Waiting for clients");
        }

        TcpListener? listener = new(IPAddress.Any, port);

        listener.Start();

        do {
            // wait for a client at least once
            try {
                _waitForCancellation = new CancellationTokenSource();
                TcpClient tcpClient = await listener.AcceptTcpClientAsync(_waitForCancellation.Token);
                // get a new connection to a client

                // create a networking object to represent the connection from server -> client
                Networking newNetworking = new(_logger, onConnect, onDisconnect, onMessage, _terminationChar);
                newNetworking._self = tcpClient;
                // get information on the client that has connected to us
                IPEndPoint remoteConnection =
                    (IPEndPoint)(newNetworking._self.Client.RemoteEndPoint ?? throw new SocketException());
                newNetworking.ID = remoteConnection.Address.MapToIPv4().ToString();

                lock (_logger) {
                    _logger.LogInformation($"Connection established with {remoteConnection.Address.MapToIPv4()}");
                }

                newNetworking
                    .AwaitMessagesAsync(); // immediately start listening to messages from the new client

                onConnect(newNetworking);
            }
            catch (OperationCanceledException e) {
                listener.Stop();
                break;
            }
            catch {
                lock (_logger) {
                    _logger.LogError("Connection Failed with new client");
                    // do not throw so that we can continue to accept clients
                }
            }
        } while (infinite);
    }

    /// <summary>
    ///     As Named
    /// </summary>
    /// <exception cref="ArgumentException">This object is not waiting for clients</exception>
    public void StopWaitingForClients() {
        (_waitForCancellation ?? throw new ArgumentException()).Cancel();
    }

    /// <summary>
    ///     Disconnect and call the OnDisconnect callback
    /// </summary>
    public void Disconnect() {
        if (_self.Connected) _self.Close();

        onDisconnect(this);
    }

    /// <summary>
    ///     Send a message to the connected client. If there is no termination character at the end of the message, one is
    ///     automatically added. Calls the onDisconnect callback if the remote client disconnects while sending.
    /// </summary>
    /// <param name="text">Message to send</param>
    /// <exception cref="SocketException">This object is not connected</exception>
    public async void Send(string text) {
        if (!text.EndsWith(_terminationChar))
            text += _terminationChar;
        byte[] messageBytes = Encoding.UTF8.GetBytes(text);

        // get information on the client that has connected to us
        IPEndPoint remoteConnection =
            (IPEndPoint)(_self.Client.RemoteEndPoint ?? throw new SocketException());

        lock (_logger) {
            _logger.LogTrace(
                $"Sending \"{text.Substring(0, text.Length - 1)}\" to {remoteConnection.Address.MapToIPv4()}");
        }

        try {
            await _self.GetStream().WriteAsync(messageBytes, 0, messageBytes.Length);
        }
        catch {
            lock (_logger) {
                _logger.LogError($"Send to {remoteConnection.Address.MapToIPv4()} failed");
            }

            onDisconnect(this);
        }
    }
}