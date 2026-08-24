# 🚀 Kullanıcı ve Yetkilendirme Yönetim Paneli

## 📌 Proje Özeti
Bu proje, kurumsal bir kimliğe (Glassmorphism UI) uygun olarak geliştirilmiş; güvenli giriş, kayıt ve rol bazlı yetkilendirme işlemlerini barındıran kapsamlı bir yönetim panelidir. Proje boyunca GitHub üzerinden ekip çalışması yapılmış, görev paylaşımları uygulanmış ve tüm süreç dokümante edilmiştir. 

## 🛠 Kullanılan Teknolojiler
Projeyi geliştirirken aşağıdaki teknolojiler kullanılmıştır:
* **Backend:** ASP.NET Core MVC
* **ORM:** Entity Framework Core
* **Veritabanı:** SQL Server LocalDB
* **Versiyon Kontrol & İşbirliği:** Git & GitHub
* **Frontend:** HTML5, Bootstrap 5, Custom CSS (Glassmorphism)

## ⚙️ Proje Kapsamı ve Özellikler
Aşağıdaki modüller geliştirilmiştir:
1. **Kaydolma:** Yeni kullanıcıların sisteme güvenli bir şekilde kayıt olabilmesi.
2. **Giriş / Çıkış (Login - Logout):** Cookie tabanlı (Claims) güvenli oturum yönetimi.
3. **Kullanıcı Yönetimi (CRUD):** Admin paneli üzerinden kullanıcıların listelenmesi, rollerinin güncellenmesi ve silinmesi.
4. **Rol Yönetimi:** Sistem üç temel rolü desteklemektedir: **Admin, Yönetici ve Kullanıcı**.

## 👥 Ekip Çalışması ve GitHub Süreci
* Proje iki kişi tarafından ortak geliştirilmiştir.
* Proje tek bir GitHub repository üzerinden yürütülmüştür.
* Geliştirmeler sırasında GitHub üzerinden branch mantığı kullanılmış, doğrudan main branch'ine commit yapılmamıştır.
* Her geliştirme için anlamlı commit'ler atılmış, kodlar ekip üyeleri tarafından incelenerek (Pull Request) merge edilmiştir.

## 🚀 Kurulum Adımları
1. Projeyi bilgisayarınıza klonlayın: `git clone <repo-url>`
2. Visual Studio üzerinden `KullaniciYonetimi.sln` dosyasını açın.
3. Package Manager Console (PMC) üzerinden veritabanını oluşturmak için `Update-Database` komutunu çalıştırın.
4. Projeyi başlatın (F5 veya Ctrl+F5).
