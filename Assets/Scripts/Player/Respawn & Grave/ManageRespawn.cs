using UnityEngine;

public class ManageRespawn : MonoBehaviour
{
    private GameObject player;
    private Inventory inv;
    private EntityHealthManager health;
    private PlayerHunger hunger;

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
        Time.timeScale = 1f;
        player.transform.position = respawnPoint;
        health.ResetHealth();
        hunger.ResetHunger();
    }

    private void OnPlayerDeath(DamageContext context)
    {
        if (context.victim == PlayerID.Instance.gameObject)
        {
            Time.timeScale = 0f;
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
}
