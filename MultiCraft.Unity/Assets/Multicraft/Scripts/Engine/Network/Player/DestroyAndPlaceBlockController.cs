
using MultiCraft.Scripts.Engine.Core.Blocks;
using MultiCraft.Scripts.Engine.Core.Entities;
using MultiCraft.Scripts.Engine.Core.Inventories;
using MultiCraft.Scripts.Engine.Core.Player;
using MultiCraft.Scripts.Engine.Network.Worlds;
using MultiCraft.Scripts.Engine.UI;
using UnityEngine;

namespace MultiCraft.Scripts.Engine.Network.Player
{
    public class DestroyAndPlaceBlockController : MonoBehaviour
    {
        private static readonly int DamageAmount = Shader.PropertyToID("_DamageAmount");

        public HandAnimationController handAnimationController; // Ссылка на контроллер анимации

        public LayerMask worldLayer;
        public LayerMask mobLayer;

        public GameObject destroyedBlockPrefab;

        private Inventory _playerInventory;

        private GameObject _destroyBlock;

        private Material _material;

        public float breakSpeed = 1f;
        public float miningMultiplier = 1;
        private float _currentDamage = 0f;
        private Block _currentBlock;
        private Vector3 _targetBlockPosition;

        public float placeDelay = 0.5f;
        private float _nextPlaceTime = 0f;

        public bool _isSurvival = false;

        private bool _isBreaking;
        private bool _isAnimating;

        private float _nextDestroyTime = 0f;
        public float destroyDelayNonSurvival = 0.1f; // Задержка разрушения блоков в режиме без выживания


        public void Start()
        {
            if (_destroyBlock == null)
                _destroyBlock = Instantiate(destroyedBlockPrefab, Vector3.zero, Quaternion.Euler(0f, 0f, 0f));

            _playerInventory = gameObject.GetComponentInParent<Inventory>();

            _destroyBlock.SetActive(false);
            _material = _destroyBlock.GetComponent<MeshRenderer>().material;
        }

        private void OnDisable()
        {
            if (_destroyBlock != null)
                _destroyBlock.SetActive(false);
            _currentBlock = null;
            _currentDamage = 0f;
        }

