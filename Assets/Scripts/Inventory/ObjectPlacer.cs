using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectPlacer : MonoBehaviour
{
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

    private GameObject _previewObject = null;
    private Vector3 _currentPlacementPosition = Vector3.zero;
    private bool _inPlacementMode = false;
    private bool _validPreviewState = false;


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
    }

    private void Update()
    {
        if (_inPlacementMode)
        {
            UpdateCurrentPlacementPosition();

            if (CanPlaceObject())
                SetValidPreviewState();
            else
                SetInvalidPreviewState();

            if (Input.GetKeyDown(KeyCode.F))
            {
                Debug.Log("Placing Object, Position: " + _currentPlacementPosition);
                PlaceObject();
            }
                
        }
        else
        {
            if (Input.GetButtonDown("Fire1"))
            {
                ItemInfo itemInfo = Inventory.Instance.GetSelectedItem();
                if (itemInfo != null && itemInfo.itemPlacementPrefab != null && itemInfo.itemPrefab != null)
                {
                    SetPlacementPrefabs(itemInfo.itemPrefab, itemInfo.itemPlacementPrefab);
                    EnterPlacementMode();
                }
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

        Quaternion rotation = Quaternion.Euler(0f, playerCamera.transform.eulerAngles.y, 0f);
        Instantiate(placeableObjectPrefab, _currentPlacementPosition, rotation, transform);

        ExitPlacementMode();
    }
}
