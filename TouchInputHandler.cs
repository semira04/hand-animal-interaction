using UnityEngine;

/// <summary>
/// Mobil cihazlarda ve dokunmatik ekranlarda hayvanlara dokunmayı sağlar
/// Bu script Main Camera'ya eklenir
/// </summary>
public class TouchInputHandler : MonoBehaviour
{
    [Header("Ayarlar")]
    [Tooltip("Hayvanların bulunduğu layer")]
    public LayerMask animalLayer;
    
    [Tooltip("Raycast mesafesi")]
    public float rayDistance = 100f;
    
    private Camera mainCamera;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void Update()
    {
        HandleTouchInput();
        HandleMouseInput(); // Bilgisayarda test için
    }

    void HandleTouchInput()
    {
        // Mobil dokunuşları kontrol et
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            // Dokunuş başladığında
            if (touch.phase == TouchPhase.Began)
            {
                CheckTouchOnAnimal(touch.position);
            }
        }
    }

    void HandleMouseInput()
    {
        // Fare sol tık (bilgisayarda test için)
        if (Input.GetMouseButtonDown(0))
        {
            CheckTouchOnAnimal(Input.mousePosition);
        }
    }

    void CheckTouchOnAnimal(Vector2 screenPosition)
    {
        // Ekran pozisyonundan 3D dünyaya ray at
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;
        
        // Sadece animal layer'ındaki objelere çarp
        if (Physics.Raycast(ray, out hit, rayDistance, animalLayer))
        {
            // Hayvana dokun
            AnimalInteraction animal = hit.collider.GetComponent<AnimalInteraction>();
            if (animal != null)
            {
                animal.OnTouch();
                Debug.Log("Dokunmatik ile " + hit.collider.gameObject.name + " dokunuldu!");
            }
        }
    }

    // Debug için ray'i göster
    void OnDrawGizmos()
    {
        if (Input.GetMouseButton(0) && mainCamera != null)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(ray.origin, ray.direction * rayDistance);
        }
    }
}
