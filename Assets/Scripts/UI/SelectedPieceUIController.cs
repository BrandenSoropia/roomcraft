using UnityEngine;
using UnityEngine.UI;

public class SelectedPieceUIController : MonoBehaviour
{

    [SerializeField] GameObject furniturePieceGO;
    [SerializeField] Sprite placeholderPieceSprite;

    [Header("UI")]
    [SerializeField] GameObject holdThrowControlGO;
    [SerializeField] GameObject selectedPieceBorderGO;

    [Header("3D Rendering")]
    [SerializeField] Camera renderCamera;
    [SerializeField] int renderTextureWidth = 256;
    [SerializeField] int renderTextureHeight = 256;
    [SerializeField] Vector3 renderPosition = new Vector3(0, -100, 0); // Hidden position
    [SerializeField] float cameraDistance = 3f;
    [SerializeField] Vector3 cameraRotation = new Vector3(20, -30, 0);
    [SerializeField] int renderLayer = 0; // Default layer (can be changed in inspector)

    Image myFurniturePieceImage;
    RawImage myFurniturePieceRawImage;
    RenderTexture renderTexture;
    GameObject currentRenderedPiece;

    [Header("Internal State")]
    public GameObject renderedSelectedPiece;

    void Start()
    {
        // Try to get Image component first (for placeholder)
        myFurniturePieceImage = furniturePieceGO.GetComponent<Image>();
        // Try to get RawImage component (for 3D rendering)
        myFurniturePieceRawImage = furniturePieceGO.GetComponent<RawImage>();
        
        // Unity doesn't allow both Image and RawImage on same GameObject
        // We'll use Image for sprites and temporarily replace it with RawImage for 3D rendering
        // Store Image properties if it exists
        Color savedColor = Color.white;
        bool savedRaycastTarget = true;
        bool savedMaskable = true;
        
        if (myFurniturePieceImage != null)
        {
            savedColor = myFurniturePieceImage.color;
            savedRaycastTarget = myFurniturePieceImage.raycastTarget;
            savedMaskable = myFurniturePieceImage.maskable;
        }

        // Setup render camera if not assigned
        if (renderCamera == null)
        {
            SetupRenderCamera();
        }

        holdThrowControlGO.SetActive(false);
        selectedPieceBorderGO.SetActive(false);

        // Remove the image on start so we can remove any presets used while developing.
        ClearSelectedPieceImage();
    }

    void SetupRenderCamera()
    {
        // Create a child GameObject for the render camera
        GameObject cameraGO = new GameObject("PieceRenderCamera");
        cameraGO.transform.SetParent(transform);
        cameraGO.transform.localPosition = Vector3.zero;
        cameraGO.transform.localRotation = Quaternion.identity;
        
        renderCamera = cameraGO.AddComponent<Camera>();
        renderCamera.clearFlags = CameraClearFlags.SolidColor;
        renderCamera.backgroundColor = new Color(0, 0, 0, 0); // Transparent background
        renderCamera.cullingMask = 1 << renderLayer; // Only render objects on specified layer
        renderCamera.orthographic = false;
        renderCamera.fieldOfView = 30f;
        renderCamera.depth = -100; // Render before main camera
        renderCamera.enabled = false; // Only enable when needed
    }

    public void SetSelectedPieceImage(Sprite newImpageSprite)
    {
        // Switch to Image mode (disable RawImage, enable Image)
        SwitchToImageMode();
        if (myFurniturePieceImage != null)
        {
            myFurniturePieceImage.sprite = newImpageSprite;
        }
        selectedPieceBorderGO.SetActive(true);
    }

    public void SetSelectedPlaceholderPiece()
    {
        // Switch to Image mode (disable RawImage, enable Image)
        SwitchToImageMode();
        if (myFurniturePieceImage != null)
        {
            myFurniturePieceImage.sprite = placeholderPieceSprite;
        }
        holdThrowControlGO.SetActive(true);
        selectedPieceBorderGO.SetActive(true);
    }

    void SwitchToImageMode()
    {
        // Remove RawImage if it exists (can't have both)
        if (myFurniturePieceRawImage != null)
        {
            DestroyImmediate(myFurniturePieceRawImage);
            myFurniturePieceRawImage = null;
        }
        
        // Ensure Image component exists
        if (myFurniturePieceImage == null)
        {
            myFurniturePieceImage = furniturePieceGO.GetComponent<Image>();
            if (myFurniturePieceImage == null)
            {
                myFurniturePieceImage = furniturePieceGO.AddComponent<Image>();
            }
        }
        myFurniturePieceImage.enabled = true;
    }

