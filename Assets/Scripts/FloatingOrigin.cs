using UnityEngine;
using UnityEngine.SceneManagement;

public class FloatingOrigin : MonoBehaviour
{
    [SerializeField] private float distanceThreshold = 1000f;
    [SerializeField] private bool updateParticles = true;
    [SerializeField] private bool updateTrailRenderers = true;
    [SerializeField] private bool updateLineRenderers = true;

    private ParticleSystem.Particle[] _particles;
    private Transform _transform;
    private float _sqrDistance;

    private void Awake()
    {
        _transform = transform;
        _sqrDistance = Mathf.Pow(distanceThreshold, 2);
    }

    private void LateUpdate()
    {
        var referencePosition = _transform.position;

        if (referencePosition.sqrMagnitude > _sqrDistance)
        {
            MoveRootTransforms(referencePosition);

            if (updateParticles)
                MoveParticles(referencePosition);

            if (updateTrailRenderers)
                MoveTrailRenderers(referencePosition);

            if (updateLineRenderers)
                MoveLineRenderers(referencePosition);
        }
    }

    private static void MoveRootTransforms(Vector3 offset)
    {
            foreach (var rootObjects in SceneManager.GetActiveScene().GetRootGameObjects())
                rootObjects.transform.position -= offset;
    }

    private static void MoveTrailRenderers(Vector3 offset)
    {
        var trails = FindObjectsOfType<TrailRenderer>();
        foreach (var trail in trails)
        {
            var positions = new Vector3[trail.positionCount];

            var positionCount = trail.GetPositions(positions);
            for (var i = 0; i < positionCount; ++i)
                positions[i] -= offset;

            trail.SetPositions(positions);
        }
    }

    private static void MoveLineRenderers(Vector3 offset)
    {
        var lines = FindObjectsOfType<LineRenderer>();
        foreach (var line in lines)
        {
            var positions = new Vector3[line.positionCount];

            var positionCount = line.GetPositions(positions);
            for (var i = 0; i < positionCount; ++i)
                positions[i] -= offset;

            line.SetPositions(positions);
        }
    }

    private void MoveParticles(Vector3 offset)
    {
        var particles = FindObjectsOfType<ParticleSystem>();
        foreach (var system in particles)
        {
            if (system.main.simulationSpace != ParticleSystemSimulationSpace.World)
                continue;

            var particlesNeeded = system.main.maxParticles;

            if (particlesNeeded <= 0)
                continue;
            
            if (_particles == null || _particles.Length < particlesNeeded)
            {
                _particles = new ParticleSystem.Particle[particlesNeeded];
            }
            
            var num = system.GetParticles(_particles);

            for (var i = 0; i < num; i++)
            {
                _particles[i].position -= offset;
            }

            system.SetParticles(_particles, num);
        }
    }
}