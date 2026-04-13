using UnityEngine;

public class ManageRespawn : MonoBehaviour
{
    private GameObject player;
    private Inventory inv;
    private EntityHealthManager health;
    private PlayerHunger hunger;
    private PlayerStamina stamina;

    [SerializeField] private GameObject deathMenu;

    public Vector3 respawnPoint;
    public GameObject graveObj;
    GameObject curGrave = null;

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
        Debug.Log("Respawing player");
        Time.timeScale = 1f;
        player.transform.position = respawnPoint;
        player.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        health.ResetHealth();
        hunger.ResetHunger();
        stamina.ResetStamina();
        PlayerID.Instance.IsAlive = true;
    }

    private void OnPlayerDeath(DamageContext context)
    {
        if (context.victim == PlayerID.Instance.gameObject)
        {
            Time.timeScale = 0f;
            PlayerID.Instance.IsAlive = false;
            CreateGrave();
            deathMenu.GetComponent<DeathMenu>().ShowDeathMenu(true);
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
