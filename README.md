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

## API Reference

Базовые URL: `https://localhost:7202`, `http://localhost:7203`
Swagger: `https://localhost:7202/swagger/index.html`, `http://localhost:7203/swagger/index.html`

### Работа с событиями

URL: `/events`

| Метод | Эндпоинт | Описание | Статус |
| --- | --- | --- | --- |
| `GET` | `/` | Получение всего списка событий | 200 OK |
| `GET` | `/{id}` | Получение события по идентификатору | 200 OK, 404 NotFound |
| `POST` | `/` | Создание нового события | 201 Created, 400 BadRequest |
| `PUT` | `/{id}` | Обновление события | 204 NoContent, 400 BadRequest, 404 NotFound |
| `DELETE` | `/{id}` | Удаление события | 204 NoContent, 404 NotFound |