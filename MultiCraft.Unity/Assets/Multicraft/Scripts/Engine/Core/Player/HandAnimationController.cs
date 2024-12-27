using System.Collections;
using UnityEngine;

namespace MultiCraft.Scripts.Engine.Core.Player
{
    public class HandAnimationController : MonoBehaviour
    {
        public Transform handTransform; // Объект руки
        public float animationDuration = 0.2f; // Длительность одного цикла анимации
        public Vector3 animationOffset = new Vector3(0.1f, -0.05f, 0); // Смещение руки в процессе разрушения
        public Vector3 placeAnimationOffset = new Vector3(0, -0.2f, 0.2f); 

        private Vector3 _initialPosition;
        private bool _isAnimating;
        
        private void Start()
        {
            // Сохраняем исходное положение руки
            _initialPosition = handTransform.localPosition;
        }

        
        public void PlayPlaceAnimation()
        {
            StopAllCoroutines();
            StartCoroutine(AnimateHand(placeAnimationOffset));
        }
        private IEnumerator AnimateHand(Vector3 targetOffset)
        {
            // Начальное и конечное положение
            Vector3 startPosition = _initialPosition;
            Vector3 targetPosition = _initialPosition + targetOffset;

            float elapsed = 0f;

            // Анимация "туда"
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;
                handTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
                yield return null;
            }

            // Анимация "обратно"
            elapsed = 0f;
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;
                handTransform.localPosition = Vector3.Lerp(targetPosition, startPosition, t);
                yield return null;
            }
        }
        
        public void StartBreakingAnimation()
        {
            if (!_isAnimating)
            {
                _isAnimating = true;
                StartCoroutine(LoopBreakingAnimation());
            }
        }

        public void StopBreakingAnimation()
        {
            if (_isAnimating)
            {
                _isAnimating = false;
                StopAllCoroutines();
                handTransform.localPosition = _initialPosition; // Возвращаем руку в начальное положение
            }
        }

        private IEnumerator LoopBreakingAnimation()
        {
            while (_isAnimating)
            {
                // Анимация движения "туда"
                float elapsed = 0f;
                while (elapsed < animationDuration / 2)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / (animationDuration / 2);
                    handTransform.localPosition = Vector3.Lerp(_initialPosition, _initialPosition + animationOffset, t);
                    yield return null;
                }

                // Анимация движения "обратно"
                elapsed = 0f;
                while (elapsed < animationDuration / 2)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / (animationDuration / 2);
                    handTransform.localPosition = Vector3.Lerp(_initialPosition + animationOffset, _initialPosition, t);
                    yield return null;
                }
            }
        }
    }
}