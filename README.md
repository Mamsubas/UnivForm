# UnivForm - Üniversite Öğrenci Forum Platformu

Üniversite ve lise öğrencileri için soru-cevap ve tartışma platformu.

## Seeded Admin Credentials (İlk Giriş)

Uygulama başlatıldığında otomatik olarak bir admin hesabı oluşturulur:

- **Kullanıcı Adı:** `admin`
- **E-posta:** `admin@univform.com`
- **Şifre:** `Admin123!`

Admin hesabı e-posta doğrulaması olmadan giriş yapabilir.

## Kurulum ve Çalıştırma

### Gereksinimler

- .NET 9.0 SDK
- SQL Server (MSSQL)
- Node.js (npm) - Tailwind CSS için

### Adımlar

1. **Veritabanı Migrasyonları Uygulayın:**

   ```bash
   dotnet ef database update
   ```

2. **Uygulamayı Çalıştırın:**

   ```bash
   dotnet watch
   # veya
   dotnet run
   ```

   Uygulama `http://localhost:5046` (veya gösterilen URL) adresinde çalışacaktır.

3. **Admin Paneline Giriş:**
   - Tarayıcıda `http://localhost:5046/Account/Login` sayfasına gidin
   - Kullanıcı adı veya e-posta: `admin` (veya `admin@univform.com`)
   - Şifre: `Admin123!`
   - Giriş yaptıktan sonra menüde "🛠️ Yönetici" bağlantısını tıklayın

## Özellikler

### Kullanıcı Özellikleri

- ✅ Hesap kaydı ve e-posta doğrulaması
- ✅ Forum konuları oluşturma ve cevap verme
- ✅ Cevapları beğenme (like)
- ✅ Profil düzenleme (ad, soyad, biyografi, profil resmi yükleme)
- ✅ Cevap ve soru paylaşma (link kopyala / sosyal paylaş)
- ✅ Gösterge paneli (dashboard) - son sorular, cevaplar, istatistikler
- ✅ Kişisel profil sayfası

### Admin Özellikleri

- ✅ Kategori yönetimi
- ✅ Kullanıcı listesi ve yönetimi
- ✅ Kullanıcıları admin yapma/kaldırma
- ✅ Kullanıcı aktiflik durumu değiştirme

## API Endpoints

### Forum

- `GET /Forum` - Tüm konuları listele
- `GET /Forum/ThreadDetail/{id}` - Konuya yanıtları göster
- `POST /Forum/CreateThread` - Yeni konu oluştur
- `POST /Forum/AddPost` - Konuya cevap ekle
- `POST /Forum/ToggleLike` - Cevap beğen/beğenmekten çıkar

### Profil

- `GET /UserProfile/View/{id?}` - Profil görüntüle
- `GET /UserProfile/Edit` - Profil düzenleme sayfası
- `POST /UserProfile/Edit` - Profil güncelle (profil resmi yükleme dahil)

### Dashboard

- `GET /Home/Dashboard` - Kullanıcının kişisel gösterge paneli

### Admin

- `GET /Admin` - Kullanıcı yönetimi (sadece Admin rolüne sahip kullanıcılar)
- `POST /Admin/ToggleAdmin` - Kullanıcıya Admin rolü ekle/kaldır
- `POST /Admin/ToggleActive` - Kullanıcı aktiflik durumunu değiştir

## Veritabanı Schema

### Temel Tablolar

- `AspNetUsers` - Kullanıcı hesapları (ad, soyad, biyografi, profil resmi)
- `AspNetRoles` - Roller (Admin, User, Manager, Student)
- `AspNetUserRoles` - Kullanıcı-rol ilişkileri
- `ForumThreads` - Forum konuları (başlık, içerik, kategori)
- `Posts` - Cevaplar ve yorumlar (nested replies desteklenir)
- `PostLikes` - Cevap beğenileri
- `Categories` - Forum kategorileri
- `Students` - Öğrenci bilgileri (lise/üniversite)

## Dosya Yükleme

Profil resimleri `wwwroot/uploads/profiles/` dizinine yüklenir. Sunucu tarafı doğrulama:

- Maksimum dosya boyutu: 2 MB
- İzin verilen türler: `image/*` (JPEG, PNG, GIF, vb.)

## Teknoloji Stack

- **Framework:** ASP.NET Core 9.0 (MVC)
- **Veritabanı:** Entity Framework Core + SQL Server
- **Kimlik Doğrulama:** ASP.NET Core Identity
- **UI Framework:** Tailwind CSS
- **Diller:** C#, HTML, Razor, JavaScript

## Ayarlar

`appsettings.json` ve `appsettings.Development.json` dosyalarında konfigürasyon yapılabilir.

### Bağlantı Dizesi Örneği

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=UnivForm;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

## Deployment

Produktion ortamında:

1. `appsettings.Production.json` oluşturun ve bağlantı dizesini ayarlayın
2. `dotnet publish -c Release` ile yayın paketi oluşturun
3. Veritabanı migrasyonlarını çalıştırın: `dotnet ef database update --configuration Release`
4. Uygulamayı barındırma platformuna yükleyin (Azure, AWS, vb.)

## Geliştirme

Geliştirme sırasında:

- `dotnet watch` ile hot reload etkindir
- Tailwind CSS otomatik derlenmiştir
- Veritabanı değişiklikleri için: `dotnet ef migrations add MigrationName`
- Migrasyonları uygula: `dotnet ef database update`

## Sorun Giderme

### Admin paneline giremiyorum

1. Kullanıcı adı `admin` (küçük harf) olduğundan emin olun
2. Şifre `Admin123!` (büyük A, rakamlar ve ünlem işareti)
3. E-posta doğrulaması Admin hesapları için zorunlu değildir
4. Logs kontrol edin: `SeedData` uygulamayı başlattığında admin credentiallerini loglar

### Profil resmi yüklenmiyor

1. İçerik türü kontrol edin (resim formatı olmalı)
2. Dosya boyutu 2 MB'ı aşmamalı
3. `wwwroot/uploads/profiles/` dizininin yazılabilir olduğundan emin olun

### Veritabanı hataları

1. SQL Server bağlantısını kontrol edin
2. `appsettings.json` bağlantı dizesini doğrulayın
3. Migrasyonları yeniden çalıştırın: `dotnet ef database update`

## İletişim ve Destek

Sorunlar veya öneriler için:

- E-posta: info@univform.com
- GitHub Issues: [UnivForm Repository](https://github.com/Mamsubas/UnivForm)

## Lisans

Bu proje açık kaynaklıdır. Detaylar için LICENSE dosyasına bakın.

---

**Son Güncelleme:** 16 Kasım 2025
