using System;
using System.Collections;
using MultiCraft.Scripts.Engine.Core.Items;
using MultiCraft.Scripts.Engine.Core.Worlds;
using MultiCraft.Scripts.Engine.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MultiCraft.Scripts.Engine.Core.Entities
{
    public class Mob : MonoBehaviour
    {
        public int health = 10;
        public int maxHealth = 10;
        public MobDrop drop;

        public float maxMoveSpeed = 2f;
        public float moveSpeed = 2f; // Скорость движения
        public float jumpForce = 5f; // Сила прыжка
        public float detectionRange = 2f; // Дальность обнаружения препятствий
        public LayerMask obstacleLayer; // Слой для препятствий
        public float rotationSpeed = 5f; // Скорость поворота
        public float minMoveDistance = 3f; // Минимальное расстояние, которое пройдет моб
        public float maxMoveDistance = 10f; // Максимальное расстояние, которое пройдет моб
        public float minStopDuration = 2f; // Время остановки в секундах
        public float maxStopDuration = 15f; // Время остановки в секундах

        private Vector3 _targetDirection; // Направление движения
        private Rigidbody _rigidbody;
        private bool _isGrounded; // Флаг, находится ли объект на земле
        private bool _isMoving = true; // Флаг, двигается ли моб
        private float _remainingDistance; // Оставшееся расстояние до следующей остановки
        private MeshRenderer _meshRenderer;

        private void Start()
        {
            moveSpeed = maxMoveSpeed;
            _meshRenderer = GetComponent<MeshRenderer>();
            _rigidbody = GetComponent<Rigidbody>();
            SetRandomDirection();
            SetRandomDistance();
            StartCoroutine(MovementRoutine());
        }

        private void Update()
        {
            if (_isMoving)
            {
                Move();
            }
        }

        private void Move()
        {
            if (transform.position.y < 0)
            {
                Destroy(gameObject);
            }

            // Точка, куда моб планирует переместиться
            Vector3 plannedPosition = transform.position + _targetDirection * moveSpeed * Time.deltaTime;

            // Проверка, есть ли платформа под планируемой точкой
            if (!Physics.Raycast(plannedPosition, Vector3.down, detectionRange, obstacleLayer))
            {
                SetRandomDirection(); // Сменить направление
                return; // Прекратить текущий цикл движения
            }

            // Проверка, нужно ли сменить направление
            if (Physics.Raycast(transform.position, _targetDirection, detectionRange, obstacleLayer))
            {
                if (IsObstacleClimbable())
                {
                    Jump(); // Прыжок на препятствие
                }
                else
                {
                    SetRandomDirection(); // Сменить направление
                }
            }

            // Движение
            Vector3 move = _targetDirection * moveSpeed * Time.deltaTime;
            transform.position += new Vector3(move.x, 0, move.z);

            // Уменьшение оставшегося расстояния
            _remainingDistance -= moveSpeed * Time.deltaTime;

            // Поворот в сторону движения
            RotateTowards(_targetDirection);

            // Если расстояние пройдено, остановка
            if (_remainingDistance <= 0)
            {
                StopMovement();
            }
        }

        private void RotateTowards(Vector3 direction)
        {
            if (direction == Vector3.zero) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            targetRotation = Quaternion.Euler(-90, targetRotation.eulerAngles.y, targetRotation.eulerAngles.z);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        private void SetRandomDirection()
        {
            // Генерация случайного направления
            _targetDirection = new Vector3(
                Random.Range(-1f, 1f),
                0,
                Random.Range(-1f, 1f)
            ).normalized;
        }

        private bool IsObstacleClimbable()
        {
            // Проверка, есть ли препятствие высотой до 1 блока
            return Physics.Raycast(transform.position + _targetDirection * detectionRange, Vector3.up, 1f,
                obstacleLayer);
        }

        private void Jump()
        {
            if (_isGrounded)
            {
                _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Проверка, находится ли объект на земле
            if (collision.contacts[0].normal.y > 0.5f)
            {
                _isGrounded = true;
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            _isGrounded = false;
        }

        private void SetRandomDistance()
        {
            _remainingDistance = Random.Range(minMoveDistance, maxMoveDistance);
        }

        private void StopMovement()
        {
            _isMoving = false;
            StartCoroutine(StopRoutine());
        }

        private IEnumerator StopRoutine()
        {
            yield return new WaitForSeconds(Random.Range(minStopDuration, maxStopDuration));
            SetRandomDirection();
            SetRandomDistance();
            _isMoving = true;
            moveSpeed = maxMoveSpeed;
        }

        private IEnumerator MovementRoutine()
        {
            while (true)
            {
                if (!_isMoving)
                {
                    yield return null; // Ждем, пока моб стоит
                }
                else
                {
                    yield return null; // Ждем следующий кадр, пока идет движение
                }
            }
        }

        public IEnumerator TakeDamage(int damage, Vector3 direction)
        {
            var material = _meshRenderer.material;
            health = Mathf.Clamp(health - damage, 0, maxHealth);
            material.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            StartCoroutine(StopRoutine());
            moveSpeed *= 2;
            Move();
            material.color = Color.white;
            if (health <= 0)
            {
                World.Instance.DropItem(transform.position, ResourceLoader.Instance.GetItem(drop.item), drop.amount);
                Destroy(gameObject);
            }
        }
    }

    [Serializable]
    public class MobDrop
    {
        public ItemScriptableObject item;
        public int amount;
    }
}