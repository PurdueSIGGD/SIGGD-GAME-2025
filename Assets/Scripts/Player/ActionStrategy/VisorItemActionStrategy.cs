using UnityEngine;

public class VisorItem : IPlayerActionStrategy
{
    private static readonly string eatSound = "Eating";
    private bool isOn = false;





    protected override void OnEnter()
    {
        base.OnEnter();
        PlayHandAction(); // plays animation for apple, but this is instant rn so it does nothing
        //AudioManager.Instance.PlayOneShotNoAsync(eatSound, PlayerID.Instance.gameObject.transform.position);

        float currentFOV = Camera.main.fieldOfView;

        if (currentFOV > 40f)
        {
            CameraZoomController.Instance.SetFOV(15f);
        }
        else
        {
            CameraZoomController.Instance.SetFOV(60f);
        }
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        
    }

    protected override void OnExit()
    {
        base.OnExit();
        


    }



}