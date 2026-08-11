using UnityEngine;

public class AppleItem : IPlayerActionStrategy
{
    private static readonly string eatSound = "Eating";
    private bool isOn = false;

    



    protected override void OnEnter()
    {
        base.OnEnter();
        PlayHandAction(); // plays animation for apple, but this is instant rn so it does nothing
        Inventory.Instance.Decrement();
        AudioManager.Instance.PlayOneShotNoAsync(eatSound, PlayerID.Instance.gameObject.transform.position);
        DamageContext healContext = new DamageContext();
        healContext.attacker = healContext.victim = PlayerID.Instance.gameObject;
        healContext.amount = -40;
        PlayerID.Instance.GetComponent<EntityHealthManager>().Heal(healContext);
        PlayerID.Instance.GetComponent<PlayerHunger>().UpdateHunger(20);
        Debug.Log("player ate an apple");
       
        


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

   



}
