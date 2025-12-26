using UnityEngine;

/// <summary>
/// Bu script hayvanlara eklenir. Hayvan dokunulduğunda ses çıkarır ve renk değiştirir.
/// Hem el ile, hem fare ile, hem de dokunmatik ekranla çalışır.
/// </summary>
public class AnimalInteraction : MonoBehaviour
{
    [Header("Ses Ayarları")]
    [Tooltip("Hayvana dokunulduğunda çalacak ses dosyası")]
    public AudioClip animalSound;
    private AudioSource audioSource;
    
    [Header("Renk Ayarları")]
    [Tooltip("Hayvanın normal haldeki rengi")]
    public Color normalColor = Color.white;
    
    [Tooltip("Dokunulduğunda değişecek renk")]
    public Color touchedColor = Color.yellow;
    
    private Renderer animalRenderer;
    private Color currentColor;
    
    [Header("Etkileşim Ayarları")]
    [Tooltip("Renk değişimi kaç saniye sürecek")]
    public float colorChangeDuration = 0.5f;
    
    [Tooltip("Dokunma efekti için bekleme süresi (spam önleme)")]
    public float touchCooldown = 0.3f;
    
    private float colorChangeTimer = 0f;
    private float cooldownTimer = 0f;
    private bool isTouched = false;
    private bool canTouch = true;

    void Start()
    {
        // AudioSource bileşenini al, yoksa ekle
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // AudioSource ayarları
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D ses
        
        // Renderer'ı al (hayvanın görünümü için)
        animalRenderer = GetComponent<Renderer>();
        if (animalRenderer == null)
        {
            // Çocuk objelerden renderer aramayı dene
            animalRenderer = GetComponentInChildren<Renderer>();
        }
        
        if (animalRenderer != null)
        {
            currentColor = normalColor;
            animalRenderer.material.color = normalColor;
        }
        else
        {
            Debug.LogWarning(gameObject.name + " üzerinde Renderer bulunamadı!");
        }
    }

    void Update()
    {
        // Cooldown sayacı
        if (!canTouch)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= touchCooldown)
            {
                canTouch = true;
                cooldownTimer = 0f;
            }
        }
        
        // Renk değişimi geri dönüşü
        if (isTouched)
        {
            colorChangeTimer += Time.deltaTime;
            if (colorChangeTimer >= colorChangeDuration)
            {
                ReturnToNormalColor();
            }
        }
    }

    /// <summary>
    /// El, fare veya dokunmatik ekranla dokunulduğunda çağrılır
    /// </summary>
    public void OnTouch()
    {
        if (!canTouch) return; // Cooldown aktifse çık
        
        if (!isTouched)
        {
            isTouched = true;
            colorChangeTimer = 0f;
            canTouch = false;
            cooldownTimer = 0f;
            
            // Rengi değiştir
            ChangeColor(touchedColor);
            
            // Ses çal
            PlaySound();
            
            Debug.Log(gameObject.name + " dokunuldu!");
        }
    }

    private void ChangeColor(Color newColor)
    {
        if (animalRenderer != null)
        {
            animalRenderer.material.color = newColor;
            currentColor = newColor;
        }
    }

    private void ReturnToNormalColor()
    {
        ChangeColor(normalColor);
        isTouched = false;
        colorChangeTimer = 0f;
    }

    private void PlaySound()
    {
        if (audioSource != null && animalSound != null)
        {
            audioSource.PlayOneShot(animalSound);
        }
        else if (animalSound == null)
        {
            Debug.LogWarning(gameObject.name + " için ses dosyası atanmamış!");
        }
    }

    // ==================== FARE İLE TIKLAMA ====================
    // Unity otomatik olarak fareyle tıklandığında bu fonksiyonu çağırır
    void OnMouseDown()
    {
        OnTouch();
    }

    // ==================== DEBUG İÇİN ====================
    // Scene view'da hayvanın çevresinde yeşil bir kutu gösterir
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
