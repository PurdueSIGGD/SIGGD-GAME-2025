using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SlimedOut : Singleton<SlimedOut>
{
    public Image slimeOutImage;

    protected override void Awake()
    {
        base.Awake();
        
        slimeOutImage.enabled = false;
    }
    
    public void TriggerSlimedOut()
    {
        StartCoroutine(Slime(1f));
    }

    IEnumerator Slime(float duration)
    {
        // flash the image in and out using alpha over the duration
        
        float elapsed = 0f;
        slimeOutImage.enabled = true;

        while (elapsed < duration)
        {
            float alpha = Mathf.PingPong(elapsed * 2f, 1f); // oscillate alpha between 0 and 1
            
            elapsed += Time.deltaTime;
            var slimeColor = slimeOutImage.color;
            
            var newColor = new Color(slimeColor.r, slimeColor.g, slimeColor.b, alpha);
            slimeOutImage.color = newColor;
                
                
            yield return null;
        }
        
        slimeOutImage.enabled = false;
    }
}