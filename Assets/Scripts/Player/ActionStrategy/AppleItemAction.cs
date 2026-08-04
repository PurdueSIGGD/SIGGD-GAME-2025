using UnityEngine;

public class AppleItem : IPlayerActionStrategy
{
    private static readonly string eatSound = "Eating";
    private bool isOn = false;

    private void OnEnable()
    {
        Inventory.OnHotbarSelectionChanged += OnHotbarSelectionChanged;
    }

    private void OnDisable()
    {
        Inventory.OnHotbarSelectionChanged -= OnHotbarSelectionChanged;
    }




    protected override void OnEnter()
    {
        base.OnEnter();
        PlayHandAction(); // plays animation for apple, but this is instant rn so it does nothing
        //Inventory.Instance.Decrement();
        AudioManager.Instance.PlayOneShotNoAsync(eatSound, PlayerID.Instance.gameObject.transform.position);
        DamageContext healContext = new DamageContext();
        healContext.attacker = healContext.victim = PlayerID.Instance.gameObject;
        healContext.amount = -20;
        PlayerID.Instance.GetComponent<EntityHealthManager>().Heal(healContext);
        PlayerID.Instance.GetComponent<PlayerHunger>().UpdateHunger(20);
        Debug.Log("player ate an apple");
        Debug.Log("jasen made a change");
        //Camera.main.fieldOfView = 30;


        float currentFOV = Camera.main.fieldOfView;

        if (currentFOV > 40f)
        {
            CameraZoomController.Instance.SetFOV(30f);
        }
        else
        {
            CameraZoomController.Instance.SetFOV(60f);
        }
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        //Camera.main.fieldOfView = 30;
    }

    protected override void OnExit()
    {
        base.OnExit();
        //Camera.main.fieldOfView = 60;


    }

    private void OnHotbarSelectionChanged(int selectedIndex)
    {
        // If the player switches to a different item while eating, exit the action state
        Camera.main.fieldOfView = 60;
    }



}
