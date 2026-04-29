using System;

namespace TriangleLab3
{
    // Интерфейс для взаимодействия с пользователем
    public interface IUserInput
    {
        (string SideA, string SideB, string SideC) GetTriangleSides();  // Получение информации от пользователя
        void ShowMessage(string message);
        void ShowResult(string type, string errorMessage);
    }

    // Класс c: реализация взаимодействия с пользователем (через консоль)
    public class ConsoleUserInput : IUserInput
    {
        public (string SideA, string SideB, string SideC) GetTriangleSides()
        {
            Console.Write("Введите сторону A: ");
            string sideA = Console.ReadLine() ?? "";

            Console.Write("Введите сторону B: ");
            string sideB = Console.ReadLine() ?? "";

            Console.Write("Введите сторону C: ");
            string sideC = Console.ReadLine() ?? "";

            return (sideA, sideB, sideC);
        }

        public void ShowMessage(string message)
        {
            Console.WriteLine(message);
        }

        public void ShowResult(string type, string errorMessage)
        {
            if (string.IsNullOrEmpty(type))
            {
                Console.WriteLine($"Ошибка: {errorMessage}");
            }
            else if (type == "не треугольник")
            {
                Console.WriteLine($"Результат: {type} ({errorMessage})");
            }
            else
            {
                Console.WriteLine($"Результат: {type} треугольник");
            }
        }
    }
}