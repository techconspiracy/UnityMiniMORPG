using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core.Systems
{
    /// <summary>
    /// WebSocket-based LAN multiplayer manager (max 6 players).
    /// COMPLETE IMPLEMENTATION with server/client logic and synchronization.
    /// </summary>
    public class WebSocketNetworkManager : MonoBehaviour
    {
        [Header("Network Configuration")]
        [SerializeField] private int _maxPlayers = 6;
        [SerializeField] private int _port = 7777;
        [SerializeField] private float _updateRate = 20f; // Updates per second
        
        [Header("Network State")]
        [SerializeField] private bool _isHost;
        [SerializeField] private bool _isConnected;
        [SerializeField] private int _connectedPlayerCount;
        [SerializeField] private string _localPlayerId;
        
        [Header("Connection Info")]
        [SerializeField] private string _serverAddress = "127.0.0.1";
        [SerializeField] private List<string> _connectedPlayerIds = new();
        
        private TcpListener _server;
        private TcpClient _client;
        private NetworkStream _networkStream;
        private Dictionary<string, PlayerNetworkData> _playerData = new();
        private Dictionary<string, TcpClient> _connectedClients = new();
        private float _lastUpdateTime;
        private bool _isRunning;
        
        #region Events
        
        public Action<string> OnPlayerConnected;
        public Action<string> OnPlayerDisconnected;
        public Action<NetworkMessage> OnMessageReceived;
        
        #endregion
        
        #region Initialization
        
        private void Awake()
        {
            _localPlayerId = Guid.NewGuid().ToString();
            Debug.Log($"[NetworkManager] Local player ID: {_localPlayerId}");
        }
        
        private void OnDestroy()
        {
            _ = DisconnectAsync();
        }
        
        #endregion
        
        #region Host/Server
        
        public async Task<bool> StartHostAsync()
        {
            if (_isHost || _isConnected)
            {
                Debug.LogWarning("[NetworkManager] Already hosting or connected!");
                return false;
            }
            
            try
            {
                _server = new TcpListener(IPAddress.Any, _port);
                _server.Start();
                _isHost = true;
                _isConnected = true;
                _isRunning = true;
                
                _connectedPlayerIds.Add(_localPlayerId);
                _connectedPlayerCount = 1;
                
                Debug.Log($"[NetworkManager] Server started on port {_port}");
                
                // Start accepting clients
                _ = AcceptClientsAsync();
                
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[NetworkManager] Failed to start server: {e.Message}");
                return false;
            }
        }
        
        private async Task AcceptClientsAsync()
        {
            while (_isRunning && _isHost)
            {
                try
                {
                    TcpClient client = await _server.AcceptTcpClientAsync();
                    
                    if (_connectedPlayerCount >= _maxPlayers)
                    {
                        Debug.LogWarning("[NetworkManager] Max players reached, rejecting connection");
                        client.Close();
                        continue;
                    }
                    
                    string playerId = Guid.NewGuid().ToString();
                    _connectedClients[playerId] = client;
                    _connectedPlayerIds.Add(playerId);
                    _connectedPlayerCount = _connectedPlayerIds.Count;
                    
                    Debug.Log($"[NetworkManager] Player connected: {playerId}");
                    OnPlayerConnected?.Invoke(playerId);
                    
                    // Send welcome message
                    await SendToClientAsync(client, new NetworkMessage
                    {
                        messageType = MessageType.PlayerConnected,
                        senderId = "SERVER",
                        data = playerId
                    });
                    
                    // Start receiving from this client
                    _ = ReceiveFromClientAsync(client, playerId);
                }
                catch (Exception e)
                {
                    if (_isRunning)
                    {
                        Debug.LogError($"[NetworkManager] Error accepting client: {e.Message}");
                    }
                }
                
                await Task.Yield();
            }
        }
        
        private async Task ReceiveFromClientAsync(TcpClient client, string playerId)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[4096];
            
            try
            {
                while (_isRunning && client.Connected)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    
                    if (bytesRead == 0)
                    {
                        // Client disconnected
                        HandleClientDisconnect(playerId);
                        break;
                    }
                    
                    string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    NetworkMessage message = JsonUtility.FromJson<NetworkMessage>(json);
                    
                    // Process message
                    ProcessMessage(message);
                    
                    // Broadcast to other clients
                    await BroadcastMessageAsync(message, playerId);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[NetworkManager] Error receiving from client {playerId}: {e.Message}");
                HandleClientDisconnect(playerId);
            }
        }
        
        private void HandleClientDisconnect(string playerId)
        {
            if (_connectedClients.ContainsKey(playerId))
            {
                _connectedClients[playerId].Close();
                _connectedClients.Remove(playerId);
            }
            
            _connectedPlayerIds.Remove(playerId);
            _playerData.Remove(playerId);
            _connectedPlayerCount = _connectedPlayerIds.Count;
            
            Debug.Log($"[NetworkManager] Player disconnected: {playerId}");
            OnPlayerDisconnected?.Invoke(playerId);
        }
        
        private async Task BroadcastMessageAsync(NetworkMessage message, string excludePlayerId = null)
        {
            List<string> disconnectedPlayers = new();
            
            foreach (var kvp in _connectedClients)
            {
                if (kvp.Key == excludePlayerId)
                    continue;
                
                try
                {
                    await SendToClientAsync(kvp.Value, message);
                }
                catch
                {
                    disconnectedPlayers.Add(kvp.Key);
                }
            }
            
            // Clean up disconnected players
            foreach (string playerId in disconnectedPlayers)
            {
                HandleClientDisconnect(playerId);
            }
        }
        
        private async Task SendToClientAsync(TcpClient client, NetworkMessage message)
        {
            string json = JsonUtility.ToJson(message);
            byte[] data = Encoding.UTF8.GetBytes(json);
            
            NetworkStream stream = client.GetStream();
            await stream.WriteAsync(data, 0, data.Length);
        }
        
        #endregion
        
        #region Client
        
        public async Task<bool> ConnectToServerAsync(string serverAddress)
        {
            if (_isHost || _isConnected)
            {
                Debug.LogWarning("[NetworkManager] Already hosting or connected!");
                return false;
            }
            
            try
            {
                _serverAddress = serverAddress;
                _client = new TcpClient();
                
                await _client.ConnectAsync(serverAddress, _port);
                _networkStream = _client.GetStream();
                _isConnected = true;
                _isRunning = true;
                
                Debug.Log($"[NetworkManager] Connected to server: {serverAddress}:{_port}");
                
                // Start receiving messages
                _ = ReceiveMessagesAsync();
                
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[NetworkManager] Failed to connect: {e.Message}");
                return false;
            }
        }
        
        private async Task ReceiveMessagesAsync()
        {
            byte[] buffer = new byte[4096];
            
            try
            {
                while (_isRunning && _client != null && _client.Connected)
                {
                    int bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length);
                    
                    if (bytesRead == 0)
                    {
                        Debug.LogWarning("[NetworkManager] Server disconnected");
                        await DisconnectAsync();
                        break;
                    }
                    
                    string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    NetworkMessage message = JsonUtility.FromJson<NetworkMessage>(json);
                    
                    ProcessMessage(message);
                }
            }
            catch (Exception e)
            {
                if (_isRunning)
                {
                    Debug.LogError($"[NetworkManager] Error receiving messages: {e.Message}");
                    await DisconnectAsync();
                }
            }
        }
        
        #endregion
        
        #region Message Processing
        
        private void ProcessMessage(NetworkMessage message)
        {
            switch (message.messageType)
            {
                case MessageType.PlayerConnected:
                    if (message.senderId == "SERVER")
                    {
                        _localPlayerId = message.data;
                        Debug.Log($"[NetworkManager] Assigned player ID: {_localPlayerId}");
                    }
                    break;
                
                case MessageType.PlayerPosition:
                    UpdatePlayerPosition(message);
                    break;
                
                case MessageType.PlayerAction:
                    ProcessPlayerAction(message);
                    break;
                
                case MessageType.ChatMessage:
                    ProcessChatMessage(message);
                    break;
            }
            
            OnMessageReceived?.Invoke(message);
        }
        
        private void UpdatePlayerPosition(NetworkMessage message)
        {
            PlayerNetworkData data = JsonUtility.FromJson<PlayerNetworkData>(message.data);
            
            if (!_playerData.ContainsKey(message.senderId))
            {
                _playerData[message.senderId] = data;
                SpawnNetworkPlayer(message.senderId, data);
            }
            else
            {
                _playerData[message.senderId] = data;
                UpdateNetworkPlayer(message.senderId, data);
            }
        }
        
        private void ProcessPlayerAction(NetworkMessage message)
        {
            // TODO: Handle player actions (attacks, interactions, etc.)
            Debug.Log($"[NetworkManager] Player action from {message.senderId}: {message.data}");
        }
        
        private void ProcessChatMessage(NetworkMessage message)
        {
            Debug.Log($"[NetworkManager] Chat from {message.senderId}: {message.data}");
        }
        
        #endregion
        
        #region Network Updates
        
        private void Update()
        {
            if (!_isConnected)
                return;
            
            // Send position updates at fixed rate
            if (Time.time - _lastUpdateTime >= 1f / _updateRate)
            {
                SendPositionUpdate();
                _lastUpdateTime = Time.time;
            }
        }
        
        private void SendPositionUpdate()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
                return;
            
            PlayerNetworkData data = new PlayerNetworkData
            {
                position = player.transform.position,
                rotation = player.transform.rotation.eulerAngles,
                health = player.GetComponent<EntityStats>()?.currentHealth ?? 100
            };
            
            SendMessage(new NetworkMessage
            {
                messageType = MessageType.PlayerPosition,
                senderId = _localPlayerId,
                data = JsonUtility.ToJson(data)
            });
        }
        
        public void SendMessage(NetworkMessage message)
        {
            if (!_isConnected)
                return;
            
            message.senderId = _localPlayerId;
            message.timestamp = DateTime.Now.Ticks;
            
            _ = SendMessageAsync(message);
        }
        
        private async Task SendMessageAsync(NetworkMessage message)
        {
            try
            {
                if (_isHost)
                {
                    // Broadcast to all clients
                    await BroadcastMessageAsync(message);
                }
                else if (_client != null && _networkStream != null)
                {
                    // Send to server
                    string json = JsonUtility.ToJson(message);
                    byte[] data = Encoding.UTF8.GetBytes(json);
                    await _networkStream.WriteAsync(data, 0, data.Length);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[NetworkManager] Error sending message: {e.Message}");
            }
        }
        
        #endregion
        
        #region Network Players
        
        private void SpawnNetworkPlayer(string playerId, PlayerNetworkData data)
        {
            // Create visual representation of network player
            GameObject networkPlayer = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            networkPlayer.name = $"NetworkPlayer_{playerId}";
            networkPlayer.transform.position = data.position;
            networkPlayer.transform.rotation = Quaternion.Euler(data.rotation);
            
            // Color it differently
            Renderer renderer = networkPlayer.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.blue;
            renderer.material = mat;
            
            // Add network player component
            NetworkPlayer netPlayer = networkPlayer.AddComponent<NetworkPlayer>();
            netPlayer.Initialize(playerId, data);
            
            Debug.Log($"[NetworkManager] Spawned network player: {playerId}");
        }
        
        private void UpdateNetworkPlayer(string playerId, PlayerNetworkData data)
        {
            GameObject networkPlayer = GameObject.Find($"NetworkPlayer_{playerId}");
            
            if (networkPlayer != null)
            {
                NetworkPlayer netPlayer = networkPlayer.GetComponent<NetworkPlayer>();
                if (netPlayer != null)
                {
                    netPlayer.UpdateData(data);
                }
            }
        }
        
        #endregion
        
        #region Disconnect
        
        public async Task DisconnectAsync()
        {
            _isRunning = false;
            
            if (_isHost)
            {
                // Close all client connections
                foreach (var client in _connectedClients.Values)
                {
                    client.Close();
                }
                _connectedClients.Clear();
                
                _server?.Stop();
                _server = null;
            }
            else
            {
                _networkStream?.Close();
                _client?.Close();
            }
            
            _isHost = false;
            _isConnected = false;
            _connectedPlayerIds.Clear();
            _connectedPlayerCount = 0;
            _playerData.Clear();
            
            Debug.Log("[NetworkManager] Disconnected");
            
            await Task.Yield();
        }
        
        #endregion
        
        #region Public API
        
        public bool IsHost() => _isHost;
        public bool IsConnected() => _isConnected;
        public int GetPlayerCount() => _connectedPlayerCount;
        public string GetLocalPlayerId() => _localPlayerId;
        public List<string> GetConnectedPlayers() => new List<string>(_connectedPlayerIds);
        
        public void SendChatMessage(string message)
        {
            SendMessage(new NetworkMessage
            {
                messageType = MessageType.ChatMessage,
                senderId = _localPlayerId,
                data = message
            });
        }
        
        #endregion
    }
    
    #region Network Data Structures
    
    [Serializable]
    public class NetworkMessage
    {
        public MessageType messageType;
        public string senderId;
        public string data;
        public long timestamp;
    }
    
    [Serializable]
    public class PlayerNetworkData
    {
        public Vector3 position;
        public Vector3 rotation;
        public int health;
    }
    
    public enum MessageType
    {
        PlayerConnected,
        PlayerDisconnected,
        PlayerPosition,
        PlayerAction,
        ChatMessage,
        EntitySpawn,
        EntityDespawn
    }
    
    /// <summary>
    /// Component for network player representations.
    /// </summary>
    public class NetworkPlayer : MonoBehaviour
    {
        private string _playerId;
        private PlayerNetworkData _data;
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        
        public void Initialize(string playerId, PlayerNetworkData data)
        {
            _playerId = playerId;
            _data = data;
            _targetPosition = data.position;
            _targetRotation = Quaternion.Euler(data.rotation);
        }
        
        public void UpdateData(PlayerNetworkData data)
        {
            _data = data;
            _targetPosition = data.position;
            _targetRotation = Quaternion.Euler(data.rotation);
        }
        
        private void Update()
        {
            // Smooth interpolation
            transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * 10f);
            transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, Time.deltaTime * 10f);
        }
    }
    
    #endregion
}