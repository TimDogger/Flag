using UnityEngine;

namespace DigitalDouble.Scripts
{
    public class CameraSettings : MonoBehaviour
    {
        /// <summary>
        /// Плавность
        /// </summary>
        public float smoothness = .2f;

        /// <summary>
        /// Скорость передвижения
        /// </summary>
        public float movementSpeed = 10f;

        /// <summary>
        /// Максимальная скорость передвижения
        /// </summary>
        public float maxSpeed = 100f;

        /// <summary>
        /// Скорость вращения
        /// </summary>
        public float rotateSpeed = .2f;

        /// <summary>
        /// Минимальный угол рысканья
        /// </summary>
        public float minY = -80f;

        /// <summary>
        /// Максимальный угол рысканья
        /// </summary>
        public float maxY = 80f;

        /// <summary>
        /// Чувствительность обзора камеры
        /// </summary>
        public float sensitivity = .5f;

        /// <summary>
        /// Множитель при ускорении камеры
        /// </summary>
        public float cameraShift = 2f;

        /// <summary>
        /// Скорость камеры при использовании колеса мыши
        /// </summary>
        public float speedScrollSensitivity = .0008f;

        /// <summary>
        /// Максимальная скорость при скролле
        /// </summary>
        public float maxCameraScroll = 3f;

        /// <summary>
        /// Минимальная скорость при скролле
        /// </summary>
        public float minCameraScroll = 1f;
    }
}