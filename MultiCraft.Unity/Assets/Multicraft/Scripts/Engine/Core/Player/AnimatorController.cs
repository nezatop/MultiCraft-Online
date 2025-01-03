using UnityEngine;

namespace MultiCraft.Scripts.Engine.Core.Player
{
    public class AnimatorController : MonoBehaviour
    {
        private Animator animator;
    private Rigidbody playerRigidbody;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.1f;

    [Header("Player Settings")]
    public Transform groundCheck;
    public float crouchSpeed = 2f;

    void Start()
    {
        animator = GetComponent<Animator>();
        playerRigidbody = GetComponent<Rigidbody>();

        if (!animator)
        {
            Debug.LogError("Animator не найден на объекте!");
        }

        if (!groundCheck)
        {
            Debug.LogError("Точка проверки земли (Ground Check) не назначена!");
        }
    }

    void Update()
    {
        UpdateAnimatorParameters();
    }

    private void UpdateAnimatorParameters()
    {
        // Скорость игрока
        Vector3 velocity = playerRigidbody.linearVelocity;
        animator.SetFloat("VelocityX", velocity.x);
        animator.SetFloat("VelocityY", velocity.y);
        animator.SetFloat("VelocityZ", velocity.z);

        // Проверка на землю
        bool isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckDistance, groundLayer);
        animator.SetBool("Grounded", isGrounded);

        // Положение тела
        float uprightValue = transform.up.y;
        animator.SetFloat("Upright", uprightValue);

        // Переключение анимации сидя (нажатием Ctrl)
        if (Input.GetKey(KeyCode.LeftControl))
        {
            animator.SetBool("Seated", true);
        }
        else
        {
            animator.SetBool("Seated", false);
        }

        // Проверка активности (если персонаж стоит)
        if (Input.GetKey(KeyCode.C))
        {
            animator.SetBool("AFK", true);
        }
        else
        {
            animator.SetBool("AFK", false);
        }

        // Временная отправка Punch (если нажата клавиша)
        if (Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger("Punch");
        }

        // Получение урона
        if (Input.GetKeyDown(KeyCode.H))
        {
            animator.SetTrigger("Hurt");
        }

        // Смерть
        if (Input.GetKeyDown(KeyCode.K))
        {
            animator.SetBool("Die", true);
        }
    }
    }
}