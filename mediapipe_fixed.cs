using UnityEngine;

/// <summary>
/// MediaPipe El Takibi - FİNAL VERSİYON
/// - Kırmızı nokta hem Game hem Scene'de
/// - Derinliğe göre boyut değişimi
/// - Hassas hayvan algılama
/// </summary>
public class DirectMediaPipeHand : MonoBehaviour
{
    [Header("MEDİAPİPE")]
    public GameObject mediaPipeAnnotationObject;
    
    [Header("PARMAK SEÇİMİ")]
    public FingerType fingerToUse = FingerType.IndexFinger;
    
    public enum FingerType
    {
        IndexFinger = 8,
        Thumb = 4,
        MiddleFinger = 12,
        RingFinger = 16,
        Pinky = 20
    }
    
    [Header("DERİNLİK KONTROL")]
    public DepthMode mode = DepthMode.HandSize;
    
    public enum DepthMode
    {
        Manual,
        HandSize
    }
    
    [Range(-10f, 10f)]
    public float baseZ = -3.91f;
    
    [Range(0.5f, 5f)]
    public float zRange = 2.5f;
    
    [Range(0.5f, 5f)]
    public float manualSpeed = 2f;
    
    [Range(50f, 500f)]
    public float refHandSize = 150f;
    
    [Header("YUMUŞATİCİLAR - HASSASLИК")]
    [Range(0.01f, 0.5f)]
    public float positionSmoothing = 0.05f; // Daha hassas!
    
    [Range(0.01f, 0.5f)]
    public float zSmoothing = 0.1f; // Daha hassas!
    
    [Header("HAYVAN ETKİLEŞİMİ")]
    [Range(0.3f, 3f)]
    public float touchRadius = 1.2f; // Daha geniş alan
    
    public LayerMask animalLayer;
    
    [Header("GÖRSEL - DERİNLİK BAĞIMLI BOYUT")]
    [Range(10f, 60f)]
    public float minDotSize = 15f; // Uzakta küçük
    
    [Range(30f, 100f)]
    public float maxDotSize = 45f; // Yakında büyük
    
    public bool showRedDot = true;
    public bool debugLogs = false;
    
    // Private
    private Camera cam;
    private Transform[] landmarks = new Transform[21];
    private bool hasHand = false;
    private float currentZ;
    private float smoothZ;
    private Vector3 worldPos;
    private Vector3 smoothWorldPos;
    
    // MediaPipe koordinatlar
    private Vector3 rawFingerLocal;
    private float normX;
    private float normY;
    private float dotX;
    private float dotY;
    
    // Önceki değerler
    private float prevNormX;
    private float prevNormY;
    
    // Debug
    private float handSize;
    private Vector3 minBounds;
    private Vector3 maxBounds;
    
    // Dinamik boyut
    private float currentDotSize;
    
    // GUI için texture
    private Texture2D lineTexture;
    
    void Start()
    {
        cam = Camera.main;
        
        if (cam == null)
        {
            Debug.LogError("❌ Camera bulunamadı!");
            enabled = false;
            return;
        }
        
        if (mediaPipeAnnotationObject == null)
        {
            mediaPipeAnnotationObject = GameObject.Find("Multi HandLandmarkList Annotation");
        }
        
        currentZ = baseZ;
        smoothZ = baseZ;
        smoothWorldPos = new Vector3(2.15f, 0.5f, -3.5f); // Hayvanlara yakın
        transform.position = smoothWorldPos;
        currentDotSize = minDotSize;
        
        minBounds = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        maxBounds = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        
        lineTexture = new Texture2D(1, 1);
        lineTexture.SetPixel(0, 0, Color.white);
        lineTexture.Apply();
        
        Debug.Log("═══════════════════════════════════════");
        Debug.Log("✅ HASSAS ALGILAMA AKTIF!");
        Debug.Log($"Başlangıç: {smoothWorldPos}");
        Debug.Log($"Touch Radius: {touchRadius}m (Hassas)");
        Debug.Log($"Position Smoothing: {positionSmoothing} (Hassas)");
        Debug.Log("═══════════════════════════════════════");
    }
    
