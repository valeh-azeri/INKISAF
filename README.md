# Özünü İnkişaf

Dini oxu-izləmə tətbiqi: kitab/Quran PDF-lərinin idarəsi, xətim (Quran tamamlama) dövrələri, həftəlik dini çətələ (tally) və admin hesabatları.

## Arxitektura

Clean Architecture, Code First (EF Core), MSSQL:

```
src/
  OzunuInkisaf.Domain          — Entity-lər, enum-lar (heç bir asılılıq yoxdur)
  OzunuInkisaf.Contracts       — API DTO-ları (MAUI client bunu referans edir, Domain-i yox)
  OzunuInkisaf.Application     — Servis interfeys/implementasiyaları, biznes qaydaları
  OzunuInkisaf.Infrastructure  — EF Core DbContext, konfiqurasiyalar, JWT, parol hashing, fayl saxlama
  OzunuInkisaf.WebApi          — ASP.NET Core Web API (controller-lər, Program.cs)
clients/
  OzunuInkisaf.Maui            — .NET MAUI Blazor Hybrid tətbiqi (Windows / Android / iOS)
```

Qeyd: mürəkkəb kitabxana asılılıqlarından (MediatR, AutoMapper, FluentValidation) qəsdən imtina edilib — sadə servis-interfeys nümunəsi istifadə olunub ki, kod bazası sadə və oxunaqlı qalsın.

## ÖNƏMLİ — bu kod compiler yoxlamasından keçməyib

Bu layihə tam əl ilə yazılıb, sıfırdan Clean Architecture struktur olaraq. Kodu yazan mühitdə (bu sessiyada) `dotnet` SDK quraşdırıla bilmədi (şəbəkə məhdudiyyəti səbəbindən), ona görə **heç bir fayl `dotnet build` ilə yoxlanılmayıb**. Böyük ehtimalla kod düzgündür (bütün layihələr, using-lər, tip adları diqqətlə yoxlanılıb), amma Visual Studio-da açıb ilk dəfə build edəndə kiçik compile xətaları (məsələn unudulmuş using, kiçik tip uyğunsuzluğu) çıxa bilər. Bu normaldır — VS-in "Error List" paneli onları göstərəcək və adətən 1-2 sətirlik düzəlişlərdir.

## Tələb olunan alətlər

- Visual Studio 2022 (17.8+) — ".NET Multi-platform App UI development" və "ASP.NET and web development" workload-ları ilə
- .NET 8 SDK
- SQL Server (LocalDB kifayətdir — Visual Studio ilə birlikdə gəlir)
- Android build üçün: Android SDK (Visual Studio MAUI workload-u ilə birlikdə gəlir)
- iOS/MacCatalyst build üçün: **macOS + Xcode lazımdır** (Windows-dan uzaqdan "Pair to Mac" ilə, ya da birbaşa Mac-də). Bu, .NET MAUI-nin özünün məhdudiyyətidir — heç bir alət bunu dəyişə bilməz. Əgər iOS-a ehtiyacınız yalnız "ekrana əlavə et" səviyyəsindədirsə, Windows/Android build-lərini bitirib API-ni bir serverə qoyduqdan sonra, istifadəçilər iOS Safari-də API-yə bağlı sadə bir veb-səhifə üzərindən də sistemə "Add to Home Screen" edə bilər — amma tam MAUI tətbiqi kimi deyil, ayrıca bir addımdır.

## Backend-i işə salmaq

1. `OzunuInkisaf.sln` faylını Visual Studio-da açın.
2. `src/OzunuInkisaf.WebApi/appsettings.json`-dakı `ConnectionStrings:DefaultConnection` sətrini öz SQL Server-inizə uyğun tənzimləyin (default LocalDB-yə işarələyir, adətən dəyişməyə ehtiyac olmur).
3. Package Manager Console-da (Default Project: **OzunuInkisaf.Infrastructure**):
   ```powershell
   Add-Migration InitialCreate -StartupProject OzunuInkisaf.WebApi
   Update-Database -StartupProject OzunuInkisaf.WebApi
   ```
   (Yaxud terminal-dan, `src/OzunuInkisaf.WebApi` qovluğundan: `dotnet ef migrations add InitialCreate --project ../OzunuInkisaf.Infrastructure --startup-project .` və ardınca `dotnet ef database update --project ../OzunuInkisaf.Infrastructure --startup-project .`)
4. `OzunuInkisaf.WebApi`-ni "Startup Project" seçib F5 ilə işə salın. Swagger UI `https://localhost:7001/swagger` (və ya VS-in göstərdiyi port) ünvanında açılacaq.
5. Tətbiq ilk dəfə açılanda avtomatik olaraq **admin / admin** hesabı yaradılır (bax: `Infrastructure/Persistence/Seed/DbInitializer.cs`). **Real istifadəyə verməzdən əvvəl bu şifrəni admin profilindən dəyişdirin.**

## MAUI client-i işə salmaq

1. `clients/OzunuInkisaf.Maui/Services/ApiSettings.cs` faylında `BaseUrl`-i backend-inizin ünvanına uyğunlaşdırın:
   - Windows-da yerli test: `https://localhost:7001/`
   - Android emulyatorunda test: `https://10.0.2.2:7001/` (artıq default olaraq belədir)
   - Real cihaz və ya production: serverin öz domeni/IP-si (məsələn Azure App Service ünvanı)
2. Solution Explorer-də `OzunuInkisaf.Maui`-ni sağ-klikləyib "Set as Startup Project" edin, hədəf platformanı seçin (Windows Machine / Android Emulator) və F5.
3. Giriş ekranında `admin` / `admin` ilə daxil olub admin panelinə keçə bilərsiniz, ya da admin panelindən yeni adi istifadəçilər yaradıb onlarla test edə bilərsiniz.

