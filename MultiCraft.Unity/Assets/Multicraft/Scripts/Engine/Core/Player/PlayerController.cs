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

        public float fallThreshold = 3.0f;
        public float fallDamageMultiplier = 0.5f;

        public AudioSource footstepAudioSource;
        public AudioSource jumpAudioSource;
        public AudioClip[] footstepSounds;
        public AudioClip jumpSound;
        public float stepInterval = 0.5f;

        private CharacterController _controller;
        private Vector3 _velocity;
        private bool _isGrounded;

        public HandRenderer handRenderer;
        public DroppedItem DroppedItemPrefab;
        [SerializeField] private float DropForce = 5f;

        private Inventory _inventory;
        private float _fallStartY;
        private bool _isFalling;
        private Health _health;

        private float _stepTimer;

        public Animator animator;

        private int _currentValue = 0;
        private bool _crouching;
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

        private void Update()
        {
            HandleMovement();
            HandleItemInteraction();
            HandleAnimation();
        }

        private void HandleMovement()
        {
            _isGrounded = _controller.isGrounded;
            animator.SetBool("Grounded", _isGrounded);

            if (_isGrounded && _velocity.y < 0)
                _velocity.y = -2f;

            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;
            _controller.Move(moveDirection * (moveSpeed * Time.deltaTime));

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
                _stepTimer = 0f;
            }

            if (Input.GetButtonDown("Jump") && _isGrounded)
            {
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                PlayJumpSound();
            }

            _velocity.y += gravity * Time.deltaTime;
            _controller.Move(_velocity * Time.deltaTime);
        }


        public void Teleport(Vector3 position)
        {
            _velocity = Vector3
                .zero; // Отключаем текущую скорость, чтобы избежать странного поведения после телепортации
            _controller.enabled = false; // Отключаем CharacterController, чтобы избежать конфликтов
            transform.position = position;
            _controller.enabled = true; // Включаем обратно
        }


        private void HandleItemInteraction()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (scroll < 0f && _currentValue < MaxValue)
                _currentValue++;
            else if (scroll > 0f && _currentValue > MinValue)
                _currentValue--;

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
        }

        private void HandleAnimation()
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            bool isMoving = horizontalInput != 0 || verticalInput != 0;
            animator.SetFloat("VelocityX", horizontalInput*4);
            animator.SetFloat("VelocityZ", verticalInput*4);

            if (Input.GetKey(KeyCode.LeftControl))
            {
                _crouching = true;
                moveSpeed = 2.5f;
            }
            else
            {
                _crouching = false;
                moveSpeed = 5.0f;
            }

            var center = _controller.center;
            center.y = _crouching ? 0.6f : 0.21f;
            _controller.center = center;
            animator.SetFloat("Upright", !_crouching ? 1.0f : 0.0f);
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