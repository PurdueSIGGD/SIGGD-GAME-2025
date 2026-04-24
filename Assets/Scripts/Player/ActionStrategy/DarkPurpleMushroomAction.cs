using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // Or HighDefinition if using HDRP

public class DarkPurpleMushroomItemAction : IPlayerActionStrategy
{
    private static readonly string eatSound = "PlayerConsume";
    protected override void OnEnter()
    {
        base.OnEnter();
        Inventory.Instance.Decrement();
        AudioManager.Instance.PlayOneShotNoAsync(eatSound, PlayerID.Instance.gameObject.transform.position);
        GameObject player = PlayerID.Instance.gameObject;
        player.GetComponent<MonoBehaviour>().StartCoroutine(HealOverTime(player));
        PlayerID.Instance.GetComponent<PlayerHunger>().UpdateHunger(20);
        Debug.Log("player ate dark purple mushroom");
    }

    private IEnumerator HealOverTime(GameObject player)
    {
        GameObject globalVolumeObj = GameObject.Find("Global Volume");
        Transform volumeTransform = globalVolumeObj.GetComponent<Transform>().GetChild(0);
        volumeTransform.gameObject.SetActive(true);
        Volume volume = volumeTransform.GetComponent<Volume>();
        LensDistortion lensDistortion;
        DepthOfField depthOfField;
        ColorAdjustments colorAdjustments;
        volume.profile.TryGet(out lensDistortion);
        volume.profile.TryGet(out depthOfField);
        volume.profile.TryGet(out colorAdjustments);
        int totalHeals = 8;
        float totalDuration = 16f;
        float interval = totalDuration / totalHeals;
        for (int i = 0; i < totalHeals; i++)
        {
            if (colorAdjustments != null)
            {
                Color randomTripColor = Color.HSVToRGB(Random.value, 0.6f, 1f);
                colorAdjustments.colorFilter.Override(randomTripColor);
                colorAdjustments.postExposure.Override(Random.Range(-0.2f, 1.0f));
            }
            if (lensDistortion != null)
            {
                lensDistortion.intensity.Override(Random.Range(-1f, 1f));
                lensDistortion.xMultiplier.Override(Random.Range(0.8f, 1f));
                lensDistortion.yMultiplier.Override(Random.Range(0.8f, 1f));
            }
            if (depthOfField != null)
            {
                depthOfField.gaussianStart.Override(Random.Range(-10f, 3f));
                depthOfField.gaussianStart.Override(Random.Range(5, 10f));
            }
            globalVolumeObj.GetComponent<Transform>().GetChild(1).gameObject.SetActive(false);
            DamageContext healContext = new DamageContext();
            healContext.attacker = healContext.victim = player;
            healContext.amount = 20;
            PlayerID.Instance.GetComponent<EntityHealthManager>().Heal(healContext);
            PlayerID.Instance.GetComponent<PlayerHunger>().UpdateHunger(20);
            yield return new WaitForSeconds(interval);
        }
        if (lensDistortion != null) lensDistortion.intensity.Override(0);
        volumeTransform.gameObject.SetActive(false);
    }
}