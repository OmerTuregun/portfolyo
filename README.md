# Kişisel Portfolyo Sitem (ASP.NET Core & Docker)

Bu proje, ASP.NET Core MVC, Bootstrap 5 ve modern CSS teknikleri kullanılarak oluşturulmuş, Docker üzerinde çalışmak üzere tasarlanmış kişisel bir portfolyo web sitesidir.

# ✨ Öne Çıkan Özellikler

Modern Arayüz: "Glassmorphism" (Cam Efekti) ve canlı "fıstık yeşili" renk paleti.

Tamamen Responsive: Mobil, tablet ve masaüstü cihazlarla uyumludur.

Dinamik Animasyonlar: Sayfa kaydırıldığında dolan yetenek barları gibi interaktif öğeler.

Docker Desteği: Proje, docker-compose ile tek komutta ayağa kalkacak şekilde yapılandırılmıştır.

# 🚀 Projeyi Çalıştırma (Docker ile)

Bu projeyi çalıştırmanın en kolay ve tavsiye edilen yolu Docker kullanmaktır.

Ön Gereksinimler

Docker Desktop'ın bilgisayarınızda kurulu ve çalışır durumda olması gerekmektedir.

Kurulum Adımları

# Projeyi Klonlayın:

git clone [https://github.com/OmerTuregun/my-portfolyo.git](https://github.com/OmerTuregun/my-portfolyo.git)
cd my-portfolyo


Dockerfile'ı Gözden Geçirin (Opsiyonel ama Önemli):
Dockerfile dosyasını açın ve en alttaki ENTRYPOINT satırının, projenizin .dll adıyla eşleştiğinden emin olun. (Genellikle My-Portfolyo.dll veya proje adınız neyse o olmalıdır).

Dockerfile'dan örnek satır:

# ...
# Proje adınızın "My-Portfolyo.dll" olduğunu varsayıyoruz
ENTRYPOINT ["dotnet", "My-Portfolyo.dll"]


Docker Compose'u Çalıştırın:
Terminalde, proje ana dizinindeyken (docker-compose.yml dosyasının olduğu yer) aşağıdaki komutu çalıştırın:

docker-compose up -d --build


--build: Docker'ın projenizi Dockerfile tarifine göre sıfırdan derlemesini sağlar.

-d: Konteyneri arka planda (detached mod) çalıştırır.

# Siteye Erişin:
Build işlemi bittikten sonra tarayıcınızı açın ve adres çubuğuna şunu yazın:

http://localhost:5000

Tebrikler! Portfolyo siteniz artık bir Docker konteyneri içinde çalışıyor.