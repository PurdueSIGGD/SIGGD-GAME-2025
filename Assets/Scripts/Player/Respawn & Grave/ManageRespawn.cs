using System.Collections;
using SIGGD.Save;
using SIGGD.Save.Modules;
using UnityEngine;

public class ManageRespawn : MonoBehaviour
{
    private GameObject player;
    private Inventory inv;
    private EntityHealthManager health;
    private PlayerHunger hunger;
    private PlayerStamina stamina;
    private PlayerRadiation radiation;

    [SerializeField] private GameObject deathMenu;

    public Vector3 respawnPoint;
    public GameObject graveObj;
    GameObject curGrave = null;

    private Collider playerCollider;

    public void UpdateSpawnPoint(Transform spawnPoint)
    {
        respawnPoint = spawnPoint.position;
    }

    public void CreateGrave()
    {
        if (curGrave)
        {
            Destroy(curGrave);
        }
        if (!inv.IsInventoryEmpty())
        {
            curGrave = Instantiate(graveObj, transform.position, transform.rotation);
            curGrave.GetComponent<GraveInteract>().FillGrave(inv);
        }
    }

    public void RespawnPlayer()
    {
        StartCoroutine(RespawnRoutine());
        /*
        Debug.Log("Respawing player");
        player.transform.position = respawnPoint;
        player.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        PlayerID.Instance.IsAlive = true;
        Time.timeScale = 1f;
        */
    }

    IEnumerator RespawnRoutine()
    {
        Debug.Log("Respawing player");
        player.transform.position = respawnPoint;

        yield return new WaitForEndOfFrame();

        var rb = player.GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;

        PlayerID.Instance.IsAlive = true;
        playerCollider.enabled = true;
        Time.timeScale = 1f;
    }

    private void OnPlayerDeath(DamageContext context)
    {
        if (context.victim == PlayerID.Instance.gameObject)
        {
            Time.timeScale = 0f;
            PlayerID.Instance.IsAlive = false;
            playerCollider.enabled = false;
            CreateGrave();
            deathMenu.GetComponent<DeathMenu>().ShowDeathMenu(true);
            health.ResetHealth();
            hunger.ResetHunger();
            stamina.ResetStamina();
            radiation.CurrentRadiation = 0f;
        }
    }

    void OnEnable()
    {
        EntityHealthManager.OnDeath += OnPlayerDeath;
    }

    void OnDisable()
    {
        EntityHealthManager.OnDeath -= OnPlayerDeath;
    }

    void Start()
    {
        player = PlayerID.Instance.gameObject;
        inv = PlayerID.Instance.Inventory;
        health = PlayerID.Instance.playerHealth;
        hunger = PlayerID.Instance.playerHunger;
        stamina = PlayerID.Instance.playerStamina;
        radiation = PlayerID.Instance.playerRadiation;
        playerCollider = PlayerID.Instance.GetComponent<Collider>();

        // Re-spawn any grave that was persisted for this scene.
        var save = SaveManager.Instance;
        if (save != null)
        {
            save.WhenGameplayReady(() => save.Apply<GraveModule>());
        }
    }

    void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            RespawnPlayer();
        }
#endif
    }

    public GameObject GetCurGrave() {
        return curGrave;
    }

    public void CreateGrave(Vector3 position, Quaternion rotation, string[] names, int[] count) {
        Debug.Log("Instantiating grave from save");
        inv = PlayerID.Instance.Inventory;
        curGrave = Instantiate(graveObj, position, rotation);
        curGrave.GetComponent<GraveInteract>().FillGrave(inv, names, count);
    }
}