    void SwitchToRawImageMode()
    {
        // Remove Image if it exists (can't have both)
        if (myFurniturePieceImage != null)
        {
            // Store properties before removing
            Color savedColor = myFurniturePieceImage.color;
            bool savedRaycastTarget = myFurniturePieceImage.raycastTarget;
            bool savedMaskable = myFurniturePieceImage.maskable;
            
            DestroyImmediate(myFurniturePieceImage);
            myFurniturePieceImage = null;
            
            // Add RawImage
            myFurniturePieceRawImage = furniturePieceGO.GetComponent<RawImage>();
            if (myFurniturePieceRawImage == null)
            {
                myFurniturePieceRawImage = furniturePieceGO.AddComponent<RawImage>();
            }
            
            // Restore properties
            myFurniturePieceRawImage.color = savedColor;
            myFurniturePieceRawImage.raycastTarget = savedRaycastTarget;
            myFurniturePieceRawImage.maskable = savedMaskable;
        }
        else
        {
            // Image doesn't exist, just ensure RawImage exists
            if (myFurniturePieceRawImage == null)
            {
                myFurniturePieceRawImage = furniturePieceGO.GetComponent<RawImage>();
                if (myFurniturePieceRawImage == null)
                {
                    myFurniturePieceRawImage = furniturePieceGO.AddComponent<RawImage>();
                }
            }
        }
        if (myFurniturePieceRawImage != null)
        {
            myFurniturePieceRawImage.enabled = true;
        }
    }

    public void SetSelectedPieceFromGameObject(GameObject piece)
    {
        if (piece == null)
        {
            SetSelectedPlaceholderPiece();
            return;
        }

        // Clean up previous rendered piece
        CleanupRenderedPiece();

        // Create a copy to render (so we don't affect the original)
        currentRenderedPiece = Instantiate(piece, renderPosition, Quaternion.identity);
        currentRenderedPiece.SetActive(true);
        
        // Set layer so camera can see it
        SetLayerRecursively(currentRenderedPiece, renderLayer);
        
        // Make sure all renderers are enabled
        Renderer[] renderers = currentRenderedPiece.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = true;
        }

        // Disable physics components
        Rigidbody[] rigidbodies = currentRenderedPiece.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = true;
        }

        // Disable colliders
        Collider[] colliders = currentRenderedPiece.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        // Setup render texture and camera
        SetupRenderTexture();
        PositionCameraForPiece(currentRenderedPiece);

        // Switch to RawImage mode for 3D rendering
        SwitchToRawImageMode();

        // Enable rendering
        if (myFurniturePieceRawImage != null)
        {
            myFurniturePieceRawImage.texture = renderTexture;
        }

        if (renderCamera != null)
        {
            renderCamera.enabled = true;
        }

        holdThrowControlGO.SetActive(true);
        selectedPieceBorderGO.SetActive(true);
    }

    void SetupRenderTexture()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }

        renderTexture = new RenderTexture(renderTextureWidth, renderTextureHeight, 16);
        renderTexture.Create();

        if (renderCamera != null)
        {
            renderCamera.targetTexture = renderTexture;
        }
    }

    void PositionCameraForPiece(GameObject piece)
    {
        if (renderCamera == null || piece == null) return;

        // Calculate bounds
        Bounds bounds = GetBounds(piece);
        Vector3 center = bounds.center;

        // Position camera
        renderCamera.transform.position = center + Quaternion.Euler(cameraRotation) * Vector3.back * cameraDistance;
        renderCamera.transform.LookAt(center);

        // Adjust camera distance to fit the object
        float objectSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        float requiredDistance = objectSize / (2f * Mathf.Tan(renderCamera.fieldOfView * 0.5f * Mathf.Deg2Rad));
        renderCamera.transform.position = center + (renderCamera.transform.position - center).normalized * requiredDistance * 1.2f; // Add some padding
    }

    Bounds GetBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(obj.transform.position, Vector3.one);

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }
        return bounds;
    }

    void CleanupRenderedPiece()
    {
        if (currentRenderedPiece != null)
        {
            Destroy(currentRenderedPiece);
            currentRenderedPiece = null;
        }

        if (renderCamera != null)
        {
            renderCamera.enabled = false;
        }
    }

    public void ClearSelectedPieceImage()
    {
        CleanupRenderedPiece();

        // Switch back to Image mode for placeholder
        SwitchToImageMode();
        
        if (myFurniturePieceImage != null)
        {
            myFurniturePieceImage.sprite = null;
        }

        holdThrowControlGO.SetActive(false);
        selectedPieceBorderGO.SetActive(false);
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }


    void OnDestroy()
    {
        CleanupRenderedPiece();
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }
}
