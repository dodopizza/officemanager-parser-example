# officemanager-parser-example

Пример бота для парсинга интерфейсов "Менеджера Офиса", работающих на новом сервисе авторизации.

# admin-web-parser-example

Пример бота для парсинга интерфейсов "Администратора Подразделения", работающих на новом сервисе авторизации.
Запуск аналогичен парсеру "Менеджера Офиса".

# shift-manager-parser-example

Пример бота для парсинга интерфейсов "Менеджера Смены", работающих на новом сервисе авторизации.
Запуск аналогичен парсеру "Менеджера Офиса".

## Как запустить локально

Установить .NET SDK не ниже версии 6.0.

В папке с проектом создать локальный конфиг на основе базового:

```shell
cp ParserExamples/OfficeManagerParserExample/appsettings.json ParserExamples/OfficeManagerParserExample/appsettings.local.json
```

Заполнить `appsettings.local.json` своими значениями:

- `AuthBaseUrl` - базовый url сервиса авторизации (если отличается от production)
- `OfficeManagerBaseUrl` - базовый url "Менеджера Офиса" (если отличается от production)
- `Username` - логин парсера в Dodo IS
- `Password` - пароль парсера в Dodo IS
- `RoleId` - уникальный идентификатор роли в Dodo IS, под которой должен заходить парсер
- `DepartmentUuid` - Uuid департамента в Dodo IS, в который должен заходить парсер

Выполнить команду:

```shell
dotnet run --project ParserExamples/OfficeManagerParserExample
```