> **Qeyd — localhost və HTTPS sertifikatı:** Android emulyatoru və fiziki cihazlar development sertifikatına etibar etmədiyi üçün ilk testlərdə bağlantı xətası ala bilərsiniz. Ən sadə həll development zamanı `launchSettings.json`-da HTTP profilini istifadə etmək, ya da `dotnet dev-certs https --trust` icra etməkdir. Production-da real domain + real sertifikat istifadə edəcəyiniz üçün bu məhdudiyyət aradan qalxır.

## Funksionallıq xülasəsi

- **Kitablar/Quran**: Admin PDF yükləyir (tək və ya bir-bir təkrarlanan sorğularla toplu), istifadəçi açıb oxuyur, mövqe (səhifə) avtomatik yadda saxlanılır və növbəti dəfə oradan davam edir.
- **Xətim**: 30 cüzdən ibarət dövrə. İstifadəçilər boş cüzü götürüb tamamlayır. Bütün 30 cüz bitəndə sistem avtomatik yeni dövrə açır (`KhatimCycle` + `JuzClaim` cədvəlləri). Admin "Xətim idarəetməsi" səhifəsində indiyə qədər neçə dövrün bitdiyini və hazırkı dövrün vəziyyətini görür.
- **Dualar**: Admin istənilən vaxt oxumaq üçün dua mətnləri (ərəbcə + tərcümə + transliterasiya) əlavə edir, istifadəçi kateqoriyaya görə axtarıb oxuyur.
- **Çətələ**: Admin həftəlik bəndlər (gündəlik Quran, oruc günləri, təhəccüd və s.) təyin edib yayımlayır; hər istifadəçi öz vəziyyətini gün-gün işarələyir; toplama tarixinədək davam edir. Ballar `PointsTransaction` jurnalına yazılır (heç vaxt silinmir/üzərinə yazılmır — bu, hesabatların istənilən vaxt üçün dəqiq hesablanmasını təmin edir).
- **Hesabatlar**: Admin həftəlik/aylıq/illik/xüsusi aralıqda, seçdiyi kateqoriyalara (kitab oxuma, xətim, çətələ) görə reytinq görür və qalib elan edə bilər.

## Admin hesabı

```
İstifadəçi adı: admin
Şifrə: admin
```

Bu, layihə sahibinin açıq tələbi ilə seed edilib. **Production-a keçməzdən əvvəl mütləq dəyişdirin** — admin panelindən "Profil → Şifrəni dəyiş" ilə, ya da verilənlər bazasında `Users` cədvəlindəki admin sətrini yeniləyərək.

## Production-a keçid (Azure və ya başqa server)

Bu sessiyada Azure/GitHub-a birbaşa yükləmə mümkün olmadı (bu mühitin şəbəkə/icazə məhdudiyyətləri səbəbindən). Tam kod sizə fayl olaraq təhvil verilib. Tövsiyə olunan yol:

1. **Backend (`OzunuInkisaf.WebApi`)**: Visual Studio-da layihəyə sağ-klikləyib **Publish → Azure → Azure App Service** seçin (yeni App Service yaradın və ya mövcud birini seçin). SQL Server üçün Azure SQL Database yaradıb `appsettings.json`-dakı connection string-i (Azure Portal-da "Connection strings" bölməsindən götürərək) Azure App Service-in "Configuration → Connection strings" hissəsinə əlavə edin (koda yazmaq əvəzinə).
2. **JWT `Secret`**: `appsettings.json`-dakı development açarını **mütləq** dəyişdirin — Azure App Service-in "Configuration → Application settings" bölməsində `Jwt__Secret` adlı ətraf mühit dəyişəni kimi əlavə edin (kod içində saxlamayın).
3. **MAUI client**: `ApiSettings.BaseUrl`-i production App Service ünvanına yönləndirib, Windows üçün MSIX paketləyin (Publish → Windows), Android üçün imzalanmış APK/AAB yaradın (Publish → Android), iOS üçün isə (yuxarıda qeyd olunduğu kimi) Mac + Xcode ilə App Store-a və ya TestFlight-a yükləyin.
4. **GitHub**: Kod öz kompüterinizdə hazır git repository-dədir (`git log` ilə commit tarixçəsini görə bilərsiniz). `git remote -v` ilə mövcud remote-u yoxlayıb, öz GitHub hesabınızdan `git push -u origin main` etməklə repo-ya yükləyə bilərsiniz. İstəsəniz, bundan sonra GitHub Actions ilə Azure-a avtomatik deploy (CI/CD) üçün ayrıca bir `azure-webapps-deploy` workflow faylı da hazırlaya bilərəm — bunun üçün mənə xəbər verin.

## Bilinməyən/görülməli işlər

- EF Core migration-ları hələ generasiya edilməyib (yuxarıdakı `Add-Migration` addımı ilk dəfə sizin tərəfinizdən işə salınmalıdır).
- MAUI client-də PDF, cihazın öz default PDF oxucusu ilə açılır (fayl yükləyib `Launcher.OpenAsync` ilə). Tam in-app PDF render (səhifə-səhifə, tətbiq daxilində) istəsəniz, bu, ayrıca bir təkmilləşdirmə kimi əlavə oluna bilər (məsələn PDF.js-i wwwroot-a inteqrasiya etməklə).
- İstifadəçi profilində ümumi bal göstərilmir (hazırda bu, yalnız admin hesabatlarında mövcuddur) — istəsəniz "mənim balım" üçün ayrıca, admin olmayan bir endpoint əlavə edə bilərəm.
