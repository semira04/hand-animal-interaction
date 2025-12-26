using UnityEngine;
using UnityEngine.Android;
using System.Collections;

/// <summary>
/// Android için kamera izni ve başlatma - DÜZELTİLMİŞ
/// Main Camera'ya ekle
/// </summary>
public class AndroidCameraSetup : MonoBehaviour
{
    [Header("KAMERA AYARLARI")]
    [Tooltip("Arka kamera mı ön kamera mı")]
    public bool useFrontCamera = true;
    
    [Tooltip("Otomatik başlat")]
    public bool autoStart = true;
    
    [Header("DEBUG")]
    public bool showDebugLog = true;
    
    private WebCamTexture webCamTexture;
    private bool cameraStarted = false;
    private bool permissionGranted = false;
    private bool permissionRequested = false;
    
    void Start()
    {
        Debug.Log("═══════════════════════════════════════");
        Debug.Log("📱 ANDROID KAMERA BAŞLATILIYOR...");
        Debug.Log($"Platform: {Application.platform}");
        Debug.Log("═══════════════════════════════════════");
        
        if (autoStart)
        {
            StartCoroutine(CheckPermissionAndStart());
        }
    }
    
    /// <summary>
    /// İzin kontrolü ve başlatma
    /// </summary>
    IEnumerator CheckPermissionAndStart()
    {
        #if UNITY_ANDROID
        // İzin kontrolü
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            if (showDebugLog)
            {
                Debug.Log("📷 Kamera izni YOK - isteniyor...");
            }
            
            RequestCameraPermission();
            permissionRequested = true;
            
            // İznin verilmesini bekle (max 10 saniye)
            float timeout = 10f;
            float elapsed = 0f;
            
            while (!permissionGranted && elapsed < timeout)
            {
                yield return new WaitForSeconds(0.5f);
                elapsed += 0.5f;
                
                // İzin verildi mi kontrol et
                if (Permission.HasUserAuthorizedPermission(Permission.Camera))
                {
                    if (showDebugLog)
                    {
                        Debug.Log("✅ Kamera izni ALINDI!");
                    }
                    permissionGranted = true;
                    break;
                }
            }
            
            if (!permissionGranted)
            {
                Debug.LogError("❌ Kamera izni alınamadı! Lütfen ayarlardan izin verin.");
                yield break;
            }
        }
        else
        {
            if (showDebugLog)
            {
                Debug.Log("✅ Kamera izni zaten var!");
            }
            permissionGranted = true;
        }
        #else
        // PC'de direkt başlat
        Debug.Log("🖥️ PC modunda - izin gerekmez");
        permissionGranted = true;
        #endif
        
