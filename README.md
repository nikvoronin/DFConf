# DFConf

A configuration utility for `Danfoss FC302` with `MCB-103 Extension` (resolver).

Конфигуратор синхронного привода `Danfoss FC302` с резольвером `MCB-103`.

> Это экспериментальное ПО. Используйте его на свой страх и риск!

```log
Конфигуратор синхронного привода с резольвером [1.0.4209.26870]
Danfoss FC302 + MCB-103
(c) Nikolai Voronin, 2011

Краткая справка:
   ПК  - персональный компьютер.
   ПЧ  - частотный привод (инвертер).
   LCP - панель оператора ПЧ.
    №  - выберите пункт меню и введите его номер.
    =  - введите числовой параметр.
    >  - нажмите любую клавишу для продолжения.
    *  - для продолжения, выполните требуемое действие.

ВНИМАНИЕ! Отсоедините механическую передачу от синхронного двигателя.
          Нажмите на LCP клавишу `Auto On`.

 *** Шаг 0: Подключение ПЧ к ПК

Выберите COM-порт ПК для подключения:
    1: COM8
  № 1

  Частотный привод:
    Тип       : FC-302
    Мощность  : P1K5: 1.50kW
    Питание   : T5: 3 X 380-500VAC
    Версия ПО : 06.33
    Тип LCP   : Graphical V 13

  Опциональные платы:
    A:X No option
    B:U MCB103 CL Resolver


 *** Шаг 1: Сброс ПЧ в заводские настройки

    1. Сбросить все настройки ПЧ в заводские настройки.
    2. Сбросить в заводские только настройки двигателя.
    3. Пропустить шаг и перейти к настройкам двигателя.
    4. Перейти к пробному пуску двигателя.
  № 1

Выполните действия перечисленные ниже.
После выполнения требуемых действий, нажмите любую клавишу.
Если требуемое действие невозможно, перезапустите программу и повторите.

  * Отключите силовое питание ПЧ.
    Силовое питание выключено.
  * Подождите не менее 10 секунд и включите силовое питание.
    Силовое питание включено.
  * Нажмите клавишу Reset на LCP.
    Ошибка сброшена.

Загрузка параметров.............ОК

ПЧ в заводских настройках.

 *** Шаг 2: Задание настроек двигателя

Текущие параметры двигателя:
    Номинальный ток                = 2,2 А
    Номинальная частота вращения   = 3000 об/мин
    Номинальная частота двигателя  = 50 Гц
    Выходная частота               = 132 Гц
    Макс. частота вращения         = 1980 об/мин
    Номинальный момент             = 7,2 Н*м
    Сопротивление статора (Rs)     = 1,255 Ом
    Индуктивность (Ld)             = 10 мГн
    Число полюсов двигателя        = 8
    BEMF на 1000 об/мин            = 99
    Ограничение по току            = 160 %
    Ограничение момента, мотор     = 160 %
    Ограничение момента, генератор = 100 %

    1. Изменить настройки двигателя.
    2. Пропустить шаг и перейти к настройкам резольвера.
  № 1

Введите числовой параметр отделяя десятичный знак запятой.
Чтобы оставить параметр без изменения, ничего не вводите, сразу нажмите Enter.

    Номинальный ток                2,2 А                 = 1,23
    Номинальная частота вращения   3000 об/мин           = 2000
    Номинальная частота двигателя  50 Гц                 = 167
    Выходная частота               132 Гц                = 166,7
    Макс. частота вращения         1980 об/мин           = 2000
    Номинальный момент             7,2 Н*м               = 3,6
    Сопротивление статора (Rs)     1,255 Ом              = 29,4
    Индуктивность (Ld)             10 мГн                = 131
    Число полюсов двигателя        8                     = 10
    BEMF на 1000 об/мин            99                    = 179
    Ограничение по току            160 %                 = 200
    Ограничение момента, мотор     160 %                 = 200
    Ограничение момента, генератор 100 %                 = 200

    1. Загрузить в ПЧ заданные выше параметры, перейти к настройкам резольвера.
    2. Не загружать параметры, перейти к настройкам резольвера.
    3. Задать параметры еще раз.
  № 1

Загрузка параметров................ОК

Параметры двигателя заданы.

 *** Шаг 3: Задание настроек резольвера

Текущие параметры резольвера:
    Число полюсов резольвера = 2
    Напряжение питания       = 7 В
    Частота возбуждения      = 10 кГц
    Коэф. трансформации      = 0,5

    1. Изменить настройки резольвера.
    2. Пропустить шаг и перейти к установке смещения резольвера.
  № 1

Введите числовой параметр отделяя десятичный знак запятой.
Чтобы оставить параметр без изменения, ничего не вводите, сразу нажмите Enter.

    Число полюсов резольвера 2           =
    Напряжение питания       7 В         = 8
    Выходная частота         10 кГц      =
    Коэф. трансформации      0,5         =

    1. Загрузить в ПЧ заданные выше параметры, перейти к установке нуля.
    2. Не загружать параметры, перейти к установке нуля.
    3. Задать параметры еще раз.
  № 1

Загрузка параметров........ОК

Параметры резольвера заданы.
Резольвер включен.

 *** Шаг 4: Настройка смещения резольвера

ВНИМАНИЕ! Для выполнения этого шага, программа сбросит функции цифровых входов.
          Нажмите на LCP клавишу `Auto On`.

    1. Продолжить автоустановку смещения резольвера...
    2. Перейти к пробному пуску двигателя.
  № 1

Загрузка параметров............ОК
Автоустановка нуля угла двигателя...3..2..1...OК
Загрузка параметров................OK

Ноль угла двигателя выставлен автоматически.

ВНИМАНИЕ! Не трогайте ротор двигателя до завершения шага.

Автонастройка смещения...10..9..8..7..6..5..4..3..2..1...OK
Угол за 10 секунд (мин..макс) = 49229..49274 инкр.

Смещение резольвера выставлено автоматически.
Двигатель готов для пробного пуска.

 *** Шаг 5: Пробный пуск двигателя

    1. Старт двигателя.
    2. Задать параметры PID-регулятора.
    3. Завершить работу программы.
  № 2

Введите числовой параметр отделяя десятичный знак запятой.
Чтобы оставить параметр без изменения, ничего не вводите, сразу нажмите Enter.

    PID пропорциональная (Kp) 0,015              = 0,01
    PID интегральная     (Ki) 200 мс             = 50
    PID дифференциальная      0 мс               = 1

    1. Загрузить в ПЧ заданные выше параметры.
    2. Не загружать параметры, вернуться к пробному пуску.
    3. Задать параметры еще раз.
  № 1

Загрузка параметров......ОК

 *** Шаг 5: Пробный пуск двигателя

    1. Старт двигателя.
    2. Задать параметры PID-регулятора.
    3. Завершить работу программы.
  № 1

ВНИМАНИЕ! Если во время пуска двигателя происходит ошибка...
          или двигатель ведет себя неадекватно - попробуйте:
       а. Выключить силовое питание и поменять полярность sin/cos резольвера.
       б. Задать более мягкие параметры PID-регулятора.

Введите частоту вращения двигателя в об/мин.
Введите 0 или пустую строку, чтобы остановить двигатель.

      Частота вращения 0 об/мин          = 10
      Частота вращения 0 об/мин          = 20
      Частота вращения 0 об/мин          = 50
      Частота вращения 0 об/мин          = 100
      Частота вращения 0 об/мин          = 200
      Частота вращения 0 об/мин          = 0

 *** Шаг 5: Пробный пуск двигателя

    1. Старт двигателя.
    2. Задать параметры PID-регулятора.
    3. Завершить работу программы.
  № 3