using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CockpitUI : MonoBehaviour
{
    [SerializeField] private ShipController shipController;
    [SerializeField] private Slider durabilitySlider;
    [SerializeField] private Slider shieldSlider;
    [SerializeField] private Slider speedSlider;
    [SerializeField] private TextMeshProUGUI durabilityText;
    [SerializeField] private TextMeshProUGUI shieldText;
    [SerializeField] private TextMeshProUGUI speedText;

    private Rigidbody _shipRigidbody;

    private void Start()
    {
        durabilitySlider.value = 100;
        shieldSlider.value = 100;
        durabilityText.text = $"{durabilitySlider.value}";
        shieldText.text = $"{shieldSlider.value}";
        shipController.DurabilityChanged += UpdateDurability;
        shipController.ShieldChanged += UpdateShield;
        _shipRigidbody = shipController.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        var velocity = (int)_shipRigidbody.velocity.magnitude;
        speedSlider.value = velocity;
        speedText.text = $"{velocity}";
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
}
