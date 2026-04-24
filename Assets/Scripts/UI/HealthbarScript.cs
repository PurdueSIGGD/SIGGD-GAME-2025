using UnityEngine;
using UnityEngine.UI;

public class HealthbarScript : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;

    private Image fillImage;

    private void Awake()
    {
        if (healthSlider == null) healthSlider = GetComponent<Slider>();
    }

    private void Start()
    {
        EntityHealthManager.OnHealthChanged += OnHealthChanged;
        if (healthSlider != null && healthSlider.fillRect != null)
        {
            fillImage = healthSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.type = Image.Type.Filled;
                fillImage.fillMethod = Image.FillMethod.Radial360;
                fillImage.fillOrigin = 0;
                fillImage.fillClockwise = true;
                Debug.Log("[Healthbar] Enforced fillImage type=Filled, method=Radial360");
            }
            else
            {
                Debug.LogWarning("[Healthbar] fillRect has no Image component.");
            }
        }
        else
        {
            Debug.LogWarning("[Healthbar] healthSlider or healthSlider.fillRect is null.");
        }
        TryInitializeFromPlayer();
    }

    private void OnDestroy()
    {
        EntityHealthManager.OnHealthChanged -= OnHealthChanged;
    }

    private void TryInitializeFromPlayer()
    {
        if (healthSlider == null) return;
        if (PlayerID.Instance == null) return;
        var ph = PlayerID.Instance.playerHealth;
        if (ph == null) return;
        healthSlider.minValue = 0f;
        healthSlider.maxValue = ph.MaxHealth;
        healthSlider.SetValueWithoutNotify(ph.CurrentHealth);

        if (fillImage != null && ph.MaxHealth > 0)
            fillImage.fillAmount = Mathf.Clamp01(ph.CurrentHealth / ph.MaxHealth);
    }

    private void OnHealthChanged(DamageContext context)
    {
        if (PlayerID.Instance == null) return;
        if (context.victim != PlayerID.Instance.gameObject) return;
        if (healthSlider == null) { Debug.LogError("[Healthbar] slider null"); return; }

        var ph = PlayerID.Instance.playerHealth;
        if (ph == null) { Debug.LogError("[Healthbar] playerHealth null"); return; }

        healthSlider.maxValue = ph.MaxHealth;
        healthSlider.SetValueWithoutNotify(ph.CurrentHealth);

        // direct visual update for circular slider
        if (fillImage != null)
        {
            float percent = ph.MaxHealth > 0 ? ph.CurrentHealth / ph.MaxHealth : 0f;
            fillImage.fillAmount = Mathf.Clamp01(percent);
            Canvas.ForceUpdateCanvases();
        }
    }
}