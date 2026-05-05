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

## Модели и статусы

```
/// <summary>
/// Модель события
/// </summary>
public record Event
{
    /// <summary>
    /// Уникальный идентификатор события
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Название события
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Описание события
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Дата начала
    /// </summary>
    public required DateTime StartAt { get; set; }

    /// <summary>
    /// Дата окончания
    /// </summary>
    public required DateTime EndAt { get; set; }
}
```
```
/// <summary>
/// Бронь
/// </summary>
public record Booking
{
    /// <summary>
    /// Уникальный идентификатор брони
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Идентификатор события, к которому относится бронь
    /// </summary>
    public required Guid EventId { get; init; }

    /// <summary>
    /// Текущий статус брони
    /// </summary>
    public required BookingStatusEnum Status { get; set; }

    /// <summary>
    /// Дата и время создания брони
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Дата и время обработки брони
    /// </summary>
    public DateTime? ProcessedAt { get; set; }
}
```
```
/// <summary>
/// Статус брони
/// </summary>
public enum BookingStatusEnum
{
    /// <summary>
    /// Бронь создана, ожидает обработки
    /// </summary>
    Pending,

    /// <summary>
    /// Бронь подтверждена
    /// </summary>
    Confirmed,

    /// <summary>
    /// Бронь отклонена
    /// </summary>
    Rejected
}
```


## Работа с событиями

URL: `/events`

| Метод    | Эндпоинт | Описание                            | Параметры                   | Статус                                      |
| -------- | -------- | ----------------------------------- | --------------------------- | ------------------------------------------- |
| `GET`    | `/`      | Получение списка событий            | title - Фильтр по названию; from - С даты; to - По дату; page - Номер страницы; pageSize - Размер страницы | 200 OK                                      |
| `GET`    | `/{id}`  | Получение события по идентификатору |                             | 200 OK, 404 NotFound                        |
| `POST`   | `/`      | Создание нового события             |                             | 201 Created, 400 BadRequest                 |
| `PUT`    | `/{id}`  | Обновление события                  |                             | 204 NoContent, 400 BadRequest, 404 NotFound |
| `DELETE` | `/{id}`  | Удаление события                    |                             | 204 NoContent, 404 NotFound                 |
| 'POST'   | `/{id}/book` | Создание брони для события      |                             | 202 Accepted, 400 BadRequest, 404 NotFound  |

## Работа с бронью

URL: `/bookings`

| Метод    | Эндпоинт       | Описание                          | Параметры                   | Статус                                      |
| -------- | -------------- | ----------------------------------| --------------------------- | ------------------------------------------- |
| `GET`    | `/{bookingId}` | Получение брони по идентификатору |                             | 200 OK, 404 NotFound                        |


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
  "statusCode": 200,
  "dateTime": "2026-04-20T08:10:30.249Z",
  "message": "string",
  "data": [
    {
      "id": 1,
      "title": "string",
      "description": "string",
      "startAt": "2026-04-20T08:10:30.249Z",
      "endAt": "2026-04-20T08:10:30.249Z"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalItems": 1,
  "totalPages": 1
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