# Steam Achievement Unlocker (Otomatik & Tek Tık) 🏆

Steam kütüphanenizdeki oyunların başarımlarını (achievements) tek tek veya topluca açmanızı sağlayan hafif, hızlı ve harici hiçbir kütüphaneye ihtiyaç duymayan bir araçtır.

> 🤖 **Not:** Bu proje mantık kurgusu, C# kodları ve Steamworks API entegrasyonu aşamalarında yapay zeka desteğiyle geliştirilmiştir. 

---

## 💡 Neden Yaptık? Ne İşe Yarar?

Outlast 2'nin o akıl sağlığı kaybettiren *"Insane modda pil değiştirmeden bitir"* gibi saçma sapan başarımlarıyla saatlerce ömür tüketmek yerine, Steam'in kendi resmi API köprüsünü (`steam_api64.dll`) kullanarak bu işlemi tek tıkla çözmek için yazdık.

- **Kütüphaneyi Otomatik Tarar:** Bilgisayarındaki yerel Steam dosyalarını (`localconfig.vdf`, `libraryfolders.vdf`, `appmanifest`) tarayıp hesabındaki bütün oyunları otomatik listeler.
- **Toplu Açma Modu:** İster sadece bilgisayarında o an yüklü olan oyunları, istersen hesabındaki tüm oyunları sırayla taratıp eksik bütün başarımları tek seferde açtırabilirsin.
- **Tekil Oyun (AppID) Modu:** Sadece tek bir oyunu açmak istiyorsan oyunun Steam AppID numarasını girmen yeterli (Örn: Outlast 2 için `414700`, Outlast Trials için `1304930`, Dead by Daylight için `381210`).
- **Kurulumsuz & Sıfır Bağımlılık:** Harici hiçbir NuGet paketi veya kütüphane gerektirmez. Windows'un kendi C# derleyicisiyle doğrudan çalışır.

---

## 🛡️ Ban Riski Var mı? (Anti-Cheat Durumu)

**Kısacası: Hayır.**
- Bu program oyunların kendi `.exe` dosyasını **kesinlikle çalıştırmaz**.
- Oyun açılmadığı için oyun içi koruma sistemleri (Easy Anti-Cheat, BattlEye, VAC vb.) arka planda devreye girmez veya belleği taramaz.
- Doğrudan açık olan Steam istemcisine `StoreStats()` çağrısı gönderir. Steam Achievement Manager (SAM) mantığıyla birebir aynı şekilde işler.

> ⚠️ **Önemli Kural:** Aracı kullanırken üzerinde işlem yaptığınız oyunun o esnada **açık olmamasına** dikkat edin yeterli.

---

## 🚀 Nasıl Kullanılır?

1. **Steam açık olsun** ve hesabınıza giriş yapılmış durumda bulunsun.
2. `bin/` klasöründeki **`SteamAchievementUnlocker.exe`** dosyasını çalıştırın (Yanındaki `steam_api64.dll` dosyasıyla aynı klasörde olmalı).
3. Karşına çıkan konsol menüsünden seçimini yap:
   - `[1]` Hesabındaki bütün oyunları otomatik tara ve eksik başarımları aç.
   - `[2]` Sadece o an bilgisayarda yüklü olan oyunları tara.
   - `[3]` İstediğin oyunun Steam AppID'sini elle gir.
4. Başarımlar açıldığında sağ altta Steam bildirimleri patlamaya başlar ve profilinde anında senkronize (synced) olur.

---

## 🔨 Kaynak Koddan Nasıl Derlenir?

Visual Studio kurmanıza gerek yok. Windows'ta zaten yüklü gelen dahili derleyiciyle tek tıkla derleyebilirsiniz:

1. Proje ana dizinindeki **`build.bat`** dosyasına çift tıklayın.
2. Veya CMD üzerinden şu komutu çalıştırın:
```cmd
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:exe /platform:x64 /optimize+ /out:bin\SteamAchievementUnlocker.exe src\Program.cs
```

---

## ⚠️ Yasal Uyarı

Bu araç kişisel kullanım ve eğitim amacıyla paylaşılmıştır. Valve Corporation veya Steam ile resmi bir bağı yoktur.
