using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using MultiCraft.Scripts.Engine.Core.Player;
using MultiCraft.Scripts.Engine.Network.Worlds;
using MultiCraft.Scripts.Engine.UI;
using UnityWebSocket;

namespace MultiCraft.Scripts.Engine.Network
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance { get; private set; }

        [Header("Debug Settings")] public bool enableLogging = true; // Флаг для включения/отключения логирования

        public GameObject playerPrefab;
        public GameObject otherPlayerPrefab;
        private Dictionary<string, GameObject> _otherPlayers = new Dictionary<string, GameObject>();

        public string serverAddress = "wss://ms-mult.onrender.com";

        private WebSocket _webSocket;

        private string _playerName;
        private string _playerPassword;

        private GameObject _player;
        private Vector3 _playerPosition;
        private Coroutine moveCoroutine;

        private int _chunksToLoad;

        private void Start()
        {
            Instance = this;

            _playerName = Guid.NewGuid().ToString();
            _playerPassword = Guid.NewGuid().ToString();
            
            string jsonMessage = JsonSerializer.Serialize(new { type = "connect", login = _playerName, password = _playerPassword });
            Debug.Log(jsonMessage);
            Debug.Log(Encoding.UTF8.GetBytes(jsonMessage).ToString());

            _webSocket = new WebSocket(serverAddress);

            _webSocket.OnOpen += OnOpen;
            _webSocket.OnClose += OnClose;
            _webSocket.OnMessage += OnMessage;
            _webSocket.OnError += OnError;
            /*
            _webSocket.OnOpen += () =>
            {
                LogDebug("[Client] WebSocket connection opened.");
                SendMessageToServer(new { type = "connect", login = _playerName, password = _playerPassword });
            };

            _webSocket.OnMessage += (messageBytes) =>
            {
                string message = Encoding.UTF8.GetString(messageBytes);
                //LogDebug($"[Client] Received message: {message}");
                HandleServerMessage(message);
            };

            _webSocket.OnClose += (e) => { LogDebug($"[Client] Connection closed. Reason: {e}"); };

            _webSocket.OnError += (e) => { LogError($"[Client] Error: {e}"); };
*/
            _webSocket.ConnectAsync();
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            LogError($"[Client] Error: {e}");
        }

        private void OnMessage(object sender, MessageEventArgs e)
        {
            string message = Encoding.UTF8.GetString(e.RawData);
            //LogDebug($"[Client] Received message: {message}");
            HandleServerMessage(message);
        }

        private void OnClose(object sender, CloseEventArgs e)
        {
            LogDebug($"[Client] Connection closed. Reason: {e}");
        }

        private void OnOpen(object sender, OpenEventArgs e)
        {
            LogDebug("[Client] WebSocket connection opened.");
            SendMessageToServer(new { type = "connect", login = _playerName, password = _playerPassword });
        }

        private void HandleServerMessage(string data)
        {
            try
            {
                var message = JsonDocument.Parse(data);
                var type = message.RootElement.GetProperty("type").GetString();

                if (type != "player_moved")
                    LogDebug($"[Client] Handling message of type: {type}\nmessage:{message.RootElement.ToString()}");


                switch (type)
                {
                    case "connected":
                        LogDebug($"[Client] Player connected: {_playerName}");
                        OnConnected(message.RootElement);
                        SendMessageToServer(new { type = "get_players" });
                        break;

                    case "player_connected":
                        LogDebug("[Client] A new player connected.");
                        OnPlayerConnected(message.RootElement);
                        break;

                    case "player_moved":
                        //LogDebug("[Client] Player moved.");
                        OnPlayerMoved(message.RootElement);
                        break;

                    case "player_disconnected":
                        LogDebug("[Client] A player disconnected.");
                        OnPlayerDisconnected(message.RootElement);
                        break;

                    case "players_list":
                        LogDebug("[Client] Received players list.");
                        OnPlayersListReceived(message.RootElement);
                        break;

                    case "chunk_data":
                        LogDebug("[Client] Received chunk data.");
                        HandleChunkData(message.RootElement);
                        break;

                    case "player_update":
                        LogDebug("[Client] Player update received.");
                        HandlePlayerUpdate(message.RootElement);
                        break;

                    case "block_update":
                        LogDebug("[Client] Block update received.");
                        HandleBlockUpdate(message.RootElement);
                        break;

                    case "event":
                        LogDebug("[Client] Event received.");
                        HandleEvent(message.RootElement);
                        break;

                    default:
                        LogWarning($"[Client] Unknown message type: {type}");
                        break;
                }
            }
            catch (Exception ex)
            {
                LogError($"[Client] Exception while handling server message: {ex}");
            }
        }

        private void OnConnected(JsonElement data)
        {
            Vector3 position = JsonToVector3Safe(data, "position");
            LogDebug($"[Client] Spawned player at position {position}");
            _playerPosition = position;
            RequestChunksInView(position);
        }

        private void OnPlayerConnected(JsonElement data)
        {
            string playerId = data.GetProperty("player_id").GetString();
            Vector3 position = JsonToVector3(data.GetProperty("position"));

            if (!_otherPlayers.ContainsKey(playerId) && playerId != _playerName)
            {
                LogDebug($"[Client] Player {playerId} connected at position {position}");
                GameObject playerObject = Instantiate(otherPlayerPrefab, position, Quaternion.identity);
                _otherPlayers[playerId] = playerObject;
            }
        }

        private void OnPlayerMoved(JsonElement data)
        {
            string playerId = data.GetProperty("player_id").GetString();
            Vector3 position = JsonToVector3(data.GetProperty("position"));

            if (_otherPlayers.TryGetValue(playerId, out var player))
            {
                //LogDebug($"[Client] Player {playerId} moved to position {position}");
                player.transform.position = position;
            }
        }

        private void OnPlayerDisconnected(JsonElement data)
        {
            string playerId = data.GetProperty("player_id").GetString();

            if (_otherPlayers.ContainsKey(playerId))
            {
                LogDebug($"[Client] Player {playerId} disconnected.");
                Destroy(_otherPlayers[playerId]);
                _otherPlayers.Remove(playerId);
            }
        }

        private void OnPlayersListReceived(JsonElement data)
        {
            var players = data.GetProperty("players");

            foreach (JsonElement player in players.EnumerateArray())
            {
                string playerId = player.GetProperty("player_id").GetString();
                Vector3 position = JsonToVector3(player.GetProperty("position"));

                if (!_otherPlayers.ContainsKey(playerId) && playerId != _playerName)
                {
                    LogDebug($"[Client] Player {playerId} is at position {position}");
                    GameObject playerObject = Instantiate(otherPlayerPrefab, position, Quaternion.identity);
                    _otherPlayers[playerId] = playerObject;
                }
            }
        }

        private void HandleChunkData(JsonElement data)
        {
            Vector3Int chunkCoord = JsonToVector3Int(data.GetProperty("position"));

            LogDebug($"[Client] Chunk data received for coordinates {chunkCoord}");

            const int chunkWidth = 16;
            const int chunkHeight = 256;
            const int chunkDepth = 16;

            JsonElement blocksJson = data.GetProperty("blocks");
            int[] flatBlocks = new int[chunkWidth * chunkHeight * chunkDepth];
            int[,,] blocks = new int[chunkWidth, chunkHeight, chunkDepth];

            int index = 0;
            foreach (JsonElement blockValue in blocksJson.EnumerateArray())
            {
                flatBlocks[index] = blockValue.GetInt32();
                index++;
            }

            for (int x = 0; x < chunkWidth; x++)
            {
                for (int y = 0; y < chunkHeight; y++)
                {
                    for (int z = 0; z < chunkDepth; z++)
                    {
                        int flatIndex = x + z * chunkWidth + y * chunkWidth * chunkDepth;
                        blocks[x, y, z] = flatBlocks[flatIndex];
                    }
                }
            }

            if (NetworkWorld.instance.SpawnChunk(chunkCoord, blocks))
                _chunksToLoad--;

            if (_chunksToLoad <= 0)
            {
                NetworkWorld.instance.RenderChunks();
                OnAllChunksLoaded();
            }
        }


        private void RequestChunksInView(Vector3 position)
        {
            Vector3Int playerChunkPosition = new Vector3Int(
                Mathf.FloorToInt(position.x / NetworkWorld.ChunkWidth),
                0,
                Mathf.FloorToInt(position.z / NetworkWorld.ChunkWidth)
            );

            int loadDistance = NetworkWorld.instance.settings.loadDistance;

            _chunksToLoad = 0;

            for (int x = -loadDistance; x <= loadDistance; x++)
            {
                for (int z = -loadDistance; z <= loadDistance; z++)
                {
                    Vector3Int chunkPosition = new Vector3Int(
                        playerChunkPosition.x + x,
                        0,
                        playerChunkPosition.z + z
                    );

                    _chunksToLoad++;

                    SendMessageToServer(new
                    {
                        type = "get_chunk",
                        position = new { x = chunkPosition.x, y = chunkPosition.y, z = chunkPosition.z }
                    });
                }
            }
        }


        private void HandlePlayerUpdate(JsonElement data)
        {
            string playerId = data.GetProperty("player_id").GetString();
            Vector3 position = JsonToVector3(data.GetProperty("position"));

            LogDebug($"[Client] Update for player {playerId}: position {position}");

            if (_otherPlayers.TryGetValue(playerId, out GameObject player))
            {
                player.transform.position = position;
            }
            else
            {
                GameObject newPlayer = Instantiate(otherPlayerPrefab, position, Quaternion.identity);
                _otherPlayers[playerId] = newPlayer;
            }
        }

        private void OnAllChunksLoaded()
        {
            Debug.Log("[Client] All chunks have been loaded. Spawning player.");
            _player = Instantiate(playerPrefab, _playerPosition, Quaternion.identity);

            // Инициализируйте интерфейсы, контроллер игрока и другие компоненты
            UiManager.Instance.PlayerController = _player.GetComponent<PlayerController>();
            UiManager.Instance.Initialize();
            UiManager.Instance.CloseLoadingScreen();

            moveCoroutine = StartCoroutine(SendPlayerPositionRepeatedly());
        }

        private void HandleBlockUpdate(JsonElement data)
        {
            Vector3Int position = JsonToVector3Int(data.GetProperty("position"));
            int newBlockType = data.GetProperty("block_type").GetInt32();
            LogDebug($"[Client] Block updated at {position} to type {newBlockType}");

            NetworkWorld.instance.UpdateBlock(position, newBlockType);
        }


        private Vector3Int JsonToVector3Int(JsonElement json)
        {
            return new Vector3Int(
                json.GetProperty("x").GetInt32(),
                json.GetProperty("y").GetInt32(),
                json.GetProperty("z").GetInt32()
            );
        }

        private Vector3 JsonToVector3(JsonElement json)
        {
            float x = json.GetProperty("x").GetSingle();
            float y = json.GetProperty("y").GetSingle();
            float z = json.GetProperty("z").GetSingle();
            return new Vector3(x, y, z);
        }

        private Vector3 JsonToVector3Safe(JsonElement data, string propertyName)
        {
            if (data.TryGetProperty(propertyName, out JsonElement positionElement))
            {
                return JsonToVector3(positionElement);
            }

            // Если свойства нет, возвращаем вектор (0, 0, 0)
            Debug.LogWarning($"[Client] Property '{propertyName}' not found. Defaulting position to (0, 0, 0).");
            return Vector3.zero;
        }

        public void SendBlockPlaced(Vector3 position, int blockType)
        {
            SendMessageToServer(new
            {
                type = "place_block",
                position = new
                {
                    x = position.x,
                    y = position.y,
                    z = position.z
                },
                block_type = blockType
            });
        }

        public void SendBlockDestroyed(Vector3 position)
        {
            SendMessageToServer(new
            {
                type = "destroy_block",
                position = new
                {
                    x = position.x,
                    y = position.y,
                    z = position.z
                },
            });
        }

        private void SendMessageToServer(object message)
        {
            string jsonMessage = JsonSerializer.Serialize(message);
            Debug.Log(jsonMessage);
            Debug.Log(Encoding.UTF8.GetBytes(jsonMessage).ToString());
            _webSocket.SendAsync(Encoding.UTF8.GetBytes(jsonMessage));
        }

        private void OnApplicationQuit()
        {
            if (_webSocket != null)
            {
                _webSocket.CloseAsync();
            }
        }

        private IEnumerator SendPlayerPositionRepeatedly()
        {
            while (true)
            {
                ServerMassageMove(_player);
                yield return null;
            }
        }

        public void ServerMassageMove(GameObject player)
        {
            SendMessageToServer(new
            {
                type = "move",
                player = _playerName,
                position = new
                {
                    x = player.transform.position.x,
                    y = player.transform.position.y,
                    z = player.transform.position.z
                }
            });
        }

        private void HandleEvent(JsonElement data)
        {
            string eventType = data.GetProperty("event_type").GetString();
            LogDebug($"[Client] Event received: {eventType}");
        }

        private void LogDebug(string message)
        {
            if (enableLogging)
            {
                Debug.Log(message);
            }
        }

        private void LogWarning(string message)
        {
            if (enableLogging)
            {
                Debug.LogWarning(message);
            }
        }

        private void LogError(string message)
        {
            if (enableLogging)
            {
                Debug.LogError(message);
            }
        }
    }
}