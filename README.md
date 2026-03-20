Projenin tüm baðýmlýlýklarý ve servisleri Docker üzerinden veya local olarak ayaða kaldýrýlabilir.
Çalýþtýrma Adýmlarý
Projeyi Klonlayýn:
git clone https://github.com/RecepERsoy/BackendTask.git
Docker Compose ile Baþlatýn:
docker-compose up -d --build
Bu komut, Docker Compose dosyasýný kullanarak tüm servisleri baþlatýr ve gerekli baðýmlýlýklarý oluþturur.

Servis Eriþimleri:

Auth API (Swagger): http://localhost:5001/swagger

Product API (Swagger): http://localhost:5002/swagger

Log API (Console): docker logs -f backendtask-log-api-1

RabbitMQ Management: http://localhost:15672 (guest/guest)

Redis: localhost:6379

Ürün listeleme, ürün ekleme ve ürün güncellemeyi sadece admin yapabilir.
Admin kullanýcý bilgileri:
Email:admin@sirket.com
Password:admin123

Kod Deposu Baðlantýsý: https://github.com/RecepERsoy/BackendTask
