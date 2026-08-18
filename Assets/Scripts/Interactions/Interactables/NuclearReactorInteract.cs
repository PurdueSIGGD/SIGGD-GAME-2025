using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NuclearReactorInteract : MonoBehaviour, IInteractable<IInteractor>
{

    [SerializeField] private Image fadeOut;
    [SerializeField] private float fadeOutTime = 1.5f;
    [SerializeField] private float waitTime = 1.5f;
    private bool fadingout = false;
    private float timer = 0.0f;
    
    public static readonly string Explosion = "FinalBossExplosion";

    public void OnHoverEnter(InteractableUI ui) {
        if (ConsoleInteract.consolesBroken >= 3)
            ui.ActivateUI(this);
    }
    public void OnHoverExit(InteractableUI ui) {
        if (ConsoleInteract.consolesBroken >= 3)
            ui.DeactivateUI();
    }

    private void Update()
    {
        if (fadingout)
        {
            timer += Time.deltaTime;
            fadeOut.color = Color.Lerp(new Color(0, 0, 0, 0), new Color(0, 0, 0, 1), timer / fadeOutTime);
            Debug.Log(fadeOut.color.a);
            if (timer >= fadeOutTime)
            {
                StartCoroutine(FadeOut());
                fadingout = false;
            }
        }
    }

    IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(waitTime);
        SceneFader.Instance.FadeToScene("Credtis", PlayerID.Instance.rb.transform);
    }

    public void OnInteract(IInteractor interactor)
    {
        if (ConsoleInteract.consolesBroken >= 3)
        {
            Debug.Log("Broke this console");
            fadingout = true;
            AudioManager.Instance.PlayOneShotNoAsync(Explosion, transform.position);
        }
        
    }
}