using UnityEngine;

// attach this script to an empty GameObject in the scene

public class ObjectPlacer : MonoBehaviour
{
    public static ObjectPlacer Instance { get; private set; }

    [Header("Placement Parameters")]
    private GameObject placeableObjectPrefab;
    private GameObject previewObjectPrefab;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask placementSurfaceLayerMask;

    [Header("Preview Material")]
    [SerializeField] private Material previewMaterial;
    [SerializeField] private Color validColor;
    [SerializeField] private Color invalidColor;

    [Header("Raycast Parameters")]
    [SerializeField] private float objectDistanceFromPlayer;
    [SerializeField] private float raycastStartVerticalOffset;
    [SerializeField] private float raycastDistance;

    [Header("Adjustment Variables for Placement Location")]
    [SerializeField] private float minDistance = 1.5f;
    [SerializeField] private float maxDistance = 5f;
    [SerializeField] private float distanceChangeSpeed = 1f;

    private GameObject _previewObject = null;
    private Vector3 _currentPlacementPosition = Vector3.zero;
    private bool _inPlacementMode = false;
    private bool _validPreviewState = false;
    [HideInInspector] public bool startPlaceMode = false;

    private ItemInfo itemInfo;

    private void Awake()
    {
        // singleton stuff
        if (Instance != null && Instance != this)
            Destroy(this.gameObject);
        else
            Instance = this;
    }

    public void SetPlacementPrefabs(GameObject prefab, GameObject previewObject)
    {
        placeableObjectPrefab = prefab;
        previewObjectPrefab = previewObject;
    }

    public void EnterPlacementMode()
    {
        // check if already in placement mode
        if (_inPlacementMode)
            return;

        // TODO: Disable other left click keybinds temporarily

        // instantiate preview object
        Quaternion rotation = Quaternion.Euler(0f, playerCamera.transform.eulerAngles.y, 0f);
        _previewObject = Instantiate(previewObjectPrefab, _currentPlacementPosition, rotation, transform);
        _inPlacementMode = true;
    }

    public void ExitPlacementMode()
    {
        // TODO: Reenable other left click keybinds

        Destroy(_previewObject);
        _previewObject = null;
        _inPlacementMode = false;
        startPlaceMode = false;
    }

    public void StartPlacement(ItemInfo item)
    {
        itemInfo = item; // chosen item from inventory button
        SetPlacementPrefabs(item.itemPrefab, item.itemPlacementPrefab);
        EnterPlacementMode();
    }

    private void Update()
    {
        if (_inPlacementMode)
        {

            // change distance from player with mouse scroll
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f)
            {
                objectDistanceFromPlayer += scroll * distanceChangeSpeed;
                objectDistanceFromPlayer = Mathf.Clamp(objectDistanceFromPlayer, minDistance, maxDistance);
            }

            // update preview object position
            UpdateCurrentPlacementPosition();

            // update preview object material based on validity
            if (CanPlaceObject())
                SetValidPreviewState();
            else
                SetInvalidPreviewState();

            // press f to place object
            if (Input.GetKeyDown(KeyCode.F))
            {
                Debug.Log("Attempting to place object, Position: " + _currentPlacementPosition);
                PlaceObject();
            }
            // press escape to exit placement mode
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                ExitPlacementMode();
                startPlaceMode = false;
            }
        }
    }

    private void UpdateCurrentPlacementPosition()
    {
        // Calculate the start position for the raycast
        Vector3 cameraForward = new Vector3(playerCamera.transform.forward.x, 0f, playerCamera.transform.forward.z);
        cameraForward.Normalize();

        // Start position is in front of the player at a set distance, with a vertical offset
        Vector3 startPos = playerCamera.transform.position + (cameraForward * objectDistanceFromPlayer);
        startPos.y += raycastStartVerticalOffset;

        if (Physics.Raycast(startPos, Vector3.down, out RaycastHit hitInfo, raycastDistance, placementSurfaceLayerMask))
        {
            _currentPlacementPosition = hitInfo.point;
        }

        // Update preview object position and rotation
        Quaternion rotation = Quaternion.Euler(0f, playerCamera.transform.eulerAngles.y, 0f);
        _previewObject.transform.position = _currentPlacementPosition;
        _previewObject.transform.rotation = rotation;
    }


    private void SetValidPreviewState()
    {
        previewMaterial.color = validColor;
        _validPreviewState = true;
    }

    private void SetInvalidPreviewState()
    {
        previewMaterial.color = invalidColor;
        _validPreviewState = false;
    }

    private bool CanPlaceObject()
    {
        if (_previewObject == null)
            return false;

        return _previewObject.GetComponent<PreviewObjectValidChecker>().IsValid;
    }

    private void PlaceObject()
    {
        if (!_inPlacementMode || !_validPreviewState)
            return;

        // instantiate the placeable object at the current placement position
        Quaternion rotation = Quaternion.Euler(0f, playerCamera.transform.eulerAngles.y, 0f);
        Instantiate(placeableObjectPrefab, _currentPlacementPosition, rotation, transform);

        ExitPlacementMode();

        // remove one trap from inventory
        Inventory.Instance.RemoveItem(itemInfo, 1);
    }
}