    void Update()
    {
        ProcessMediaPipe();
        
        if (hasHand)
        {
            CheckAnimals();
        }
    }
    
    void ProcessMediaPipe()
    {
        hasHand = false;
        
        if (mediaPipeAnnotationObject == null || !mediaPipeAnnotationObject.activeInHierarchy)
        {
            return;
        }
        
        Transform parent = FindHandParent();
        
        if (parent == null || parent.childCount < 21)
        {
            return;
        }
        
        // Tüm landmark'ları al
        for (int i = 0; i < 21 && i < parent.childCount; i++)
        {
            landmarks[i] = parent.GetChild(i);
        }
        
        Transform finger = landmarks[(int)fingerToUse];
        
        if (finger == null)
        {
            return;
        }
        
        // Bounds hesapla
        CalculateHandBounds();
        
        // Z hesapla
        if (mode == DepthMode.HandSize)
        {
            CalculateZFromHandSize();
        }
        else
        {
            if (Input.GetKey(KeyCode.W))
            {
                currentZ += Time.deltaTime * manualSpeed;
            }
            if (Input.GetKey(KeyCode.S))
            {
                currentZ -= Time.deltaTime * manualSpeed;
            }
            
            currentZ = Mathf.Clamp(currentZ, baseZ - zRange, baseZ + zRange);
        }
        
        // Z yumuşatma - daha hassas
        smoothZ = Mathf.Lerp(smoothZ, currentZ, zSmoothing);
        
        // RAW koordinatları al
        rawFingerLocal = finger.localPosition;
        
        // Normalize et
        NormalizeFingerPosition();
        
        // Yumuşat - daha hassas
        normX = Mathf.Lerp(prevNormX, normX, 1f - positionSmoothing);
        normY = Mathf.Lerp(prevNormY, normY, 1f - positionSmoothing);
        
        prevNormX = normX;
        prevNormY = normY;
        
        // GUI koordinatları
        dotX = normX * Screen.width;
        dotY = (1f - normY) * Screen.height;
        
        // 3D dünya pozisyonu hesapla
        CalculateWorldPosition();
        
        // Derinliğe göre boyut hesapla
        CalculateDotSize();
        
        hasHand = true;
    }
    
    void NormalizeFingerPosition()
    {
        if (maxBounds.x > minBounds.x && maxBounds.y > minBounds.y)
        {
            normX = (rawFingerLocal.x - minBounds.x) / (maxBounds.x - minBounds.x);
            normY = (rawFingerLocal.y - minBounds.y) / (maxBounds.y - minBounds.y);
        }
        else
        {
            normX = rawFingerLocal.x;
            normY = rawFingerLocal.y;
        }
        
        normX = Mathf.Clamp01(normX);
        normY = Mathf.Clamp01(normY);
    }
    
    void CalculateWorldPosition()
    {
        // Viewport to World - İYİLEŞTİRİLMİŞ
        // Kameranın Z pozisyonunu dikkate alarak hesapla
        float distanceFromCamera = Mathf.Abs(smoothZ - cam.transform.position.z);
        Vector3 viewportPos = new Vector3(normX, normY, distanceFromCamera);
        worldPos = cam.ViewportToWorldPoint(viewportPos);
        
        // Z pozisyonunu doğrudan smoothZ kullanarak ayarla
        worldPos.z = smoothZ;
        
        // Daha hassas yumuşatma
        smoothWorldPos = Vector3.Lerp(smoothWorldPos, worldPos, 1f - positionSmoothing);
        transform.position = smoothWorldPos;
        
        if (debugLogs && Time.frameCount % 30 == 0)
        {
            Debug.Log($"[POS] smoothZ:{smoothZ:F2} worldPos.z:{worldPos.z:F2} transform.z:{transform.position.z:F2}");
        }
    }
    
