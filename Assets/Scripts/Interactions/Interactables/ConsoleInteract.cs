using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class ConsoleInteract : MonoBehaviour, IInteractable<IInteractor>
{
    public static int consolesBroken = 0;
    
    [SerializeField] private VisualEffect consoleEffect;
    [SerializeField] private GameObject consoleModel;
    [SerializeField] private float time = 0.5f;
    
    public static readonly string ConsoleBroken = "ConsoleDamaged";

    // In the final boss scene the consoles only work once the reactor has called the Apex in
    private static bool Usable => NuclearReactorInteract.Instance == null || NuclearReactorInteract.Instance.ConsolesUsable;

    private void Start()
    {
        consoleEffect.pause = true;
    }
    
    public void OnHoverEnter(InteractableUI ui) {
        if (Usable)
            ui.ActivateUI(this);
    }
    public void OnHoverExit(InteractableUI ui) {
        ui.DeactivateUI();
    }

    public void OnInteract(IInteractor interactor)
    {
        if (!Usable) return;

        Debug.Log("Broke this console");
        AudioManager.Instance.PlayOneShotNoAsync(ConsoleBroken, transform.position);
        consolesBroken++;
        if (NuclearReactorInteract.Instance != null)
            NuclearReactorInteract.Instance.OnConsoleBroken(consolesBroken);

        StartCoroutine(DestroyAndPlayPart());
        //Destroy(this.gameObject);
    }

    private IEnumerator DestroyAndPlayPart()
    {
        consoleModel.SetActive(false);
        consoleEffect.pause = false;
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
