# EventManagementApi

Проектная работа на курсе Яндекс Практикум "Продвинутая разработка на C# и .Net"

Автор: **Погорелов М.В.**

## Запуск приложения

* клонируйте репозиторий
* установите зависимости
```
dotnet restore
```
* запустите проект
```
dotnet run
```
* при необходимости выполнения тестов
```
dotnet test
```


## API Reference

Базовые URL: `https://localhost:7202`, `http://localhost:7203`

Swagger: `https://localhost:7202/swagger/index.html`, `http://localhost:7203/swagger/index.html`

## Работа с событиями

URL: `/events`

| Метод    | Эндпоинт | Описание                            | Параметры                   | Статус                                      |
| -------- | -------- | ----------------------------------- | --------------------------- | ------------------------------------------- |
| `GET`    | `/`      | Получение списка событий            | title - Фильтр по названию; from - С даты; to - По дату; page - Номер страницы; pageSize - Размер страницы | 200 OK                                      |
| `GET`    | `/{id}`  | Получение события по идентификатору |                             | 200 OK, 404 NotFound                        |
| `POST`   | `/`      | Создание нового события             |                             | 201 Created, 400 BadRequest                 |
| `PUT`    | `/{id}`  | Обновление события                  |                             | 204 NoContent, 400 BadRequest, 404 NotFound |
| `DELETE` | `/{id}`  | Удаление события                    |                             | 204 NoContent, 404 NotFound                 |

### Примеры положительных ответов
Конкретные ответы можно посмотреть в документации по swagger
```json
{
  "success": true,
  "statusCode": 200,
  "dateTime": "2026-04-20T08:10:30.256Z",
  "message": "string",
  "data": {
    "id": 1,
    "title": "string",
    "description": "string",
    "startAt": "2026-04-20T08:10:30.256Z",
    "endAt": "2026-04-20T08:10:30.256Z"
  }
}
```
```json
{
  "success": true,
  "statusCode": 100,
  "dateTime": "2026-04-20T08:10:30.249Z",
  "message": "string",
  "data": [
    {
      "id": 0,
      "title": "string",
      "description": "string",
      "startAt": "2026-04-20T08:10:30.249Z",
      "endAt": "2026-04-20T08:10:30.249Z"
    }
  ],
  "page": 0,
  "pageSize": 0,
  "totalItems": 0,
  "totalPages": 0
}
```

### Примеры возвращаемых ошибок
Конкретные ответы можно посмотреть в документации по swagger
```json
{
  "success": false,
  "statusCode": 400,
  "dateTime": "2026-04-20T08:10:30.252Z",
  "message": "string"
}
```