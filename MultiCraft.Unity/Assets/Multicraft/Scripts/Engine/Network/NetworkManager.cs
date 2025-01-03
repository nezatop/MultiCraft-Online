using UnityEngine;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using MultiCraft.Scripts.Engine.Core.Inventories;
using MultiCraft.Scripts.Engine.Core.Player;
using MultiCraft.Scripts.Engine.Network.Player;
using MultiCraft.Scripts.Engine.Network.Worlds;
using MultiCraft.Scripts.Engine.UI;
using MultiCraft.Scripts.Engine.Utils;
using MultiCraft.Scripts.UI.Authorize;
using UnityEngine.SceneManagement;
using UnityWebSocket;

namespace MultiCraft.Scripts.Engine.Network
{
    public class NetworkManager : MonoBehaviour
    {
        private static readonly int VelocityX = Animator.StringToHash("VelocityX");
        private static readonly int VelocityY = Animator.StringToHash("VelocityY");
        private static readonly int VelocityZ = Animator.StringToHash("VelocityZ");

        #region Parametrs

        public static NetworkManager instance { get; private set; }

        [Header("Debug Settings")] public bool enableLogging = true;

        public GameObject playerPrefab;
        public OtherNetPlayer otherPlayerPrefab;
        private Dictionary<string, OtherNetPlayer> _otherPlayers;

        public string serverAddress = "wss://ms-mult.onrender.com";

        private WebSocket _webSocket;

        public string playerName;
        private string _playerPassword;

        private GameObject _player;
        private Vector3 _playerPosition;

        private int _chunksToLoad;

        private ConcurrentQueue<Vector3Int> _chunks;
        private bool _spawnChunks;

        #endregion

        #region Initialization

        private void Start()
        {
            instance = this;

            NetworkWorld.instance.RenderAllChunks += OnAllChunksLoaded;

            _chunks = new ConcurrentQueue<Vector3Int>();
            _otherPlayers = new Dictionary<string, OtherNetPlayer>();

            if (PlayerPrefs.HasKey("UserData"))
            {
                string jsonData = PlayerPrefs.GetString("UserData");
                var userData = JsonUtility.FromJson<UserData>(jsonData);
                playerName = userData.username;
                _playerPassword = userData.password;
            }
            else
            {
                playerName = Guid.NewGuid().ToString();
                _playerPassword = Guid.NewGuid().ToString();
            }

            _webSocket = new WebSocket(serverAddress);

            _webSocket.OnOpen += OnOpen;
            _webSocket.OnClose += OnClose;
            _webSocket.OnMessage += OnMessage;
            _webSocket.OnError += OnError;

            _webSocket.ConnectAsync();
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            LogError($"[Client] Error: {e}");
        }

        private void OnMessage(object sender, MessageEventArgs e)
        {
            string message = Encoding.UTF8.GetString(e.RawData);
            //LogDebug($"[MassageFromServer] {message}");
            HandleServerMessage(message);
        }

        private void OnClose(object sender, CloseEventArgs e)
        {
            SceneManager.LoadScene("MainMenu");
            LogDebug($"[Client] Connection closed. Reason: {e}");
        }

        private void OnOpen(object sender, OpenEventArgs e)
        {
            LogDebug("[Client] WebSocket connection opened.");
            SendMessageToServer(new { type = "connect", login = playerName, password = _playerPassword });
        }

        private void OnApplicationQuit()
        {
            _webSocket?.CloseAsync();
        }

        #endregion

        private void Update()
        {
            if (!_spawnChunks) return;

            if (_chunks.TryDequeue(out var chunkCoord))
            {
                StartCoroutine(NetworkWorld.instance.RenderChunks(chunkCoord));
                StartCoroutine(NetworkWorld.instance.RenderWaterChunks(chunkCoord));
                StartCoroutine(NetworkWorld.instance.RenderFloraChunks(chunkCoord));
            }
            else
            {
                NetworkWorld.instance.canSpawnPlayer = true;
            }
        }

        #region HandleServerMessage

