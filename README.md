# Multiplayer Shooter

Минимальный сетевой 1v1-шутер на Unity 6 с авторитетным выделенным сервером, Photon Fusion 2 и PlayFab Authentication, Matchmaking и Multiplayer Servers.

## Что необходимо для запуска

- Unity 6 с поддержкой Windows Dedicated Server.
- Настроенный Photon Fusion App ID в проекте.
- PlayFab Title ID. Это публичный идентификатор проекта из PlayFab Game Manager, а не секретный ключ.
- Для PlayFab Unified SDK на каждом клиентском компьютере должны быть установлены или обновлены Microsoft Gaming Services.
- Для облачного матча должны быть настроены PlayFab Multiplayer Servers и очередь 1v1. Подробная инструкция находится в [PLAYFAB_DEPLOYMENT.md](PLAYFAB_DEPLOYMENT.md).

Все команды ниже выполняются в PowerShell. Если не указано обратное, текущей директорией должен быть корень репозитория.

## Сборка клиента и локального сервера

В Unity выберите:

```text
Shooter > Local Development > Build Development Client and Dedicated Server (Clean)
```

Будут созданы Development-сборки с включённым Script Debugging:

```text
Builds/Local/Client/ShooterClient.exe
Builds/Local/Server/ShooterServer.exe
```

Для сборки без предварительной очистки используйте `Build Development Client and Dedicated Server`, а для удаления существующих сборок — `Clean Builds`.

## Сценарий 1: локальный сервер и два клиента на одном компьютере

Запустите:

```powershell
.\RunLocalMatch.ps1 -PlayFabTitleId ABCDE
```

Скрипт запустит выделенный сервер, дождётся его готовности и откроет два клиента с разными PlayFab Custom ID. В обоих клиентах нажмите `Find Game`.

При необходимости можно задать имя Fusion-сессии, порт и увеличенное время ожидания сервера:

```powershell
.\RunLocalMatch.ps1 -PlayFabTitleId ABCDE -SessionName local-test-2 -Port 28000 -ServerReadyTimeoutSeconds 120
```

Этот сценарий использует локальный matchmaking-адаптер и не создаёт тикеты в очереди PlayFab.

## Сценарий 2: PlayFab matchmaking с двумя клиентами на одном компьютере

Сначала убедитесь, что серверный build в PlayFab находится в состоянии `Deployed`, в выбранном регионе есть сервер `StandingBy`, а очередь ссылается на этот build или Build Alias.

Запустите два клиента:

```powershell
.\RunPlayFabMatch.ps1 -PlayFabTitleId ABCDE -QueueName quick-1v1
```

В обоих окнах нажмите `Find Game`. Скрипт использует разные идентификаторы `client-a` и `client-b`, поэтому PlayFab создаёт два независимых игрока и два matchmaking-тикета.

Логи этого сценария находятся здесь:

```text
Builds/Local/Logs/playfab-client-a.log
Builds/Local/Logs/playfab-client-b.log
```

## Сценарий 3: PlayFab matchmaking с клиентами на двух физических компьютерах

1. Соберите Development-клиент и скопируйте на второй компьютер всю директорию `Builds/Local/Client`, а не только `ShooterClient.exe`.
2. Установите Microsoft Gaming Services на обоих компьютерах.
3. Используйте одинаковые PlayFab Title ID и имя очереди, но разные `--shooter-player-id`.

На первом компьютере, из директории со сборкой клиента:

```powershell
.\ShooterClient.exe --shooter-playfab-title-id ABCDE --shooter-player-id physical-pc-a --shooter-playfab-matchmaking-queue quick-1v1 -logFile client-a.log
```

На втором компьютере:

```powershell
.\ShooterClient.exe --shooter-playfab-title-id ABCDE --shooter-player-id physical-pc-b --shooter-playfab-matchmaking-queue quick-1v1 -logFile client-b.log
```

После успешной аутентификации нажмите `Find Game` на обоих компьютерах. Клиентам не требуется находиться в одной локальной сети: они создают тикеты в одной очереди PlayFab и подключаются к выделенному серверу по выданному PlayFab адресу.

Если появляется `Failed to initialize XGameRuntime (HRESULT 0x89240101)`, на этом компьютере отсутствует или повреждён Gaming Services Runtime. Установите или восстановите приложение `Gaming Services` через Microsoft Store и перезагрузите компьютер.

## Сценарий 4: ручной запуск одного PlayFab-клиента

Этот вариант удобен для проверки конкретного идентификатора игрока или для запуска клиента из Rider/терминала:

```powershell
.\Builds\Local\Client\ShooterClient.exe --shooter-playfab-title-id ABCDE --shooter-player-id test-player-1 --shooter-playfab-matchmaking-queue quick-1v1 -logFile .\Builds\Local\Logs\manual-client.log
```

Для второго участника запустите ещё один экземпляр с другим `--shooter-player-id` и другим файлом лога.

## Остановка процессов

Корректно завершить сервер и клиентов, запущенных скриптами:

```powershell
.\StopLocalMatch.ps1
```

Предварительно посмотреть список процессов без их остановки:

```powershell
.\StopLocalMatch.ps1 -WhatIf
```

Скрипт сначала запрашивает штатное завершение Fusion-сессии и только затем принудительно останавливает процессы, которые не завершились вовремя.

## Проверка локального матча

Проверить логи на известные ошибки запуска и выполнения:

```powershell
.\ValidateLocalMatch.ps1
```

Потребовать наличие полностью завершённого матча:

```powershell
.\ValidateLocalMatch.ps1 -RequireCompletedMatch
```

Проверить два последовательных матча через `Find Game Again`:

```powershell
.\ValidateLocalMatch.ps1 -RequireRepeatedMatch
```

## Сборка сервера для PlayFab

В Unity выберите:

```text
Shooter > PlayFab Deployment > Build Production Dedicated Server Package
```

Готовый архив получает уникальное имя вида:

```text
Builds/PlayFab/ShooterServer-<UTC timestamp>-<GUID>.zip
```

Его нужно загрузить в PlayFab Multiplayer Servers. Настройки build, порта `game_port`, региона, очереди, Build Alias и порядок проверки termination подробно описаны в [PLAYFAB_DEPLOYMENT.md](PLAYFAB_DEPLOYMENT.md).

## Дополнительная документация

- [LOCAL_DEVELOPMENT.md](LOCAL_DEVELOPMENT.md) — устройство локального запуска, сборка, логи, отладка через Rider и проверка повторного матча.
- [PLAYFAB_DEPLOYMENT.md](PLAYFAB_DEPLOYMENT.md) — развёртывание выделенного сервера и настройка matchmaking в PlayFab.
- [PROJECT_SPEC.md](PROJECT_SPEC.md) — требования и границы MVP.
- [ARCHITECTURE.md](ARCHITECTURE.md) — архитектура проекта и зависимости сборок.
