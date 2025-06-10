# QuickChat.Server  
**Кроссплатформенный мессенджер с поддержкой реального времени**  

Описание  
Серверная часть приложения QuickChat, реализующая REST API для управления чатами, пользователями и сообщениями с использованием SignalR для мгновенной доставки сообщений.  

Технологический стек  
- ASP.NET Core Web API  
- SignalR (реальная доставка сообщений)  
- PostgreSQL (основная база данных)  
- Entity Framework Core (ORM)  
- JWT-аутентификация + BCrypt  
- Swagger (документация API)  

## Эндпоинты API  
### Аутентификация  
- `POST /api/auth/register` - Регистрация  
- `POST /api/auth/login` - Авторизация (получение JWT)  

### Пользователи  
- `GET /api/users` - Список пользователей  
- `PUT /api/users/{userId}` - Обновление данных  

### Чаты  
- `GET /api/chats` - Список чатов пользователя  
- `POST /api/chats` - Создание чата  

### Сообщения  
- `GET /api/messages/{chatId}` - Получение сообщений (с пагинацией)  
- `POST /api/messages` - Отправка сообщения  

### SignalR Hub  
- `/chathub` - Веб-сокет соединение для:  
  - Отправки/получения сообщений  
  - Обновления статусов онлайн  
  - Уведомлений о прочтении  

Запуск проекта  
1. Установите PostgreSQL и создайте БД  
2. Настройте строку подключения в `appsettings.json`:  
```json 
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=QuickChatDB;Username=postgres;Password=your_password"
}
