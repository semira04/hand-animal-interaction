# 🐾 Hand Landmark Animal Interaction

Unity ile geliştirilmiş AR el takibi uygulaması. MediaPipe kullanarak el hareketlerini algılar ve 3D hayvanlarla etkileşim sağlar.

![Unity](https://img.shields.io/badge/Unity-2022.3-black?logo=unity)
![Platform](https://img.shields.io/badge/Platform-Android-green?logo=android)
![C#](https://img.shields.io/badge/C%23-Script-blue?logo=csharp)

## ✨ Özellikler

- 📱 Android kamera desteği ve izin yönetimi
- ✋ MediaPipe ile el landmark algılama
- 🐈 3D hayvanlara dokunma etkileşimi
- 🎵 Her hayvana özel ses efektleri
- 🎨 Dokunma anında renk değişim animasyonları
- 📱 Dokunmatik ekran ve fare desteği

## 🎮 Nasıl Çalışır?

1. **Kamera Açılır**: Android cihazda kamera izni alınır ve açılır
2. **El Algılanır**: MediaPipe ile elinizin 21 landmark noktası tespit edilir
3. **Hayvanlarla Etkileşim**: Parmağınızı hayvanlara dokundurarak:
   - Renkleri değişir
   - Ses efektleri çalar
   - İnteraktif geri bildirim alırsınız

## 🛠️ Kullanılan Teknolojiler

- **Unity** 2022.3.62f3
- **MediaPipe** Hand Tracking
- **C#** Scripting
- **Android** Camera2 API

## 📁 Proje Yapısı
```
Assets/
├── Scripts/
│   ├── android_camera_fixed.cs      # Android kamera başlatma ve izin yönetimi
│   ├── mediapipe_fixed.cs           # MediaPipe el landmark algılama
│   ├── AnimalInteraction.cs         # Hayvanların davranış scripti
│   └── TouchInputHandler.cs         # Dokunmatik giriş kontrolü
├── Scenes/
│   └── Hand Landmark Detection.unity
├── Models/                          # 3D hayvan modelleri
└── Sounds/                          # Ses dosyaları
```

## 🚀 Kurulum ve Çalıştırma

### Gereksinimler
- Unity Hub
- Unity 2022.3 veya üzeri
- Android Build Support modülü

### Adımlar

1. **Projeyi klonlayın:**
```bash
   git clone https://github.com/semira04/hand-animal-interaction.git
```

2. **Unity Hub'da açın:**
   - Unity Hub → Open → Proje klasörünü seçin

3. **Gerekli paketleri içe aktarın:**
   - MediaPipe Unity Plugin
   - TextMeshPro (otomatik)

## 📱 Android'e Build Alma

1. **File → Build Settings**
2. **Android** platformunu seçin → **Switch Platform**
3. **Player Settings** ayarları:
   - **Other Settings:**
     - ✅ Camera Permission
     - Minimum API Level: **24** (Android 7.0)
     - Target API Level: **33+**
   - **Publishing Settings:**
     - Keystore oluşturun (ilk kez build alıyorsanız)

4. **Build and Run** veya sadece **Build**

## 🎯 Kullanım

1. APK'yı Android cihaza yükleyin
2. Uygulamayı açın
3. **Kamera iznini verin** (popup çıkacak)
4. Elinizi kameraya gösterin
5. Ekrandaki hayvanlara parmağınızla dokunun

## 🐛 Bilinen Sorunlar ve Çözümler

| Sorun | Çözüm |
|-------|-------|
| Kamera açılmıyor | Ayarlar → Uygulamalar → İzinler → Kamera iznini verin |
| El algılanmıyor | Aydınlık bir ortamda deneyin, elin kameraya net görünmesini sağlayın |
| Uygulama yavaş çalışıyor | Build Settings → Quality → Low seçin |

## 📊 Performans

- **FPS**: 30-60 (cihaza bağlı)
- **Algılama Gecikmesi**: ~50ms
- **RAM Kullanımı**: ~200MB



## 📄 Lisans

Bu proje MIT lisansı altında lisanslanmıştır. Detaylar için `LICENSE` dosyasına bakın.

## 👤 Geliştirici

**Semira**
- GitHub: [@semira04](https://github.com/semira04)
- Proje: [hand-animal-interaction](https://github.com/semira04/hand-animal-interaction)



---

⭐ **Projeyi beğendiyseniz yıldız vermeyi unutmayın!**

🐛 **Sorun mu buldunuz?** [Issue açın](https://github.com/semira04/hand-animal-interaction/issues)

💡 **Öneriniz mi var?** Pull request gönderin!
