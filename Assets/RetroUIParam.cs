using UnityEngine;
using UnityEngine.UI;

public class RetroUIParam : MonoBehaviour
{
    [SerializeField] float customReductionScale;
    [SerializeField] Image img;

    private static readonly string reductionPropertyName = "Reduction Scale";
    private Material mat;

    void Awake()
    {
        if (img == null)
        {
            img = GetComponent<Image>();
        }

        if (img == null) return;

        mat = img.material;
        mat.SetFloat(reductionPropertyName, customReductionScale);
    }
}
