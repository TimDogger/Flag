using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace DigitalDouble.Scripts
{
    [RequireComponent(typeof(CameraSettings))]
    public class CameraMovement : MonoBehaviour
    {
        public static CameraMovement Singleton { get; private set; }

        public Vector2 PointerPos => _pointerPos;
        
        private CameraSettings _settings;

        private Vector3 _desiredPos = Vector3.zero;
        private Vector3 _direction = Vector3.zero;
        private Vector3 _velocity = Vector3.zero;
        private Vector2 _cameraLook = Vector2.zero;
        private Vector2 _pointerPos = Vector2.zero;

        private float _rotationX;
        private float _rotationY;
        private float _cameraSpeed = 1f;

        private bool _isSpeeding;

        private bool _isMovingToBounds;

        private void Awake()
        {
            Singleton = this;
        }


        private void Start()
        {
            _settings = GetComponent<CameraSettings>();
            _rotationX = transform.rotation.eulerAngles.y - 360f;
            _rotationY = transform.rotation.eulerAngles.x - 360f;
        }

        /// <summary>
        /// Перемещение камеры
        /// </summary>
        /// <param name="context"></param>
        public void OnMove(InputAction.CallbackContext context)
        {
            if (context.performed) _isMovingToBounds = false;
            var dir = context.ReadValue<Vector2>();
            _direction.z = dir.y;
            _direction.x = dir.x;
        }

        /// <summary>
        /// При ускорении камеры
        /// </summary>
        /// <param name="context"></param>
        public void OnCameraSpeed(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                _isSpeeding = true;
                _cameraSpeed = _settings.cameraShift;
            }
            else if (context.canceled)
            {
                _isSpeeding = false;
                _cameraSpeed = 1f;
            }
        }

        /// <summary>
        /// Регулировка скорости камеры
        /// </summary>
        /// <param name="context"></param>
        public void OnScrollCameraSpeed(InputAction.CallbackContext context)
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            if (context.performed)
            {
                var scrollValue = context.ReadValue<Vector2>().y;

                _settings.movementSpeed += _settings.movementSpeed * 0.1f * scrollValue;
                _settings.movementSpeed = Mathf.Clamp(_settings.movementSpeed, 1f, 500f);
            }
        }


        /// <summary>
        /// Перемещение камеры вверх и вниз
        /// </summary>
        /// <param name="context"></param>
        public void OnUpDown(InputAction.CallbackContext context)
        {
            _direction.y = context.ReadValue<float>();
        }

        /// <summary>
        /// Обзор камерой
        /// </summary>
        /// <param name="context"></param>
        public void OnLook(InputAction.CallbackContext context)
        {
            _cameraLook = context.ReadValue<Vector2>();
        }

        /// <summary>
        /// Позиция курсора
        /// </summary>
        /// <param name="context"></param>
        public void OnCursorPos(InputAction.CallbackContext context)
        {
            _pointerPos = context.ReadValue<Vector2>();
            _pointerPos = new Vector2(_pointerPos.x, -_pointerPos.y);
        }
        
        /// <summary>
        /// При начале обзора скрывает или показывает курсор
        /// </summary>
        /// <param name="context"></param>
        public void OnStartLook(InputAction.CallbackContext context)
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            if (context.started)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else if (context.canceled)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }

        /// <summary>
        /// Плавное обновление позиции камеры
        /// </summary>
        /// <param name="newDirection"></param>
        private void UpdateMovement(Vector3 newDirection)
        {
            if (!_isMovingToBounds)
            {
                var offset = newDirection * _settings.movementSpeed;
                offset *= _cameraSpeed;
                _desiredPos = transform.position + offset.z * transform.forward + offset.y * Vector3.up +
                              offset.x * transform.right;

                transform.position = Vector3.SmoothDamp(transform.position, _desiredPos, ref _velocity,
                    _settings.smoothness, _settings.maxSpeed);
            }
            else
            {
                transform.position = Vector3.SmoothDamp(transform.position, _desiredPos, ref _velocity,
                    _settings.smoothness, _settings.maxSpeed);
                if (Vector3.Distance(transform.position, _desiredPos) <= 0.1f) _isMovingToBounds = false;
            }
        }

        private Vector3 GetSelectedPosition(IEnumerable<GameObject> selectedActors, float hardOffsetDistance = 0f)
        {
            var renderers = new List<Renderer>();

            foreach (var actor in selectedActors)
            {
                var actorFilters = actor.GetComponentsInChildren<Renderer>();
                renderers.AddRange(actorFilters);
            }

            var bounds = GetGroupBounds(renderers);

            var boundsSize =
                Mathf.Sqrt((float) (Math.Pow(bounds.size.x, 2) + Math.Pow(bounds.size.y, 2) +
                                    Math.Pow(bounds.size.z, 2))); // диагональ bounds'а
            boundsSize = boundsSize > 500f ? 500f : boundsSize;

            var dir = -transform.forward;

            var offset = dir * boundsSize;
            var hardOffset = dir * hardOffsetDistance;

            return bounds.center + offset + hardOffset;
        }

        private Vector3 GetSelectedPosition(List<Vector3> selected, float hardOffsetDistance = 0f)
        {
            var bounds = GetGroupBounds(selected);

            var boundsSize =
                Mathf.Sqrt((float) (Math.Pow(bounds.size.x, 2) + Math.Pow(bounds.size.y, 2) +
                                    Math.Pow(bounds.size.z, 2))); // диагональ bounds'а
            boundsSize = boundsSize > 500f ? 500f : boundsSize;

            var dir = -transform.forward;

            var offset = dir * boundsSize;
            var hardOffset = dir * hardOffsetDistance;

            return bounds.center + offset + hardOffset;
        }

        private void LimitMove(Vector3 contactNormal)
        {
            var cameraDir = (_desiredPos - transform.position).normalized;

            var angle = Vector3.Dot(cameraDir, contactNormal);

            if (angle < 0) _direction = Vector3.ProjectOnPlane(_direction, contactNormal);
            Debug.Log($"Direction: {cameraDir}   angle: {angle}");
        }

        /// <summary>
        /// Перемещает камеру к выбранным объектам
        /// </summary>
        /// <param name="selectedActors">выбранные акторы</param>
        public void MoveToSelected(List<GameObject> selectedCoordinates, bool smoothMove = true, float hardOffset = 0f)
        {
            var pos = GetSelectedPosition(selectedCoordinates, hardOffset);
            if (smoothMove)
            {
                _desiredPos = pos;
                _isMovingToBounds = true;
            }
            else transform.position = pos;
        }

        public void MoveToPosition(Vector3 position, bool smoothMove = true)
        {
            if (smoothMove)
            {
                _desiredPos = position;
                _isMovingToBounds = true;
            }
            else transform.position = position;
        }

        /// <summary>
        /// Перемещает камеру к выбранным координатам
        /// </summary>
        /// <param name="selectedActors">выбранные акторы</param>
        public void MoveToSelected(List<Vector3> selectedActors, bool smoothMove = true, float hardOffset = 0f)
        {
            var pos = GetSelectedPosition(selectedActors, hardOffset);
            if (smoothMove)
            {
                _desiredPos = pos;
                _isMovingToBounds = true;
            }
            else transform.position = pos;
        }


        /// <summary>
        /// Возвращает общий bounds нескольких мешей
        /// </summary>
        /// <param name="meshFilters"></param>
        /// <returns></returns>
        private static Bounds GetGroupBounds(IReadOnlyCollection<Renderer> renderers)
        {
            if (!renderers.Any()) return new Bounds();

            var bounds = new Bounds(renderers.First().bounds.center, renderers.First().bounds.size);
            foreach (var renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }

            return bounds;
        }

        private static Bounds GetGroupBounds(List<Vector3> points)
        {
            if (!points.Any()) return new Bounds();

            var bounds = new Bounds(points.First(), Vector3.one);
            foreach (var point in points)
            {
                bounds.Encapsulate(point);
            }

            return bounds;
        }

        private Bounds GetGroupBounds(IReadOnlyCollection<MeshFilter> meshFilters)
        {
            if (!meshFilters.Any()) return new Bounds();

            var bounds = new Bounds();
            var centers = new List<Vector3>();
            foreach (var filter in meshFilters)
            {
                bounds.Encapsulate(filter.mesh.bounds);
                centers.Add(filter.transform.TransformPoint(filter.mesh.bounds.center));
            }

            bounds.center = new Vector3(centers.Average(x => x.x), centers.Average(x => x.y),
                centers.Average(x => x.z));
            return bounds;
        }


        /// <summary>
        /// Плавный поворот камеры
        /// </summary>
        /// <param name="rotate"></param>
        private void Look(Vector2 rotate)
        {
            _rotationX += rotate.x * _settings.sensitivity;
            _rotationY += rotate.y * _settings.sensitivity;
            _rotationY = Mathf.Clamp(_rotationY, _settings.minY, _settings.maxY);
            var targetRotation = Quaternion.Euler(_rotationY, _rotationX, 0);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _settings.rotateSpeed);
            transform.rotation =
                Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
        }

        private void Update()
        {
            UpdateMovement(_direction);

            // Обзор камерой
            if (!Cursor.visible)
            {
                Look(_cameraLook);
            }
        }

        private void AttachTo(Transform target)
        {
            transform.SetParent(target, true);
        }

        private void Detach()
        {
            transform.SetParent(null);
        }
    }
}