    void CalculateDotSize()
    {
        // El büyüklüğüne göre boyut hesapla
        // Büyük el (yakın) = büyük nokta, Küçük el (uzak) = küçük nokta
        
        if (handSize > 0 && refHandSize > 0)
        {
            // HandSize ratio'sunu kullan
            float sizeRatio = handSize / refHandSize;
            sizeRatio = Mathf.Clamp(sizeRatio, 0.5f, 2.0f);
            
            // Büyük el = büyük nokta
            currentDotSize = Mathf.Lerp(minDotSize, maxDotSize, sizeRatio);
            
            if (debugLogs && Time.frameCount % 60 == 0)
            {
                Debug.Log($"[SIZE] handSize:{handSize:F0}px ratio:{sizeRatio:F2} → dotSize:{currentDotSize:F0}px");
            }
        }
        else
        {
            // Fallback: Z derinliğine göre
            float normalizedZ = Mathf.InverseLerp(baseZ + zRange, baseZ - zRange, smoothZ);
            currentDotSize = Mathf.Lerp(minDotSize, maxDotSize, normalizedZ);
        }
        
        // Yumuşat
        currentDotSize = Mathf.Lerp(currentDotSize, currentDotSize, 0.2f);
    }
    
    Transform FindHandParent()
    {
        for (int i = 0; i < mediaPipeAnnotationObject.transform.childCount; i++)
        {
            Transform c = mediaPipeAnnotationObject.transform.GetChild(i);
            if (c.gameObject.activeInHierarchy && c.childCount >= 21)
            {
                return c;
            }
        }
        
        foreach (Transform c in mediaPipeAnnotationObject.transform)
        {
            foreach (Transform gc in c)
            {
                if (gc.gameObject.activeInHierarchy && gc.childCount >= 21)
                {
                    return gc;
                }
            }
        }
        
        return null;
    }
    
    void CalculateHandBounds()
    {
        minBounds = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        maxBounds = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        
        foreach (var lm in landmarks)
        {
            if (lm == null) continue;
            
            Vector3 pos = lm.localPosition;
            
            minBounds.x = Mathf.Min(minBounds.x, pos.x);
            minBounds.y = Mathf.Min(minBounds.y, pos.y);
            minBounds.z = Mathf.Min(minBounds.z, pos.z);
            
            maxBounds.x = Mathf.Max(maxBounds.x, pos.x);
            maxBounds.y = Mathf.Max(maxBounds.y, pos.y);
            maxBounds.z = Mathf.Max(maxBounds.z, pos.z);
        }
    }
    
    void CalculateZFromHandSize()
    {
        if (landmarks[0] == null || landmarks[9] == null) return;
        
        Vector3 wrist = landmarks[0].localPosition;
        Vector3 middle = landmarks[9].localPosition;
        
        float wx = ((wrist.x - minBounds.x) / (maxBounds.x - minBounds.x)) * Screen.width;
        float wy = ((wrist.y - minBounds.y) / (maxBounds.y - minBounds.y)) * Screen.height;
        float mx = ((middle.x - minBounds.x) / (maxBounds.x - minBounds.x)) * Screen.width;
        float my = ((middle.y - minBounds.y) / (maxBounds.y - minBounds.y)) * Screen.height;
        
        handSize = Vector2.Distance(new Vector2(wx, wy), new Vector2(mx, my));
        
        float ratio = Mathf.Clamp(handSize / refHandSize, 0.5f, 2f);
        currentZ = baseZ - (zRange * (ratio - 1f));
        
        currentZ = Mathf.Clamp(currentZ, baseZ - zRange, baseZ + zRange);
    }
    
    void CheckAnimals()
    {
        AnimalInteraction[] animals = FindObjectsOfType<AnimalInteraction>();
        
        foreach (var animal in animals)
        {
            if (animal == null) continue;
            
            float dist = Vector3.Distance(transform.position, animal.transform.position);
            
            // Z farkını da kontrol et
            float zDiff = Mathf.Abs(transform.position.z - animal.transform.position.z);
            
            if (dist < touchRadius)
            {
                animal.OnTouch();
                
                Debug.Log($"🎯🎯🎯 {animal.name} TEMAS! Mesafe: {dist:F3}m, Z Farkı: {zDiff:F3}m 🎯🎯🎯");
            }
            else if (dist < touchRadius * 1.5f && debugLogs)
            {
                Debug.Log($"→ {animal.name} YAKIN! Mesafe: {dist:F3}m, Z Farkı: {zDiff:F3}m");
            }
        }
    }
    