        private void Update()
        {
            var selectedItem = _playerInventory.GetSelectedItem();
            if (selectedItem != null)
            {
                if (selectedItem.Item != null)
                {
                    miningMultiplier = selectedItem.Item.MiningMultiplier;
                    miningMultiplier = miningMultiplier == 0 ? 1 : miningMultiplier;
                }
            }
            else
            {
                miningMultiplier = 1;
            }

            if (Physics.Raycast(transform.position, transform.forward, out var hitInfo, 5f, worldLayer))
            {
                var inCube = hitInfo.point - (hitInfo.normal * 0.5f);
                var removeBlock = Vector3Int.FloorToInt(inCube);

                _destroyBlock.transform.position = removeBlock + new Vector3(.5f, .5f, .5f);
            }
            else
            {
                if (_isSurvival) _destroyBlock.SetActive(false);
            }

            if (Input.GetMouseButtonDown(1))
            {
                //if (TryOpenBlock()) return;
            }

            if (Input.GetMouseButton(1))
            {
                TryPlaceBlock();
            }
            else if (Input.GetMouseButtonUp(1))
            {
                _nextPlaceTime = 0f;
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (TryAttact()) return;
            }

            if (Input.GetMouseButtonDown(0)) // ЛКМ для начала разрушения
            {
                _isBreaking = true;
                handAnimationController.StartBreakingAnimation();
                _isAnimating = true;
            }

            if (Input.GetMouseButtonUp(0)) // Отпускание ЛКМ
            {
                _isBreaking = false;
                handAnimationController.StopBreakingAnimation();
                _isAnimating = false;
            }

            if (Input.GetMouseButton(0))
            {
                if (_isBreaking)
                {
                    if (_isAnimating == false)
                        handAnimationController.StartBreakingAnimation();
                    TryDestroyBlock(); // Продолжать ломать блок
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                _currentDamage = 0f;
                _currentBlock = null;
                if (_isSurvival) _destroyBlock.SetActive(false);
            }
        }

        private bool TryAttact()
        {
            if (!Physics.Raycast(transform.position, transform.forward, out var hitInfo, 5f, mobLayer)) return false;
            GameObject hitObject = hitInfo.collider.gameObject;

            // Выводим информацию о попадании
            Debug.Log($"Hit object: {hitObject.name}");

            // Здесь можно добавить логику атаки, если объект является мобом или врагом
            // Например:
            var Mob = hitObject.GetComponent<Mob>();
            if (Mob != null)
            {
                StartCoroutine(Mob.TakeDamage(1, -hitInfo.normal));
            }

            return true;
        }

        private void TryPlaceBlock()
        {
            if (Time.time < _nextPlaceTime) return;

            if (!Physics.Raycast(transform.position, transform.forward, out var hitInfo, 5f, worldLayer)) return;

            var blockPosition = hitInfo.point + hitInfo.normal * 0.5f;
            if (Vector3Int.FloorToInt(transform.position) != Vector3Int.FloorToInt(blockPosition) &&
                Vector3Int.FloorToInt(transform.position + Vector3.down) != Vector3Int.FloorToInt(blockPosition))
            {
                var selectedItem = _playerInventory.GetSelectedItem();
                if (selectedItem != null)
                    if (selectedItem.Item != null)
                        if (selectedItem.Item.IsPlaceable)
                        {
                            NetworkWorld.instance.SpawnBlock(blockPosition, selectedItem.Item.BlockType.Id);
                            if (_isSurvival) _playerInventory.RemoveSelectedItem();

                            handAnimationController.PlayPlaceAnimation();
                        }

                _nextPlaceTime = Time.time + placeDelay;
            }
        }
/*
        private bool TryOpenBlock()
        {
            if (!Physics.Raycast(transform.position, transform.forward, out var hitInfo, 5f, worldLayer)) return false;

            var blockPosition = hitInfo.point - hitInfo.normal * 0.5f;

            var block = NetworkWorld.instance.GetBlockAtPosition(blockPosition);
            if (block.HaveInventory)
            {
                UiManager.Instance.OpenCloseChest(NetworkWorld.instance.GetInventory(blockPosition),
                    Vector3Int.FloorToInt(blockPosition));
                GetComponentInParent<InteractController>().DisableScripts();
                return true;
            }

            return false;
        }
*/
        private void TryDestroyBlock()
        {
            if (!Physics.Raycast(transform.position, transform.forward, out var hitInfo, 5f, worldLayer)) return;
            if (_isSurvival) _destroyBlock.SetActive(true);
            var blockPosition = Vector3Int.FloorToInt(hitInfo.point - hitInfo.normal * 0.5f);

            if (_currentBlock == null || _targetBlockPosition != blockPosition)
            {
                _currentDamage = 0f;
                _targetBlockPosition = blockPosition;
                _currentBlock = NetworkWorld.instance.GetBlockAtPosition(blockPosition);
            }

            if (!_isSurvival)
            {
                // Задержка разрушения в режиме без выживания
                if (Time.time < _nextDestroyTime) return;
                _nextDestroyTime = Time.time + destroyDelayNonSurvival;

                NetworkWorld.instance.DestroyBlock(blockPosition);
                _playerInventory.RemoveDurability();
                _currentBlock = null;
                if (_isSurvival)
                    if (_isSurvival)
                        _destroyBlock.SetActive(false);
                handAnimationController.StopBreakingAnimation();
                _isAnimating = false;
                return;
            }

            if (_currentBlock.Durability > 0) _currentDamage += breakSpeed * Time.deltaTime * miningMultiplier;
            _material.SetFloat(DamageAmount, _currentDamage / _currentBlock.Durability);

            if (_currentDamage >= _currentBlock.Durability && _currentBlock.Durability > 0)
            {
                NetworkWorld.instance.DestroyBlock(blockPosition);
                _playerInventory.RemoveDurability();
                _currentDamage = 0f;
                _currentBlock = null;
                if (_isSurvival) _destroyBlock.SetActive(false);
                handAnimationController.StopBreakingAnimation();
                _isAnimating = false;
            }
        }
    }
}