        private void HandleServerMessage(string data)
        {
            try
            {
                var message = JsonDocument.Parse(data);
                var type = message.RootElement.GetProperty("type").GetString();

                switch (type)
                {
                    case "connected":
                        OnConnected(message.RootElement);
                        SendMessageToServer(new { type = "get_players" });
                        break;

                    case "damage":
                        HandleDamage(message.RootElement);
                        break;

                    case "player_connected":
                        OnPlayerConnected(message.RootElement);
                        break;

                    case "player_moved":
                        OnPlayerMoved(message.RootElement);
                        break;

                    case "player_disconnected":
                        OnPlayerDisconnected(message.RootElement);
                        break;

                    case "players_list":
                        OnPlayersListReceived(message.RootElement);
                        break;

                    case "chunk_data":
                        HandleChunkData(message.RootElement);
                        break;

                    case "player_update":
                        HandlePlayerUpdate(message.RootElement);
                        break;

                    case "block_update":
                        HandleBlockUpdate(message.RootElement);
                        break;

                    case "inventory":
                        HandleInventoryGet(message.RootElement);
                        break;

                    case "chat":
                        HandleChat(message.RootElement);
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

        #endregion

        #region Handleplayers

        private void OnConnected(JsonElement data)
        {
            Vector3 position = JsonToVector3Safe(data, "position");
            _playerPosition = position;
            NetworkWorld.instance.currentPosition = Vector3Int.FloorToInt(_playerPosition);
            RequestChunksInView(position);
        }

        private void OnPlayerConnected(JsonElement data)
        {
            string playerId = data.GetProperty("player_id").GetString();
            Vector3 position = JsonToVector3(data.GetProperty("position"));

            UiManager.Instance.ChatWindow.commandReader.PrintLog($"{playerId}: Зашел на сервер");

            if (playerId != null && !_otherPlayers.ContainsKey(playerId) && playerId != playerName)
            {
                var playerObject = Instantiate(otherPlayerPrefab, position, Quaternion.identity);
                playerObject.playerName = playerId;
                playerObject.Init();
                _otherPlayers[playerId] = playerObject;
            }
        }

        private void OnPlayerMoved(JsonElement data)
        {
            string playerId = data.GetProperty("player_id").GetString();
            Vector3 targetPosition = JsonToVector3(data.GetProperty("position"));
            
            if (playerId != null && _otherPlayers.TryGetValue(playerId, out var player))
            {
                Animator playerAnimator = player.animator;
                float smoothSpeed = 0.1f; 
                Vector3 velocity = Vector3.zero;
                
                player.transform.position =
                    Vector3.SmoothDamp(player.transform.position, targetPosition, ref velocity, smoothSpeed);

                Vector3 movement = velocity; 

                playerAnimator.SetFloat(VelocityX, movement.x);
                playerAnimator.SetFloat(VelocityY, movement.y);
                playerAnimator.SetFloat(VelocityZ, movement.z);
            }
        }

        private void OnPlayerDisconnected(JsonElement data)
        {
            string playerId = data.GetProperty("player_id").GetString();

            UiManager.Instance.ChatWindow.commandReader.PrintLog($"{playerId}: Вышел с сервера сервер");
            if (playerId != null && _otherPlayers.ContainsKey(playerId))
            {
                Destroy(_otherPlayers[playerId]);
                _otherPlayers.Remove(playerId);
            }
        }

        private void HandlePlayerUpdate(JsonElement data)
        {
            string playerId = data.GetProperty("player_id").GetString();
            Vector3 position = JsonToVector3(data.GetProperty("position"));

            if (playerId != null && _otherPlayers.TryGetValue(playerId, out OtherNetPlayer player))
            {
                player.transform.position = position;
            }
            else
            {
                var playerObject = Instantiate(otherPlayerPrefab, position, Quaternion.identity);
                playerObject.playerName = playerId;
                playerObject.Init();
                if (playerId != null) _otherPlayers[playerId] = playerObject;
            }
        }

        private void OnPlayersListReceived(JsonElement data)
        {
            var players = data.GetProperty("players");

            foreach (JsonElement player in players.EnumerateArray())
            {
                string playerId = player.GetProperty("player_id").GetString();
                Vector3 position = JsonToVector3(player.GetProperty("position"));

                if (playerId != null && !_otherPlayers.ContainsKey(playerId) && playerId != playerName)
                {
                    var playerObject = Instantiate(otherPlayerPrefab, position, Quaternion.identity);
                    playerObject.playerName = playerId;
                    playerObject.Init();
                    _otherPlayers[playerId] = playerObject;
                }
            }
        }

        private void HandleBlockUpdate(JsonElement data)
        {
            Vector3Int position = JsonToVector3Int(data.GetProperty("position"));
            int newBlockType = data.GetProperty("block_type").GetInt32();

            NetworkWorld.instance.UpdateBlock(position, newBlockType);
        }

        private void HandleDamage(JsonElement data)
        {
            var attackTarget = data.GetProperty("attack_target").ToString();
            var damage = int.Parse(data.GetProperty("damage").ToString());

            _otherPlayers[attackTarget].health.TakeDamage(damage);
        }

        private IEnumerator SendPlayerPositionRepeatedly()
        {
            while (true)
            {
                ServerMassageMove(_player);
                yield return null;
            }
            // ReSharper disable once IteratorNeverReturns
        }

        #endregion

        #region Chat

        private void HandleChat(JsonElement data)
        {
            string playerId = data.GetProperty("player_id").GetString();
            string massage = data.GetProperty("chat_massage").GetString();

            if (playerId != null && playerId != playerName)
            {
                UiManager.Instance.ChatWindow.commandReader.PrintLog($"{playerId}: {massage}");
            }
        }

        #endregion

        #region HandleChunks

        private void HandleChunkData(JsonElement data)
        {
            Vector3Int chunkCoord = JsonToVector3Int(data.GetProperty("position"));

            JsonElement blocksJson = data.GetProperty("blocks");
            JsonElement waterBlocksJson = data.GetProperty("waterChunk");
            JsonElement floraBlocksJson = data.GetProperty("floraChunk");

            if (NetworkWorld.instance.SpawnChunk(chunkCoord, Blocks(blocksJson)) &&
                NetworkWorld.instance.SpawnWaterChunk(chunkCoord, Blocks(waterBlocksJson)) &&
                NetworkWorld.instance.SpawnFloraChunk(chunkCoord, Blocks(floraBlocksJson)))
                _chunksToLoad--;

            _chunks.Enqueue(chunkCoord);

            if (_chunksToLoad <= 0)
                _spawnChunks = true;
        }

        private int[,,] Blocks(JsonElement blocksJson)
        {
            var flatBlocks = new int[16 * 16 * 256];
            var blocks = new int[16, 256, 16];

            var index = 0;
            foreach (var blockValue in blocksJson.EnumerateArray())
            {
                flatBlocks[index] = blockValue.GetInt32();
                index++;
            }

            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 256; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        int flatIndex = x + z * 16 + y * 16 * 16;
                        blocks[x, y, z] = flatBlocks[flatIndex];
                    }
                }
            }

            return blocks;
        }

