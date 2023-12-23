using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.XR.Content.Interaction;

public class CockpitUI : MonoBehaviour
{
    [SerializeField] private PlayerShipController playerShipController;
    [SerializeField] private Slider durabilitySlider;
    [SerializeField] private Slider shieldSlider;
    [SerializeField] private Slider speedSlider;
    [SerializeField] private TextMeshProUGUI durabilityText;
    [SerializeField] private TextMeshProUGUI shieldText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private GameObject targetBox;
    [SerializeField] private GameObject targetLock;
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private Camera bottomCamera;
    [SerializeField] private Camera rearCamera;
    [SerializeField] private MeshRenderer bottomCameraDisplay;
    [SerializeField] private MeshRenderer rearCameraDisplay;

    private Rigidbody _shipRigidbody;
    private readonly Dictionary<Rigidbody, Transform> _targetBoxes = new();
    private readonly Dictionary<Rigidbody, Transform> _targetLocks = new();

    private const string MIXER_MUSIC = "MusicVolume";
    private const string MIXER_SFX = "SFXVolume";

    private void Start()
    {
        durabilitySlider.value = 100;
        shieldSlider.value = 100;
        durabilityText.text = $"{durabilitySlider.value}";
        shieldText.text = $"{shieldSlider.value}";
        playerShipController.DurabilityChanged += UpdateDurability;
        playerShipController.ShieldChanged += UpdateShield;
        playerShipController.TargetLocated += UpdateTargetBox;
        playerShipController.TargetLocked += UpdateTargetLock;
        _shipRigidbody = playerShipController.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        var velocity = (int)_shipRigidbody.velocity.magnitude;
        speedSlider.value = velocity;
        speedText.text = $"{velocity}";
        
        foreach (var pair in _targetBoxes)
        {
            var scale = (_shipRigidbody.position - pair.Key.position).magnitude;
            pair.Value.localScale = Vector3.one * (scale / 50);
            pair.Value.LookAt(_shipRigidbody.transform);
        }
        
        foreach (var pair in _targetLocks)
        {
            var scale = (_shipRigidbody.position - pair.Key.position).magnitude;
            pair.Value.localScale = Vector3.one * (scale / 30);
            pair.Value.LookAt(_shipRigidbody.transform);
        }
    }

    private void UpdateDurability(float value)
    {
        durabilitySlider.value = (int)value;
        durabilityText.text = $"{durabilitySlider.value}";
    }

    private void UpdateShield(float value)
    {
        shieldSlider.value = (int)value;
        shieldText.text = $"{shieldSlider.value}";
    }

    private void UpdateTargetBox(Rigidbody target, bool isLocated)
    {
        if (isLocated)
        {
            if (!_targetBoxes.ContainsKey(target))
            {
                _targetBoxes.Add(target, Instantiate(targetBox, target.transform).transform);
            }
        }
        else if (_targetBoxes.TryGetValue(target, out var box))
        {
            Destroy(box.gameObject);
            _targetBoxes.Remove(target);
        }
    }

    private void UpdateTargetLock(Rigidbody target, bool isLocked)
    {
        if (isLocked)
        {
            if (!_targetLocks.ContainsKey(target))
            {
                _targetLocks.Add(target, Instantiate(targetLock, target.transform).transform);
            }
        }
        else if (_targetLocks.TryGetValue(target, out var lockOn))
        {
            Destroy(lockOn.gameObject);
            _targetLocks.Remove(target);
        }
    }

    public void SetMusicVolume(XRKnob knob)
    {
        var value = Mathf.Max(0.0001f, knob.value);
        mixer.SetFloat(MIXER_MUSIC, Mathf.Log10(value) * 20);
    }
    
    public void SetSfxVolume(XRKnob knob)
    {
        var value = Mathf.Max(0.0001f, knob.value);
        mixer.SetFloat(MIXER_SFX, Mathf.Log10(value) * 20);
    }

    public void ToggleBottomCamera()
    {
        bottomCamera.enabled = !bottomCamera.enabled;
        bottomCameraDisplay.enabled = !bottomCameraDisplay.enabled;
    }
    
    public void ToggleRearCamera()
    {
        rearCamera.enabled = !rearCamera.enabled;
        rearCameraDisplay.enabled = !rearCameraDisplay.enabled;
    }
}