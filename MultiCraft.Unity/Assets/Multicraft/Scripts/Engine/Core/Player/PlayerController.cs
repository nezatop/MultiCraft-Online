using System;
using MultiCraft.Scripts.Engine.Core.Entities;
using MultiCraft.Scripts.Engine.Core.HealthSystem;
using MultiCraft.Scripts.Engine.Core.Inventories;
using MultiCraft.Scripts.Engine.Core.MeshBuilders;
using MultiCraft.Scripts.Engine.Utils;
using Unity.VisualScripting;
using UnityEngine;

namespace MultiCraft.Scripts.Engine.Core.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public float moveSpeed = 5.0f;
        public float jumpHeight = 1.24f;
        public float gravity = -18f;

        public float fallThreshold = 3.0f; // Высота, с которой начинается урон
        public float fallDamageMultiplier = 0.5f; // Множитель урона за каждую единицу высоты выше порога

        public AudioSource footstepAudioSource; // Источник звука шагов
        public AudioSource jumpAudioSource; // Источник звука прыжка
        public AudioClip[] footstepSounds; // Массив звуков шагов
        public AudioClip jumpSound; // Звук прыжка
        public float stepInterval = 0.5f; // Интервал между шагами (в секундах)

        private CharacterController _controller;
        private Vector3 _velocity;
        private bool _isGrounded;

        public HandRenderer handRenderer;

        public DroppedItem DroppedItemPrefab;
        [SerializeField] private float DropForce = 5f;

        private Inventory _inventory;
        private float _fallStartY; // Высота начала падения
        private bool _isFalling; // Флаг для отслеживания состояния падения
        private Health _health; // Ссылка на компонент здоровья

        private float _stepTimer; // Таймер для шагов


        private int _currentValue = 0;
        private const int MinValue = 0;
        private const int MaxValue = 8;

        private void Start()
        {
            _inventory = GetComponent<Inventory>();
            _health = GetComponent<Health>();
            if (_health == null)
            {
                Debug.LogError("Health component not found on the player!");
            }

            _controller = GetComponent<CharacterController>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void Teleport(Vector3 position)
        {
            // Отключаем текущую скорость, чтобы избежать странного поведения после телепортации
            _velocity = Vector3.zero;

            // Перемещаем игрока с использованием CharacterController.Move
            _controller.enabled = false; // Отключаем CharacterController, чтобы избежать конфликтов
            transform.position = position;
            _controller.enabled = true; // Включаем обратно
        }

        private void Update()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (scroll < 0f)
            {
                if (_currentValue < MaxValue)
                    _currentValue++;
            }
            else if (scroll > 0f)
            {
                if (_currentValue > MinValue)
                    _currentValue--;
            }

            var handItem = _inventory.UpdateHotBarSelectedSlot(_currentValue);
            if (handItem != null)
            {
                if (handItem.BlockType != null)
                {
                    var block = ResourceLoader.Instance.GetBlock(handItem.BlockType.Id);
                    if (block != null)
                    {
                        var blockMesh = DropItemMeshBuilder.GeneratedMesh(block);
                        handRenderer.SetMesh(blockMesh);
                    }
                    else
                    { 
                        var itemMesh = DropItemMeshBuilder.GeneratedMesh(handItem);
                        handRenderer.RemoveMesh();
                    }
                }
            }
            else
            {
                handRenderer.RemoveMesh();
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                var item = _inventory.RemoveSelectedItem().Item;
                var camera = transform.GetChild(0);
                var droppedItem = Instantiate(DroppedItemPrefab, camera.position + Vector3.down * 0.2f,
                    Quaternion.identity);
                Rigidbody rb = droppedItem.GetComponent<Rigidbody>();
                Vector3 force = camera.transform.forward.normalized * DropForce;
                rb.AddForce(force, ForceMode.Impulse);
                droppedItem.Item = new ItemInSlot(item, 1);
                droppedItem.Init();
            }


            _isGrounded = _controller.isGrounded;

            // Обработка начала падения
            if (!_isGrounded && !_isFalling)
            {
                _fallStartY = transform.position.y;
                _isFalling = true;
            }

            // Обработка приземления
            if (_isGrounded && _isFalling)
            {
                float fallDistance = _fallStartY - transform.position.y;
                if (fallDistance > fallThreshold)
                {
                    ApplyFallDamage(fallDistance);
                }

                _isFalling = false;
            }

            // Гравитация и движение
            if (_isGrounded && _velocity.y < 0)
                _velocity.y = -2f;

            var horizontalInput = Input.GetAxis("Horizontal");
            var verticalInput = Input.GetAxis("Vertical");

            var moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;
            _controller.Move(moveDirection * (moveSpeed * Time.deltaTime));

            // Воспроизведение звуков шагов
            if (_isGrounded && moveDirection.magnitude > 0)
            {
                _stepTimer += Time.deltaTime;
                if (_stepTimer >= stepInterval)
                {
                    PlayFootstepSound();
                    _stepTimer = 0f;
                }
            }
            else
            {
                _stepTimer = 0f; // Сброс таймера, если игрок не двигается
            }

            // Прыжок
            if (Input.GetButtonDown("Jump") && _isGrounded)
            {
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                PlayJumpSound(); // Воспроизведение звука прыжка
            }

            _velocity.y += gravity * Time.deltaTime;
            _controller.Move(_velocity * Time.deltaTime);
        }

        private void ApplyFallDamage(float fallDistance)
        {
            if (_health != null)
            {
                float damage = (fallDistance - fallThreshold) * fallDamageMultiplier;
                _health.TakeDamage(Mathf.RoundToInt(damage));
            }
        }

        private void PlayFootstepSound()
        {
            if (footstepSounds.Length > 0 && footstepAudioSource != null)
            {
                int randomIndex = UnityEngine.Random.Range(0, footstepSounds.Length);
                footstepAudioSource.PlayOneShot(footstepSounds[randomIndex]);
            }
        }

        private void PlayJumpSound()
        {
            if (jumpAudioSource != null && jumpSound != null)
            {
                jumpAudioSource.PlayOneShot(jumpSound);
            }
        }
    }
}