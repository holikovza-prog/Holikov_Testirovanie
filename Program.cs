using System;

namespace TriangleLab3
{
    class Program
    {
        static void Main(string[] args)
        {
            // Инициализация всех компонентов
            IDatabase database = new SQLiteDatabase("triangles.db");
            IUserInput userInput = new ConsoleUserInput();
            IExternalService emailService = new MockEmailService();

            // Создание контроллера
            var controller = new TriangleController(database, userInput, emailService);

            Console.WriteLine("=".PadRight(60, '='));
            Console.WriteLine("    КАЛЬКУЛЯТОР ТРЕУГОЛЬНИКА (с кэшированием)");
            Console.WriteLine("=".PadRight(60, '='));
            Console.WriteLine();
            Console.WriteLine("Программа кэширует результаты в БД SQLite");
            Console.WriteLine("При повторном вводе тех же сторон результат берется из кэша");
            Console.WriteLine();

            while (true)
            {
                Console.WriteLine("\n--- МЕНЮ ---");
                Console.WriteLine("1 - Вычислить треугольник");
                Console.WriteLine("2 - Показать все записи в БД");
                Console.WriteLine("3 - Очистить кэш БД");
                Console.WriteLine("4 - Выход");
                Console.Write("Выберите действие: ");

                string choice = Console.ReadLine() ?? "";

                switch (choice)
                {
                    case "1":
                        controller.ProcessRequest();
                        break;

                    case "2":
                        controller.ShowAllRecords();
                        break;

                    case "3":
                        controller.ClearCache();
                        break;

                    case "4":
                        Console.WriteLine("До свидания!");
                        return;

                    default:
                        Console.WriteLine("Неверный выбор. Попробуйте снова.");
                        break;
                }
            }
        }
    }
}