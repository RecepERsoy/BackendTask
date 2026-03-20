using MassTransit;
using Serilog;
using Product.Application.Events; // Bu namespace'in seninkiyle aynı olduğuna emin ol

namespace Log.Api.Consumers
{
    /// <summary>
    /// Event-Driven Architecture (Olay Yönelimli Mimari) kapsamında, RabbitMQ üzerinden gelen 
    /// ProductAddedEvent mesajlarını dinleyen "Consumer" (Tüketici/Abone) sınıfıdır.
    /// 
    /// Design Pattern Kararı: Publish-Subscribe (Pub/Sub) pattern kullanılmıştır. 
    /// Neden? Product servisi ile Log servisinin birbirine sıkı sıkıya bağlı (tightly-coupled) olmasını 
    /// engellemek ve bağımsız ölçeklenebilirlik sağlamak için bu asenkron iletişim tercih edilmiştir.
    /// Böylece Product servisi, Log servisinin ayakta olup olmamasından bağımsız çalışmaya devam edebilir.
    /// </summary>
    public class ProductAddedEvent : IConsumer<Product.Application.Events.ProductAddedEvent>
    {
        public async Task Consume(ConsumeContext<Product.Application.Events.ProductAddedEvent> context)
        {
            var message = context.Message;

            try
            {
                // 1. INFO (Information): Sistemin normal işleyişini gösteren standart loglar.
                // Mimari Karar: Task dökümanında istenen "Structured Logging" kuralı gereği '@' operatörü kullanılmıştır.
                // Neden? Logların salt metin yerine JSON objesi olarak tutulması, ileride Seq veya ELK Stack 
                // gibi log izleme araçlarında özellik bazlı (örneğin sadece belli bir UrunId'ye göre) sorgulama yapılabilmesini sağlar.
                Serilog.Log.Information("Yeni ürün ekleme olayı (Event) başarıyla yakalandı. {@ProductData}", new
                {
                    UrunId = message.Id,
                    UrunAdi = message.Name,
                    EklemeZamani = DateTime.UtcNow
                });

                // 2. WARNING (Uyarı): Sistemin çalışmasını durdurmayan ancak iş mantığı açısından 
                // dikkat edilmesi gereken eksik veya anormal durumların tespiti.
                if (string.IsNullOrWhiteSpace(message.Name))
                {
                    Serilog.Log.Warning("UYARI: Eklenen ürünün adı boş veya geçersiz geldi! Ürün ID: {UrunId}", message.Id);
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                // 3. ERROR (Hata): İşlem sırasında oluşan ve o anki akışı bozan standart hatalar.
                // Exception (ex) nesnesi loga dahil edilerek StackTrace gibi detayların merkezi log sistemine akması sağlanır.
                Serilog.Log.Error(ex, "HATA: Ürün verisi loglanırken beklenmedik bir hata oluştu! Ürün ID: {UrunId}", message.Id);

                // 4. CRITICAL / FATAL (Kritik): Uygulamanın bütünlüğünü tehdit eden kritik çökmeler.
                // Serilog kütüphanesinde Critical seviyesinin tam karşılığı Fatal olarak geçer.
                Serilog.Log.Fatal(ex, "KRİTİK HATA: Log servisi mesajı işleyemedi, veri kaybı veya altyapı sorunu riski mevcut!");

                // Mimari Karar: Hatayı yutmuyoruz (throw kullanıyoruz). 
                // Neden? MassTransit bu fırlatılan hatayı yakalayacak ve mesajı kaybetmemek için 
                // "Dead-Letter Queue" (Hata/Ölü Mesaj Kuyruğu) adı verilen güvenli bir kuyruğa taşıyacaktır. 
                // Bu sayede sistemde hata çıksa bile mesaj kalıcılığı (durability) garanti altına alınır.
                throw;
            }
        }
    }
}