        // İzin alındıysa kamerayı başlat
        if (permissionGranted)
        {
            yield return new WaitForSeconds(0.5f);
            StartCamera();
        }
    }
    
    /// <summary>
    /// Android kamera izni iste
    /// </summary>
    public void RequestCameraPermission()
    {
        #if UNITY_ANDROID
        if (showDebugLog)
        {
            Debug.Log("📷 Permission.RequestUserPermission çağrılıyor...");
        }
        
        Permission.RequestUserPermission(Permission.Camera);
        #endif
    }
    
    /// <summary>
    /// Kamerayı başlat
    /// </summary>
    public void StartCamera()
    {
        if (cameraStarted)
        {
            if (showDebugLog)
            {
                Debug.Log("⚠️ Kamera zaten başlatılmış");
            }
            return;
        }
        
        if (WebCamTexture.devices.Length == 0)
        {
            Debug.LogError("❌ Hiç kamera bulunamadı!");
            Debug.LogError("Cihazınızda kamera var mı? İzin verildi mi?");
            return;
        }
        
        if (showDebugLog)
        {
            Debug.Log($"📷 Toplam {WebCamTexture.devices.Length} kamera bulundu:");
            for (int i = 0; i < WebCamTexture.devices.Length; i++)
            {
                var dev = WebCamTexture.devices[i];
                Debug.Log($"  [{i}] {dev.name} (Ön: {dev.isFrontFacing})");
            }
        }
        
        // Uygun kamerayı seç
        WebCamDevice selectedDevice = WebCamTexture.devices[0];
        
        foreach (var device in WebCamTexture.devices)
        {
            if (useFrontCamera && device.isFrontFacing)
            {
                selectedDevice = device;
                break;
            }
            else if (!useFrontCamera && !device.isFrontFacing)
            {
                selectedDevice = device;
                break;
            }
        }
        
        if (showDebugLog)
        {
            Debug.Log("═══════════════════════════════════════");
            Debug.Log($"✅ SEÇİLEN KAMERA: {selectedDevice.name}");
            Debug.Log($"   Ön Kamera: {selectedDevice.isFrontFacing}");
            Debug.Log("═══════════════════════════════════════");
        }
        
        // WebCamTexture oluştur
        try
        {
            webCamTexture = new WebCamTexture(selectedDevice.name, 1920, 1080, 30);
            webCamTexture.Play();
            
            cameraStarted = true;
            
            if (showDebugLog)
            {
                Debug.Log("✅✅✅ KAMERA BAŞLATILDI! ✅✅✅");
                Debug.Log($"Çözünürlük: {webCamTexture.width}x{webCamTexture.height}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Kamera başlatma hatası: {e.Message}");
        }
    }
    
    /// <summary>
    /// Kamerayı durdur
    /// </summary>
    public void StopCamera()
    {
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            webCamTexture.Stop();
            cameraStarted = false;
            
            if (showDebugLog)
            {
                Debug.Log("⏹ Kamera durduruldu");
            }
        }
    }
    
    void OnDestroy()
    {
        StopCamera();
    }
    
    void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            StopCamera();
        }
        else if (permissionGranted && !cameraStarted)
        {
            StartCoroutine(RestartCameraDelayed());
        }
    }
    
    IEnumerator RestartCameraDelayed()
    {
        yield return new WaitForSeconds(0.5f);
        StartCamera();
    }
    
    // GUI'de durum göster
    void OnGUI()
    {
        if (!showDebugLog) return;
        
        GUIStyle style = new GUIStyle();
        style.fontSize = 24;
        style.fontStyle = FontStyle.Bold;
        
        float y = 10;
        
        // İzin durumu
        style.normal.textColor = permissionGranted ? Color.green : Color.red;
        string permStatus = permissionGranted ? "✓ İzin VAR" : "✗ İzin YOK";
        GUI.Label(new Rect(10, y, 400, 30), "KAMERA İZNİ: " + permStatus, style);
        y += 35;
        
        // Kamera durumu
        style.normal.textColor = cameraStarted ? Color.green : Color.yellow;
        string camStatus = cameraStarted ? "✓ AÇIK" : "○ KAPALI";
        GUI.Label(new Rect(10, y, 400, 30), "KAMERA: " + camStatus, style);
        y += 35;
        
        // Kamera sayısı
        style.fontSize = 18;
        style.normal.textColor = Color.white;
        GUI.Label(new Rect(10, y, 400, 25), $"Kamera Sayısı: {WebCamTexture.devices.Length}", style);
        y += 30;
        
        // WebCam texture durumu
        if (webCamTexture != null)
        {
            style.normal.textColor = webCamTexture.isPlaying ? Color.green : Color.red;
            GUI.Label(new Rect(10, y, 400, 25), 
                $"Texture: {webCamTexture.width}x{webCamTexture.height} @ {webCamTexture.requestedFPS}fps", style);
        }
        
        // Manuel başlatma butonu
        if (!permissionGranted || !cameraStarted)
        {
            if (GUI.Button(new Rect(10, Screen.height - 120, 250, 50), "İZİN İSTE / BAŞLAT"))
            {
                StartCoroutine(CheckPermissionAndStart());
            }
        }
        
        // İzin yoksa uyarı
        if (!permissionGranted && permissionRequested)
        {
            style.fontSize = 20;
            style.normal.textColor = Color.yellow;
            GUI.Label(new Rect(10, Screen.height / 2 - 50, Screen.width - 20, 100),
                "⚠️ LÜTFEN KAMERA İZNİ VERİN!\n\nAyarlar > Uygulamalar > Bu Uygulama > İzinler", style);
        }
    }
    
    // WebCamTexture'ı dışarıdan al
    public WebCamTexture GetWebCamTexture()
    {
        return webCamTexture;
    }
    
    // Durum kontrolleri
    public bool IsCameraStarted()
    {
        return cameraStarted && webCamTexture != null && webCamTexture.isPlaying;
    }
    
    public bool HasPermission()
    {
        return permissionGranted;
    }
}