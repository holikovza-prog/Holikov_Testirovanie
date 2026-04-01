
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Sheet1_MasterID' AND object_id = OBJECT_ID('[dbo].[Sheet1$]'))
BEGIN
    DROP INDEX IX_Sheet1_MasterID ON [dbo].[Sheet1$];
    PRINT 'Индекс IX_Sheet1_MasterID удален.';
END

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Sheet1_ClientID' AND object_id = OBJECT_ID('[dbo].[Sheet1$]'))
BEGIN
    DROP INDEX IX_Sheet1_ClientID ON [dbo].[Sheet1$];
    PRINT 'Индекс IX_Sheet1_ClientID удален.';
END

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Sheet1_Клиент' AND object_id = OBJECT_ID('[dbo].[Sheet1$]'))
BEGIN
    DROP INDEX IX_Sheet1_Клиент ON [dbo].[Sheet1$];
    PRINT 'Индекс IX_Sheet1_Клиент удален.';
END

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Sheet1_Услуга' AND object_id = OBJECT_ID('[dbo].[Sheet1$]'))
BEGIN
    DROP INDEX IX_Sheet1_Услуга ON [dbo].[Sheet1$];
    PRINT 'Индекс IX_Sheet1_Услуга удален.';
END
GO


-- 2. Приведение типов данных к INT

-- Для таблицы Лист1$ (комментарии) - правильные названия столбцов
ALTER TABLE [dbo].[Лист1$] ALTER COLUMN [commentID] INT NOT NULL;
ALTER TABLE [dbo].[Лист1$] ALTER COLUMN [masterID] INT NULL;
ALTER TABLE [dbo].[Лист1$] ALTER COLUMN [requestID] INT NOT NULL;
PRINT 'Типы данных в Лист1$ изменены.';

-- Для таблицы Лист2$ (пользователи)
ALTER TABLE [dbo].[Лист2$] ALTER COLUMN [userID] INT NOT NULL;
PRINT 'Типы данных в Лист2$ изменены.';

-- Для таблицы Sheet1$ (заявки)
ALTER TABLE [dbo].[Sheet1$] ALTER COLUMN [requestID] INT NOT NULL;
ALTER TABLE [dbo].[Sheet1$] ALTER COLUMN [masterID] INT NULL;
ALTER TABLE [dbo].[Sheet1$] ALTER COLUMN [clientID] INT NOT NULL;
PRINT 'Типы данных в Sheet1$ изменены.';
GO


-- 3. Добавление первичных ключей

-- Первичный ключ для таблицы Лист1$ (комментарии) - по commentID
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PK_Лист1_commentID]') AND type = 'PK')
BEGIN
    ALTER TABLE [dbo].[Лист1$] ADD CONSTRAINT PK_Лист1_commentID PRIMARY KEY ([commentID]);
    PRINT 'Первичный ключ PK_Лист1_commentID создан.';
END
ELSE
    PRINT 'Первичный ключ PK_Лист1_commentID уже существует.';

-- Первичный ключ для таблицы Лист2$ (пользователи)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PK_Лист2_userID]') AND type = 'PK')
BEGIN
    ALTER TABLE [dbo].[Лист2$] ADD CONSTRAINT PK_Лист2_userID PRIMARY KEY ([userID]);
    PRINT 'Первичный ключ PK_Лист2_userID создан.';
END
ELSE
    PRINT 'Первичный ключ PK_Лист2_userID уже существует.';
GO


-- 4. Обработка NULL в startDate (если нужно)

DECLARE @nullCount INT;
SELECT @nullCount = COUNT(*) FROM [dbo].[Sheet1$] WHERE [startDate] IS NULL;

PRINT 'Количество NULL в [startDate]: ' + CAST(@nullCount AS VARCHAR(10));

IF @nullCount > 0
BEGIN
    UPDATE [dbo].[Sheet1$] 
    SET [startDate] = GETDATE()
    WHERE [startDate] IS NULL;
    PRINT 'Обновлено ' + CAST(@@ROWCOUNT AS VARCHAR) + ' записей с NULL.';
END
GO

-- 5. Создание первичного ключа для Sheet1$
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PK_Sheet1_requestID]') AND type = 'PK')
BEGIN
    PRINT 'Создание первичного ключа PK_Sheet1_requestID...';
    ALTER TABLE [dbo].[Sheet1$] 
    ADD CONSTRAINT PK_Sheet1_requestID PRIMARY KEY ([requestID]);
    PRINT 'Первичный ключ PK_Sheet1_requestID создан.';
