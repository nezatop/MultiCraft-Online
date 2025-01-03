using UnityEngine;

namespace MultiCraft.Scripts.Engine.Core.Player
{
    public class CameraController : MonoBehaviour
    {
        public float sensitivity = 2.0f;
        public float maxYAngle = 80.0f;

        public Transform head; // Ссылка на голову (объект с камерой)
        public Transform body; // Ссылка на тело игрока

        private float _rotationY;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked; // Блокируем курсор
            Cursor.visible = false;                   // Скрываем курсор
        }

        private void Update()
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // Вращаем тело игрока по горизонтальной оси (вокруг Y)
            body.Rotate(Vector3.up * (mouseX * sensitivity));

            // Изменяем угол вращения головы по вертикали
            _rotationY -= mouseY * sensitivity;
            _rotationY = Mathf.Clamp(_rotationY, -maxYAngle, maxYAngle);

            // Применяем вращение головы только по X (вверх/вниз)
            head.localRotation = Quaternion.Euler(_rotationY, 0.0f, 0.0f);
        }
    }
}