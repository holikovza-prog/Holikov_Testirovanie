using System;

namespace TriangleLab3
{
    // Класс e: Controller - реализует бизнес-логику
    public class TriangleController
    {
        private readonly IDatabase _database;
        private readonly IUserInput _userInput;
        private readonly IExternalService _externalService;

        public TriangleController(IDatabase database, IUserInput userInput, IExternalService externalService)
        {
            _database = database;
            _userInput = userInput;
            _externalService = externalService;
        }

        // Основной метод бизнес-логики
        public TriangleResult ProcessRequest()
        {
            // 1. Запрашиваем входные данные у пользователя
            var (sideA, sideB, sideC) = _userInput.GetTriangleSides();

            return ProcessRequestWithData(sideA, sideB, sideC);
        }

        // Метод с передачей данных (для удобства тестирования)
        public TriangleResult ProcessRequestWithData(string sideA, string sideB, string sideC)
        {
            // 2. Проверка: если для данных нет записи в БД -> вычисляем и добавляем
            //            если есть запись в БД -> берем результат из БД
            var existingRecord = _database.GetRecord(sideA, sideB, sideC);

            TriangleResult result;

            if (existingRecord != null)
            {
                // Результат из БД
                result = new TriangleResult(
                    existingRecord.TriangleType,
                    new System.Collections.Generic.List<(int X, int Y)>(),
                    existingRecord.ErrorMessage
                );
                _userInput.ShowMessage($"→ Результат получен из БД (кэш)");
            }
            else
            {
                // Вычисляем результат
                result = TriangleCalculator.CalculateTriangle(sideA, sideB, sideC);

                // Сохраняем в БД
                var record = new TriangleRecord
                {
                    SideA = sideA,
                    SideB = sideB,
                    SideC = sideC,
                    TriangleType = result.Type,
                    ErrorMessage = result.ErrorMessage
                };
                _database.AddRecord(record);

                _userInput.ShowMessage($"→ Результат вычислен и сохранен в БД");
            }

            // 3. Отправка строки-результата сторонней зависимости (email-серверу)
            string notificationData = $"Треугольник: стороны={sideA},{sideB},{sideC} | тип={result.Type} | ошибка={result.ErrorMessage}";
            bool sent = _externalService.SendNotification(notificationData);

            if (!sent)
            {
                _userInput.ShowMessage("⚠ Предупреждение: не удалось отправить уведомление на email-сервер");
            }

            // 4. Возвращаем результат выполнения операции
            _userInput.ShowResult(result.Type, result.ErrorMessage);

            return result;
        }

        // Дополнительные методы для управления
        public bool IsInCache(string sideA, string sideB, string sideC)
        {
            return _database.HasRecord(sideA, sideB, sideC);
        }

        public void ClearCache()
        {
            _database.ClearAll();
            _userInput.ShowMessage("Кэш БД очищен");
        }

        public void ShowAllRecords()
        {
            var records = _database.GetAllRecords();
            _userInput.ShowMessage($"\nВсего записей в БД: {records.Count}");
            foreach (var r in records)
            {
                _userInput.ShowMessage($"  {r.SideA}, {r.SideB}, {r.SideC} -> {r.TriangleType}");
            }
        }
    }
}