    void OnDrawGizmos()
    {
        if (!hasHand || !Application.isPlaying) return;
        
        // KIRMIZI NOKTA - SCENE EKRANINDA - DİNAMİK BOYUT
        Gizmos.color = Color.red;
        
        // Pixel'den metre'ye - daha küçük göster
        float sceneRadius = (currentDotSize * 0.005f); // 0.01'den 0.005'e düşürdük
        Gizmos.DrawSphere(transform.position, sceneRadius);
        
        // Dış çerçeve
        Gizmos.color = new Color(1, 1, 1, 0.8f);
        Gizmos.DrawWireSphere(transform.position, sceneRadius * 1.3f);
        
        // Touch radius göstergesi
        Gizmos.color = new Color(1, 0, 0, 0.15f);
        Gizmos.DrawWireSphere(transform.position, touchRadius);
        
        // Hayvanlarla bağlantı
        AnimalInteraction[] animals = FindObjectsOfType<AnimalInteraction>();
        foreach (var a in animals)
        {
            if (a == null) continue;
            
            float d = Vector3.Distance(transform.position, a.transform.position);
            
            // Renk: Yeşil = temas, Sarı = yakın, Kırmızı = uzak
            if (d < touchRadius)
            {
                Gizmos.color = Color.green;
            }
            else if (d < touchRadius * 2f)
            {
                Gizmos.color = Color.yellow;
            }
            else
            {
                Gizmos.color = new Color(1, 0, 0, 0.3f);
            }
            
            Gizmos.DrawLine(transform.position, a.transform.position);
            
            #if UNITY_EDITOR
            Vector3 mid = (transform.position + a.transform.position) / 2f;
            UnityEditor.Handles.Label(mid, $"{d:F3}m\nSize:{currentDotSize:F0}px");
            #endif
        }
    }
    
    void OnGUI()
    {
        if (!hasHand || !showRedDot) return;
        
        // KIRMIZI NOKTA - GAME EKRANINDA - DİNAMİK BOYUT
        
        // Ana kırmızı nokta
        GUI.color = Color.red;
        GUI.DrawTexture(new Rect(dotX - currentDotSize/2, dotY - currentDotSize/2, 
            currentDotSize, currentDotSize), Texture2D.whiteTexture);
        
        // Beyaz çerçeve - daha ince
        GUI.color = Color.white;
        float b = Mathf.Max(1.5f, currentDotSize * 0.05f); // Daha ince çerçeve
        GUI.DrawTexture(new Rect(dotX - currentDotSize/2 - b, dotY - currentDotSize/2 - b, 
            currentDotSize + b*2, b), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(dotX - currentDotSize/2 - b, dotY + currentDotSize/2, 
            currentDotSize + b*2, b), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(dotX - currentDotSize/2 - b, dotY - currentDotSize/2, 
            b, currentDotSize), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(dotX + currentDotSize/2, dotY - currentDotSize/2, 
            b, currentDotSize), Texture2D.whiteTexture);
        
        // Merkez çarpı - boyuta göre
        GUI.color = Color.cyan;
        float crossSize = currentDotSize * 0.2f; // Daha küçük çarpı
        float crossThick = Mathf.Max(1.5f, currentDotSize * 0.04f); // Daha ince
        GUI.DrawTexture(new Rect(dotX - crossSize, dotY - crossThick/2, 
            crossSize*2, crossThick), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(dotX - crossThick/2, dotY - crossSize, 
            crossThick, crossSize*2), Texture2D.whiteTexture);
        
        GUI.color = Color.white;
    }
    
    void OnDestroy()
    {
        if (lineTexture != null)
        {
            Destroy(lineTexture);
        }
    }
}