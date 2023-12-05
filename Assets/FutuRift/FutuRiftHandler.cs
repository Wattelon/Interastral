using ChairControl.ChairWork;
using ChairControl.ChairWork.Options;
using UnityEngine;

namespace Futurift
{
    public class FutuRiftHandler : MonoBehaviour
    {
        [SerializeField] [Range(0, 30)] private float maxAccelerationAngle;
        [SerializeField] [Range(0, 30)] private float maxRotationAngle;
        
        private FutuRiftController _futuRiftController;
        private Rigidbody _rigidbody;
        private Transform _transform;
        private Vector3 _previousVelocity = Vector3.zero;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _transform = transform;
        }

        private void FixedUpdate()
        {
            var velocity = _rigidbody.velocity;
            var globalAcceleration = velocity - _previousVelocity;
            var localAcceleration = _transform.InverseTransformDirection(globalAcceleration);
            var clampedAcceleration = Vector3.ClampMagnitude(localAcceleration * 10, maxAccelerationAngle);
            _previousVelocity = velocity;

            var pitch = clampedAcceleration.z;
            var roll = clampedAcceleration.x;

            var globalAngularVelocity = _rigidbody.angularVelocity;
            var localAngularVelocity = _transform.InverseTransformDirection(globalAngularVelocity);
            var clampedAngularVelocity = Vector3.ClampMagnitude(localAngularVelocity * 10, maxRotationAngle);

            pitch -= clampedAngularVelocity.x;
            roll -= clampedAngularVelocity.z;

            _futuRiftController.Pitch = Mathf.Clamp(pitch, -30, 30);
            _futuRiftController.Roll = Mathf.Clamp(roll, -30, 30);
        }
    
        private void OnEnable()
        {
            _futuRiftController = new  FutuRiftController(
                new UdpOptions
                {
                    // ip = "192.168.50.126",  // ip компьютера, на котором запущен контроллер (не локальный)
                    ip= "127.0.0.1",            // локальный
                    port = 6065             // порт, на который настроен контроллер
                });

            _futuRiftController.Start();
        }

        private void OnDisable()
        {
            _futuRiftController.Stop();
        }
    }
}
