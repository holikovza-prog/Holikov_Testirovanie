using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using ForumParser.DbLibrary;

class Program
{
    static async Task Main(string[] args)
    {
        // Ссылка на ветку обсуждения на Hacker News
        string url = "https://news.ycombinator.com/item?id=352343";
        string dbPath = "forum_data.db";

        // Инициализируем БД
        var db = new DbManager(dbPath);

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

        try
        {
            Console.WriteLine($"Подключение к источнику: {url}...");
            string html = await client.GetStringAsync(url);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Находим все комментарии
            var commentNodes = doc.DocumentNode.SelectNodes("//tr[contains(@class, 'comtr')]");

            if (commentNodes == null)
            {
                Console.WriteLine("Ошибка: не удалось найти комментарии на странице.");
                return;
            }

            Console.WriteLine($"Найдено {commentNodes.Count} комментариев.");

            int savedCount = 0;

            foreach (var node in commentNodes)
            {
                try
                {
                    // Пробуем получить ID разными способами
                    string rawId = node.GetAttributeValue("id", "0");
                    Console.WriteLine($"DEBUG: rawId из tr = '{rawId}'");

                    // Если ID пустой или "0", попробуем найти скрытый input с именем id (на всякий случай)
                    if (rawId == "0")
                    {
                        var hiddenId = node.SelectSingleNode(".//input[@name='id']");
                        if (hiddenId != null)
                        {
                            rawId = hiddenId.GetAttributeValue("value", "0");
                            Console.WriteLine($"DEBUG: альтернативный ID = '{rawId}'");
                        }
                    }

                    // Если всё равно 0 - пропускаем
                    if (rawId == "0")
                    {
                        Console.WriteLine("DEBUG: не удалось определить ID, пропускаем комментарий");
                        continue;
                    }

                    long id = long.Parse(rawId);
                    Console.WriteLine($"DEBUG: распарсенный ID = {id}");

                    // Логин
                    var userNode = node.SelectSingleNode(".//a[@class='hnuser']");
                    string name = userNode != null ? userNode.InnerText.Trim() : "[deleted]";
                    Console.WriteLine($"DEBUG: имя = '{name}'");

                    // Текст
                    var messageNode = node.SelectSingleNode(".//span[contains(@class, 'commtext')]");
                    if (messageNode == null)
                        messageNode = node.SelectSingleNode(".//div[contains(@class, 'commtext')]");
                    if (messageNode == null)
                        messageNode = node.SelectSingleNode(".//*[contains(@class, 'comment')]");
                    if (messageNode == null)
                        messageNode = node.SelectSingleNode(".//*[contains(@class, 'c00')]");
                    if (messageNode == null)
                        messageNode = node.SelectSingleNode(".//td[@class='default']/div/span[@class='comment']"); // старый формат
                    if (messageNode == null)
                    {
                        Console.WriteLine("DEBUG: нет текста сообщения, пропускаем");
                        continue;
                    }
                    string message = messageNode.InnerHtml.Trim();
                    Console.WriteLine($"DEBUG: длина сообщения = {message.Length}");

                    // Проверяем, есть ли уже в БД
                    var existing = db.GetById(id);
                    Console.WriteLine($"DEBUG: запись существует? {(existing == null ? "нет" : "да")}");

                    if (existing == null)
                    {
                        var forumMsg = new ForumMessage { Id = id, Name = name, Message = message };
                        db.Add(forumMsg);
                        savedCount++;
                        Console.WriteLine($"УСПЕШНО ДОБАВЛЕН пост #{id}");
                    }
                    else
                    {
                        Console.WriteLine($"Пост #{id} уже есть в БД, пропускаем");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ОШИБКА при обработке комментария: {ex.Message}");
                }
            }

            Console.WriteLine("\n==============================================");
            Console.WriteLine($"Работа завершена. Добавлено новых записей: {savedCount}");
            Console.WriteLine("==============================================");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Критическая ошибка: {ex.Message}");
        }
    }
}