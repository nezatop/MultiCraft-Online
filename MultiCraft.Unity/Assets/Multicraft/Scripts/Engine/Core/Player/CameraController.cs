using UnityEngine;

namespace MultiCraft.Scripts.Engine.Core.Player
{
    public class CameraController : MonoBehaviour
    {
        public float sensitivity = 2.0f;
        public float maxYAngle = 80.0f;

        private float _rotationX;

        private void Update()
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            transform.parent.Rotate(Vector3.up * (mouseX * sensitivity));

            _rotationX -= mouseY * sensitivity;
            _rotationX = Mathf.Clamp(_rotationX, -maxYAngle, maxYAngle);
            transform.localRotation = Quaternion.Euler(_rotationX, 0.0f, 0.0f);
        }
    }
}