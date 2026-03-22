using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NewsApp
{
    public class NewsItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public string PubDate { get; set; }
        
        public override string ToString()
        {
            return $"📰 {Title}\n   📅 {PubDate}\n   📝 {Description?.Substring(0, Math.Min(100, Description?.Length ?? 0))}...\n";
        }
    }

    class Program
    {
        private static readonly Dictionary<int, string> NewsSources = new Dictionary<int, string>
        {
            { 1, "https://lenta.ru/rss/news" },
            { 2, "https://habr.com/ru/rss/news/" },
        };

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== 📰 NewsApp v1.0 ===");
            Console.WriteLine("Автор: Влад\n");
            
            ShowMenu();
            
            while (true)
            {
                Console.Write("\nВыберите источник (1-2) или 0 для выхода: ");
                var choice = Console.ReadLine();
                
                if (choice == "0") break;
                
                if (int.TryParse(choice, out int sourceId) && NewsSources.ContainsKey(sourceId))
                {
                    await LoadNewsAsync(NewsSources[sourceId]);
                }
                else
                {
                    Console.WriteLine("❌ Неверный выбор!");
                }
            }
            
            Console.WriteLine("👋 До свидания!");
        }

        static void ShowMenu()
        {
            Console.WriteLine("Доступные источники:");
            Console.WriteLine("1. Lenta.ru");
            Console.WriteLine("2. Habr News");
            Console.WriteLine("0. Выход");
        }

        static async Task LoadNewsAsync(string rssUrl)
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "NewsApp/1.0");
                
                Console.WriteLine($"\n⏳ Загрузка новостей из {rssUrl}...");
                var rssContent = await client.GetStringAsync(rssUrl);
                
                var news = ParseRss(rssContent);
                
                Console.WriteLine($"\n✅ Загружено {news.Count} новостей:\n");
                Console.WriteLine(new string('=', 50));
                
                for (int i = 0; i < news.Count && i < 10; i++)
                {
                    Console.WriteLine($"{i + 1}. {news[i]}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка загрузки: {ex.Message}");
            }
        }

        static List<NewsItem> ParseRss(string rssContent)
        {
            var news = new List<NewsItem>();
            var doc = XDocument.Parse(rssContent);
            
            // RSS 2.0 формат
            var items = doc.Descendants("item");
            
            foreach (var item in items)
            {
                news.Add(new NewsItem
                {
                    Title = item.Element("title")?.Value ?? "Без заголовка",
                    Description = item.Element("description")?.Value ?? "",
                    Link = item.Element("link")?.Value ?? "",
                    PubDate = item.Element("pubDate")?.Value ?? ""
                });
            }
            
            return news;
        }
    }
}