        private void RequestChunksInView(Vector3 position)
        {
            Vector3Int playerChunkPosition = new Vector3Int(
                Mathf.FloorToInt(position.x / 16),
                0,
                Mathf.FloorToInt(position.z / 16)
            );

            int loadDistance = NetworkWorld.instance.settings.loadDistance;

            _chunksToLoad = 0;

            for (int x = -loadDistance; x <= loadDistance; x++)
            {
                for (int z = -loadDistance; z <= loadDistance; z++)
                {
                    RequestChunk(new Vector3Int(
                        playerChunkPosition.x + x,
                        0,
                        playerChunkPosition.z + z
                    ));
                }
            }
        }

        public void RequestChunk(Vector3Int chunkPosition)
        {
            _chunksToLoad++;

            SendMessageToServer(new
            {
                type = "get_chunk",
                position = new { chunkPosition.x, chunkPosition.y, chunkPosition.z }
            });
        }

        private void OnAllChunksLoaded()
        {
            if (_player) return;

            _player = Instantiate(playerPrefab, _playerPosition, Quaternion.identity);

            NetworkWorld.instance.player = _player;

            UiManager.Instance.PlayerController = _player.GetComponent<PlayerController>();
            UiManager.Instance.Initialize();
            UiManager.Instance.CloseLoadingScreen();


            NetworkWorld.instance.RenderAllChunks -= OnAllChunksLoaded;

            foreach (var otherPlayers in _otherPlayers.Values)
            {
                otherPlayers.cameraTransform = _player.GetComponentInChildren<Camera>().transform;
            }

            StartCoroutine(SendPlayerPositionRepeatedly());
        }