END
ELSE
    PRINT 'Первичный ключ PK_Sheet1_requestID уже существует.';
GO

-- 6. Внешние ключи
-- 
-- Связь: Sheet1$.masterID -> Лист2$.userID (исполнитель)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Sheet1_Master')
BEGIN
    ALTER TABLE [dbo].[Sheet1$]
    ADD CONSTRAINT FK_Sheet1_Master FOREIGN KEY ([masterID]) 
    REFERENCES [dbo].[Лист2$] ([userID]);
    PRINT 'Внешний ключ FK_Sheet1_Master создан.';
END
ELSE
    PRINT 'Внешний ключ FK_Sheet1_Master уже существует.';

-- Связь: Sheet1$.clientID -> Лист2$.userID (клиент)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Sheet1_Client')
BEGIN
    ALTER TABLE [dbo].[Sheet1$]
    ADD CONSTRAINT FK_Sheet1_Client FOREIGN KEY ([clientID]) 
    REFERENCES [dbo].[Лист2$] ([userID]);
    PRINT 'Внешний ключ FK_Sheet1_Client создан.';
END
ELSE
    PRINT 'Внешний ключ FK_Sheet1_Client уже существует.';

-- Связь: Лист1$.requestID -> Sheet1$.requestID (комментарии к заявке)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Лист1_Request')
BEGIN
    ALTER TABLE [dbo].[Лист1$]
    ADD CONSTRAINT FK_Лист1_Request FOREIGN KEY ([requestID]) 
    REFERENCES [dbo].[Sheet1$] ([requestID]);
    PRINT 'Внешний ключ FK_Лист1_Request создан.';
END
ELSE
    PRINT 'Внешний ключ FK_Лист1_Request уже существует.';

-- Связь: Лист1$.masterID -> Лист2$.userID (мастер, оставивший комментарий)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Лист1_Master')
BEGIN
    ALTER TABLE [dbo].[Лист1$]
    ADD CONSTRAINT FK_Лист1_Master FOREIGN KEY ([masterID]) 
    REFERENCES [dbo].[Лист2$] ([userID]);
    PRINT 'Внешний ключ FK_Лист1_Master создан.';
END
ELSE
    PRINT 'Внешний ключ FK_Лист1_Master уже существует.';
GO


--  Создание индексов

-- Индекс на masterID
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Sheet1_MasterID' AND object_id = OBJECT_ID('[dbo].[Sheet1$]'))
BEGIN
    CREATE INDEX IX_Sheet1_MasterID ON [dbo].[Sheet1$] ([masterID]);
    PRINT 'Индекс IX_Sheet1_MasterID создан.';
END
ELSE
    PRINT 'Индекс IX_Sheet1_MasterID уже существует.';

-- Индекс на clientID
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Sheet1_ClientID' AND object_id = OBJECT_ID('[dbo].[Sheet1$]'))
BEGIN
    CREATE INDEX IX_Sheet1_ClientID ON [dbo].[Sheet1$] ([clientID]);
    PRINT 'Индекс IX_Sheet1_ClientID создан.';
END
ELSE
    PRINT 'Индекс IX_Sheet1_ClientID уже существует.';

-- Индекс на requestID в таблице комментариев
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Лист1_RequestID' AND object_id = OBJECT_ID('[dbo].[Лист1$]'))
BEGIN
    CREATE INDEX IX_Лист1_RequestID ON [dbo].[Лист1$] ([requestID]);
    PRINT 'Индекс IX_Лист1_RequestID создан.';
END
ELSE
    PRINT 'Индекс IX_Лист1_RequestID уже существует.';

-- Индекс на masterID в таблице комментариев
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Лист1_MasterID' AND object_id = OBJECT_ID('[dbo].[Лист1$]'))
BEGIN
    CREATE INDEX IX_Лист1_MasterID ON [dbo].[Лист1$] ([masterID]);
    PRINT 'Индекс IX_Лист1_MasterID создан.';
END
ELSE
    PRINT 'Индекс IX_Лист1_MasterID уже существует.';
GO


PRINT 'Все операции завершены успешно!';

GO