using UnityEngine;

public class ManageRespawn : MonoBehaviour
{
    private GameObject player;
    private Inventory inv;
    private EntityHealthManager health;
    private PlayerHunger hunger;

    public Vector3 respawnPoint;
    public GameObject graveObj;
    GameObject curGrave = null;

    public void UpdateSpawnPoint(Transform spawnPoint)
    {
        respawnPoint = spawnPoint.position;
    }

    public void RespawnPlayer()
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

        player.transform.position = respawnPoint;
        health.ResetHealth();
        hunger.ResetHunger();
    }

    private void OnPlayerDeath(DamageContext context)
    {
        if (context.victim == PlayerID.Instance.gameObject)
        {
            RespawnPlayer();
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