        #endregion

        #region Inventory

        private void HandleInventoryGet(JsonElement data)
        {
            var slots = JsonToInventory(data.GetProperty("inventory"));

            UiManager.Instance.OpenCloseChest(slots,
                Vector3Int.FloorToInt(JsonToVector3(data.GetProperty("position"))));
        }

        public void GetInventory(Vector3 chestPosition)
        {
            SendMessageToServer(new
            {
                type = "get_inventory",
                position = new { chestPosition.x, chestPosition.y, chestPosition.z }
            });
        }

        public void SetInventory(Vector3Int chestPosition, List<ItemInSlot> slots)
        {
            var slotsJson = JsonToInventory(slots);

            SendMessageToServer(new
            {
                type = "set_inventory",
                position = new { chestPosition.x, chestPosition.y, chestPosition.z },
                inventory = slotsJson
            });
        }

        #endregion

        #region Utils

        private Vector3Int JsonToVector3Int(JsonElement json)
        {
            return new Vector3Int(
                json.GetProperty("x").GetInt32(),
                json.GetProperty("y").GetInt32(),
                json.GetProperty("z").GetInt32()
            );
        }

        private string JsonToInventory(List<ItemInSlot> slots)
        {
            List<ItemJson> slotsJson = new List<ItemJson>();

            foreach (var slot in slots)
            {
                var item = new ItemJson();
                if (slot == null)
                {
                    item.type = "null";
                    item.count = 0;
                    item.durability = 0;
                }
                else
                {
                    item = new ItemJson()
                    {
                        type = slot.Item.Name,
                        count = slot.Amount,
                        durability = slot.Durability,
                    };
                }

                slotsJson.Add(item);
            }

            return JsonSerializer.Serialize(slotsJson);
        }


        private List<ItemInSlot> JsonToInventory(JsonElement json)
        {
            List<ItemInSlot> inventory = new List<ItemInSlot>();

            List<ItemJson> items = json.Deserialize<List<ItemJson>>();

            foreach (var itemJson in items)
            {
                var item = new ItemInSlot
                {
                    Amount = itemJson.count,
                    Durability = itemJson.durability,
                    Item = itemJson.type != "null" ? ResourceLoader.Instance.GetItem(itemJson.type) : null
                };

                inventory.Add(item);
            }

            return inventory;
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

            Debug.LogWarning($"[Client] Property '{propertyName}' not found. Defaulting position to (0, 0, 0).");
            return Vector3.zero;
        }

        #endregion

        #region SendToServer

        public void SendMessageToServerChat(string massage)
        {
            SendMessageToServer(new
            {
                type = "chat",
                player = playerName,
                chat_massage = massage
            });
        }

        public void SendBlockPlaced(Vector3 position, int blockType)
        {
            SendMessageToServer(new
            {
                type = "place_block",
                position = new
                {
                    position.x,
                    position.y,
                    position.z
                },
                block_type = blockType
            });
        }

        public void ServerMassageAttack(int damage, string attackTarget)
        {
            SendMessageToServer(new
            {
                type = "Attack",
                player = playerName,
                attack_target = attackTarget,
                damage,
            });
        }

        public void SendBlockDestroyed(Vector3 position)
        {
            SendMessageToServer(new
            {
                type = "destroy_block",
                position = new
                {
                    position.x,
                    position.y,
                    position.z
                },
            });
        }

        private void SendMessageToServer(object message)
        {
            string jsonMessage = JsonSerializer.Serialize(message);
            //LogDebug($"[MassageToServer] {jsonMessage}");
            _webSocket.SendAsync(Encoding.UTF8.GetBytes(jsonMessage));
        }

        private void ServerMassageMove(GameObject player)
        {
            SendMessageToServer(new
            {
                type = "move",
                player = playerName,
                position = new
                {
                    player.transform.position.x,
                    player.transform.position.y,
                    player.transform.position.z
                }
            });
        }

        #endregion

        #region Logs

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

        #endregion
    }

    public class ItemJson
    {
        public string type { get; set; }
        public int count { get; set; }
        public int durability { get; set; }
    }
}