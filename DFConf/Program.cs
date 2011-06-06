using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Threading;

namespace DFConf
{
    class Program
    {
        static void Main(string[] args)
        {
            Assembly execAsm = Assembly.GetExecutingAssembly();
			Echo.WL("DFConf [" + execAsm.GetName().Version.ToString() + "]", ConsoleColor.White);
			Echo.WL("Конфигуратор синхронного привода с резольвером: Danfoss FC302 + MCB-103", ConsoleColor.White);
			Echo.WL("(c) 2011 Николай Воронин");
			Echo.WL();
			Echo.WL("Краткая справка:", ConsoleColor.Yellow);
			Echo.WL("   ПК  - персональный компьютер.");
			Echo.WL("   ПЧ  - частотный привод (инвертер).");
			Echo.WL("   LCP - панель оператора ПЧ.");
			Echo.WL("    №  - выберите пункт меню и введите его номер.", ConsoleColor.Cyan);
			Echo.WL("    =  - введите числовой параметр.");
			Echo.WL("    >  - нажмите любую клавишу для продолжения.");
			Echo.WL("    *  - для продолжения, выполните требуемое действие.", ConsoleColor.Magenta);
			Echo.WL();
			Echo.Warn("ВНИМАНИЕ! Отсоедините механическую передачу от синхронного двигателя.");
			Echo.Warn("          Нажмите на LCP клавишу `Auto On`.");
			Echo.WL();

            string[] portNames = null;            
            string portName = null;
			int step = 0;
			bool isRun = true;
			int menuChoise = 0;

			//PID & switch
			double pidKp = 0, pidKi = 0, pidDiff = 0;
			// Motor
			double current = 0, nomSpeed = 0, nomFreq = 0, outFreq = 0, speedHiLim = 0, rateTorq = 0, rs = 0, ld = 0, motPoles = 0, bemf = 0, currentLim = 0, torqLimMotor = 0, torqLimGen = 0;
			// Resolver
			double resPoles = 0, inpV = 0, inpFreq = 0, transf = 0;
			int resShift = 0, resPos = 0;
			// Error and Warnings
			int lastErrorTime = 0;
			int errFlag0 = 0, errFlag1 = 0, errFlag2 = 0, errFlag3 = 0;
			int faultLog = 0;
			//reference
			double reference = 0;

			do
			{
				switch (step)
				{
					case 0:		//////////////////////////////////////////////////////////
						Echo.T("Шаг 0: Подключение ПЧ к ПК");
						Echo.WL();
						portNames = FC.PortNames;
						int portsCount = portNames.Length;
						if (portsCount < 1)
						{
							Echo.Warn("ОШИБКА! ПЧ не подключен к ПК или не обнаружены COM-порты.");
							Echo.WaitKeyCursor();
						}
						else
						{
							Echo.WL("Выберите COM-порт ПК для подключения:", ConsoleColor.Yellow);
							string[] menuPorts = new string[portsCount];
							for (int i = 0; i < portsCount; i++)
							{
								menuPorts[i] = string.Format("{0}: {1}",
									i + 1,
									portNames[i]
									);
							}
							menuChoise = Echo.WaitMenuChoise(menuPorts);
							if (menuChoise > 0 && menuChoise <= portsCount)
							{
								portName = portNames[menuChoise - 1];
								FC.I.Open(portName);
								step = 2;
							}
						}
						break;
					case 2:
						string fcType		= FC.I.GetStringValue(1540, 0, 1);
						string power		= FC.I.GetStringValue(1541, 0, 1);
						string voltage		= FC.I.GetStringValue(1542, 0, 1);
						string swVersion	= FC.I.GetStringValue(1543, 0, 1);
						string lcpType		= FC.I.GetStringValue(1548, 0, 1);
						string optA			= FC.I.GetStringValue(1560, 0, 1);
						string optB			= FC.I.GetStringValue(1560, 1, 1);
						lastErrorTime		= FC.I.GetParameterValue(1532, 0, 1);
						errFlag0			= FC.I.GetParameterValue(1690, 1);
						errFlag1			= FC.I.GetParameterValue(1691, 1);
						errFlag2			= FC.I.GetParameterValue(1692, 1);
						errFlag3			= FC.I.GetParameterValue(1693, 1);

						Echo.WL("  Частотный привод:", ConsoleColor.Yellow);
						Echo.WL("    Тип       : " + fcType);
						Echo.WL("    Мощность  : " + power);
						Echo.WL("    Питание   : " + voltage);
						Echo.WL("    Версия ПО : " + swVersion);
						Echo.WL("    Тип LCP   : " + lcpType);
						Echo.WL();
						Echo.WL("  Опциональные платы:", ConsoleColor.Yellow);
						Echo.WL("    " + optA);
						Echo.WL("    " + optB);
						Echo.WL();

						if (errFlag0 != 0 ||
							errFlag1 != 0 ||
							errFlag2 != 0 ||
							errFlag3 != 0)
						{
							step = 5;
							continue;
						}

						if (lcpType.ToLower().Contains("none"))
						{
							Echo.Warn("ВНИМАНИЕ! Подключите LCP панель оператора!");
						}

						if (!(optA.ToLower().Contains("mcb103") ||
							optB.ToLower().Contains("mcb103")))
						{
							Echo.Warn("ВНИМАНИЕ! Опциональная плата резольвера MCB-103 не найдена!");
						}

						step = 10;
						break;
					case 5:
						faultLog = FC.I.GetParameterValue(1530, 1);
						Echo.Warn("ВНИМАНИЕ! Ошибка ПЧ! Код ошибки = " + faultLog);
						Echo.WL("  * Нажмите клавишу Reset на LCP...", ConsoleColor.Magenta);
						Echo.WL();
						step = 6;
						break;
					case 6:
						errFlag0			= FC.I.GetParameterValue(1690, 1);
						errFlag1			= FC.I.GetParameterValue(1691, 1);
						errFlag2			= FC.I.GetParameterValue(1692, 1);
						errFlag3			= FC.I.GetParameterValue(1693, 1);
						if (errFlag0 == 0 &&
							errFlag1 == 0 &&
							errFlag2 == 0 &&
							errFlag3 == 0)
						{
							step = 10;
							continue;
						}
						break;

					case 10:	//////////////////////////////////////////////////////////
						//start/stop tests
						//FC.I.Start(10, 1);
						//Echo.WaitKeyCursor();
						//FC.I.Stop(1);
						//step = 10000;
						//continue;

						Echo.WL();
						Echo.T("Шаг 1: Сброс ПЧ в заводские настройки");
						Echo.WL();
						menuChoise = Echo.WaitMenuChoise(new string[] {
							"1. Сбросить все настройки ПЧ в заводские настройки.",
							"2. Сбросить в заводские только настройки двигателя.",
							"3. Пропустить шаг и перейти к настройкам двигателя.",
							"4. Перейти к пробному пуску двигателя."
							});
						switch (menuChoise)
						{
							case 1: step = 20; break;
							case 2: step = 29; break;
							case 3: step = 30; break;
							case 4: step = 70; break;
						}
						break;

					case 20:	//////////////////////////////////////////////////////////
						FC.I.SetParameterValue(1422, 2, 1);
						Thread.Sleep(100);
						Echo.WL("Выполните действия перечисленные ниже.", ConsoleColor.Yellow);
						Echo.WL("После выполнения требуемых действий, нажмите любую клавишу.", ConsoleColor.Yellow);
						Echo.WL("Если требуемое действие невозможно, перезапустите программу и повторите.", ConsoleColor.Yellow);
						Echo.WL();
						Echo.WL("  * Отключите силовое питание ПЧ.", ConsoleColor.Magenta);
						step = 22;
						break;
					case 22: // ждем выключение силового питания, следим за ошибкой связи: ошибка - выключено
						try
						{
							FC.I.GetParameterValue(1690, 1);
						}
						catch
						{
							Echo.WL("    Силовое питание выключено.");
							Echo.WL("  * Подождите не менее 10 секунд и включите силовое питание.", ConsoleColor.Magenta);
							step = 24;
						}
						break;
					case 24: // ждем включение силового питания, следим за ошибкой связи: нет ошибки - включено
						try
						{
							errFlag0 = FC.I.GetParameterValue(1690, 1);
							errFlag1 = FC.I.GetParameterValue(1691, 1);
							errFlag2 = FC.I.GetParameterValue(1692, 1);
							errFlag3 = FC.I.GetParameterValue(1693, 1);
							if (errFlag0 != 0 ||
								errFlag1 != 0 ||
								errFlag2 != 0 ||
								errFlag3 != 0)
							{
								faultLog = FC.I.GetParameterValue(1530, 1);
								if (faultLog == 80)
								{
									Echo.WL("    Силовое питание включено.");
									step = 26;
								}
							}
						}
						catch { }
						break;
					case 26: // проверка флага ошибки - прошла ли инициализация
						Echo.WL("  * Нажмите клавишу Reset на LCP.", ConsoleColor.Magenta);
						step = 28;
						break;
					case 28: // ждем обнуления флага ошибок
						errFlag0 = FC.I.GetParameterValue(1690, 1);
						errFlag1 = FC.I.GetParameterValue(1691, 1);
						errFlag2 = FC.I.GetParameterValue(1692, 1);
						errFlag3 = FC.I.GetParameterValue(1693, 1);
						if (errFlag0 == 0 &&
							errFlag1 == 0 &&
							errFlag2 == 0 &&
							errFlag3 == 0)
						{
							Echo.WL("    Ошибка сброшена.");
							Echo.WL();
							step = 29;
						}
						break;
					case 29:
						Echo.W("Загрузка параметров...");
						FC.I.SetParameterValue(1422, 0, 1);
						Thread.Sleep(100); Echo.W(".");
						FC.I.SetParameterValue(110, 1, 1);	// PM, non salient
						for (int i = 0; i < 8; i++) { Thread.Sleep(250); Echo.W("."); }
						FC.I.SetParameterValue(102, 3, 1);	// set to MCB 103
						Thread.Sleep(100); Echo.WL(".ОК");
						Echo.WL();
						Echo.WL("ПЧ в заводских настройках.", ConsoleColor.Green);
						Echo.WL();
						step = 30;
						break;

					case 30:	//////////////////////////////////////////////////////////
						Echo.T("Шаг 2: Задание настроек двигателя");
						Echo.WL();
						
						current			= FC.I.GetParameterValue(124, 1) / 100.0;
						nomFreq			= FC.I.GetParameterValue(123, 1);
						nomSpeed		= FC.I.GetParameterValue(125, 1);
						outFreq			= FC.I.GetParameterValue(419, 1) / 10.0;
						speedHiLim		= FC.I.GetParameterValue(413, 1);
						rateTorq		= FC.I.GetParameterValue(126, 1) / 10.0;
						rs				= FC.I.GetParameterValue(130, 1) / 10000.0;
						ld				= FC.I.GetParameterValue(137, 1) / 10.0;
						motPoles		= FC.I.GetParameterValue(139, 1);
						bemf			= FC.I.GetParameterValue(140, 1);
						currentLim		= FC.I.GetParameterValue(418, 1) / 10.0;
						torqLimMotor	= FC.I.GetParameterValue(416, 1) / 10.0;
						torqLimGen		= FC.I.GetParameterValue(417, 1) / 10.0;

						Echo.WL("Текущие параметры двигателя:", ConsoleColor.Yellow);
						Echo.WL("    Номинальный ток                = " + current + " А");
						Echo.WL("    Номинальная частота вращения   = " + nomSpeed + " об/мин");
						Echo.WL("    Номинальная частота двигателя  = " + nomFreq + " Гц");
						Echo.WL("    Выходная частота               = " + outFreq + " Гц");
						Echo.WL("    Макс. частота вращения         = " + speedHiLim + " об/мин");
						Echo.WL("    Номинальный момент             = " + rateTorq + " Н*м");
						Echo.WL("    Сопротивление статора (Rs)     = " + rs + " Ом");
						Echo.WL("    Индуктивность (Ld)             = " + ld + " мГн");
						Echo.WL("    Число полюсов двигателя        = " + motPoles);
						Echo.WL("    BEMF на 1000 об/мин            = " + bemf);
						Echo.WL("    Ограничение по току            = " + currentLim + " %");
						Echo.WL("    Ограничение момента, мотор     = " + torqLimMotor + " %");
						Echo.WL("    Ограничение момента, генератор = " + torqLimGen + " %");

						Echo.WL();

						menuChoise = Echo.WaitMenuChoise(new string[] {
							"1. Изменить настройки двигателя.",
							"2. Пропустить шаг и перейти к настройкам резольвера."
							});
						if (menuChoise == 1)
						{
							step = 33;
						}
						else
						{
							step = 40;
						}
						break;
					case 33:
						Echo.WL("Введите числовой параметр отделяя десятичный знак запятой.", ConsoleColor.Yellow);
						Echo.WL("Чтобы оставить параметр без изменения, ничего не вводите, сразу нажмите Enter.", ConsoleColor.Yellow);
						Echo.WL();
						current			= Echo.WaitValueEnter("Номинальный ток               ", current,		"А");
						nomSpeed		= Echo.WaitValueEnter("Номинальная частота вращения  ", nomSpeed,		"об/мин");
						nomFreq			= Echo.WaitValueEnter("Номинальная частота двигателя ", nomFreq,		"Гц");
						outFreq			= Echo.WaitValueEnter("Выходная частота              ", outFreq,		"Гц");
						speedHiLim		= Echo.WaitValueEnter("Макс. частота вращения        ", speedHiLim,		"об/мин");
						rateTorq		= Echo.WaitValueEnter("Номинальный момент            ", rateTorq,		"Н*м");
						rs				= Echo.WaitValueEnter("Сопротивление статора (Rs)    ", rs,				"Ом");
						ld				= Echo.WaitValueEnter("Индуктивность (Ld)            ", ld,				"мГн");
						motPoles		= Echo.WaitValueEnter("Число полюсов двигателя       ", motPoles,		"");
						bemf			= Echo.WaitValueEnter("BEMF на 1000 об/мин           ", bemf,			"");
						currentLim		= Echo.WaitValueEnter("Ограничение по току           ", currentLim,		"%");
						torqLimMotor	= Echo.WaitValueEnter("Ограничение момента, мотор    ", torqLimMotor,	"%");
						torqLimGen		= Echo.WaitValueEnter("Ограничение момента, генератор", torqLimGen,		"%");
						Echo.WL();
						menuChoise = Echo.WaitMenuChoise(new string[] {
							"1. Загрузить в ПЧ заданные выше параметры, перейти к настройкам резольвера.",
							"2. Не загружать параметры, перейти к настройкам резольвера.",
							"3. Задать параметры еще раз."
							});
						switch (menuChoise)
						{
							case 1:	step = 36; break;
							case 2: step = 40; break;
							case 3: step = 33; break;
						}
						break;
					case 36:
						Echo.W("Загрузка параметров...");
						FC.I.SetParameterValue(124, (int)(current * 100.0), 1);
						Thread.Sleep(100); Echo.W(".");
						FC.I.SetParameterValue(123, (int)(nomFreq), 1);
						Thread.Sleep(100); Echo.W(".");
						FC.I.SetParameterValue(125, (int)(nomSpeed), 1);
						Thread.Sleep(100); Echo.W(".");
						FC.I.SetParameterValue(419, (int)(outFreq * 10.0), 1);
						Thread.Sleep(100); Echo.W(".");
						FC.I.SetParameterValue(413, (int)(speedHiLim), 1);
						Thread.Sleep(100); Echo.W(".");
						FC.I.SetParameterValue(126, (int)(rateTorq * 10.0), 1);
						Thread.Sleep(100); Echo.W(".");
						FC.I.SetParameterValue(130, (int)(rs * 10000.0), 1);
						Thread.Sleep(100); Echo.W(".");
						FC.I.SetParameterValue(137, (int)(ld * 10.0), 1);
						Thread.Sleep(100); Echo.W(".");
						FC.I.SetParameterValue(139, (int)(motPoles), 1);
						Thread.Sleep(100); Echo.W(".");
						FC.I.SetParameterValue(140, (int)(bemf), 1);
						Thread.Sleep(100); Echo.W(".");
						FC.I.SetParameterValue(418, (int)(currentLim * 10.0), 1);
						Thread.Sleep(100); Echo.W(".");
						FC.I.SetParameterValue(416, (int)(torqLimMotor * 10.0), 1);
						Thread.Sleep(100); Echo.W(".");
						FC.I.SetParameterValue(417, (int)(torqLimGen * 10.0), 1);
						Thread.Sleep(100); Echo.W(".ОК");
						Echo.WL();
						Echo.WL();
						Echo.WL("Параметры двигателя заданы.", ConsoleColor.Green);
						Echo.WL();
						step = 40;
						break;

					case 40:	//////////////////////////////////////////////////////////
						Echo.T("Шаг 3: Задание настроек резольвера");
						Echo.WL();

						resPoles	= FC.I.GetParameterValue(1750, 1);
						inpV		= FC.I.GetParameterValue(1751, 1) / 10.0;
						inpFreq		= FC.I.GetParameterValue(1752, 1) / 10.0;
						transf		= FC.I.GetParameterValue(1753, 1) / 10.0;

						Echo.WL("Текущие параметры резольвера:", ConsoleColor.Yellow);
						Echo.WL("    Число полюсов резольвера = " + resPoles);
						Echo.WL("    Напряжение питания       = " + inpV + " В");
						Echo.WL("    Частота возбуждения      = " + inpFreq + " кГц");
						Echo.WL("    Коэф. трансформации      = " + transf);

						Echo.WL();

						menuChoise = Echo.WaitMenuChoise(new string[] {
							"1. Изменить настройки резольвера.",
							"2. Пропустить шаг и перейти к установке смещения резольвера."
							});
						if (menuChoise == 1)
						{
							step = 43;
						}
						else
						{
							step = 50;
						}
						break;
					case 43:
						Echo.WL("Введите числовой параметр отделяя десятичный знак запятой.", ConsoleColor.Yellow);
						Echo.WL("Чтобы оставить параметр без изменения, ничего не вводите, сразу нажмите Enter.", ConsoleColor.Yellow);
						Echo.WL();
						resPoles	= Echo.WaitValueEnter("Число полюсов резольвера", resPoles, "");
						inpV		= Echo.WaitValueEnter("Напряжение питания      ", inpV, "В");
						inpFreq		= Echo.WaitValueEnter("Выходная частота        ", inpFreq, "кГц");
						transf		= Echo.WaitValueEnter("Коэф. трансформации     ", transf, "");
						Echo.WL();
						menuChoise = Echo.WaitMenuChoise(new string[] {
							"1. Загрузить в ПЧ заданные выше параметры, перейти к установке нуля.",
							"2. Не загружать параметры, перейти к установке нуля.",
							"3. Задать параметры еще раз."
							});
						switch (menuChoise)
						{
							case 1: step = 46; break;
							case 2: step = 50; break;
							case 3: step = 43; break;
						}
						break;
					case 46:
						Echo.W("Загрузка параметров...");
						FC.I.SetParameterValue(1759, 0, 1);	// switch resolver off
						Thread.Sleep(100);
						FC.I.SetParameterValue(1750, (int)(resPoles), 1);
						Thread.Sleep(100); Echo.W(".");
						FC.I.SetParameterValue(1751, (int)(inpV * 10.0), 1);
						Thread.Sleep(100); Echo.W(".");
						FC.I.SetParameterValue(1752, (int)(inpFreq * 10.0), 1);
						Thread.Sleep(100); Echo.W(".");
						FC.I.SetParameterValue(1753, (int)(transf * 10.0), 1);
						Thread.Sleep(100); Echo.W(".");
						FC.I.SetParameterValue(1759, 1, 1);	// switch resolver on
						Thread.Sleep(100); Echo.W(".ОК");
						Echo.WL();
						Echo.WL();

						Echo.WL("Параметры резольвера заданы.", ConsoleColor.Green);
						Echo.WL("Резольвер включен.", ConsoleColor.Green);
						Echo.WL();
						step = 50;
						break;

					case 50:	//////////////////////////////////////////////////////////
						Echo.T("Шаг 4: Настройка смещения резольвера");
						Echo.WL();
						Echo.Warn("ВНИМАНИЕ! Для выполнения этого шага, программа сбросит функции цифровых входов.");
						Echo.Warn("          Нажмите на LCP клавишу `Auto On`.");
						Echo.WL();
						    menuChoise = Echo.WaitMenuChoise(new string[] {
						        "1. Продолжить автоустановку смещения резольвера...",
						        "2. Перейти к пробному пуску двигателя.",
						        });
						    switch (menuChoise)
						    {
						        case 1: step = 51; break;
						        case 2: step = 70; break;
						    }
							break;
					case 51:
						Echo.W("Загрузка параметров...");
						// reset digital inputs
						FC.I.SetParameterValue(511, 0, 1);
						Thread.Sleep(100); Echo.W(".");
						FC.I.SetParameterValue(512, 0, 1);
						Thread.Sleep(100); Echo.W(".");
						FC.I.SetParameterValue(513, 0, 1);
						Thread.Sleep(100); Echo.W(".");

						// detach resolver
						FC.I.SetParameterValue(1759, 0, 1);		// switch resolver off
						Thread.Sleep(100); Echo.W(".");
						FC.I.SetParameterValue(102, 1, 1);		// set to 24V encoder
						Thread.Sleep(100); Echo.W(".");

						// set LCP display parameters
						FC.I.SetParameterValue(21, 1620, 1);	// line1.2: motor angle
						Thread.Sleep(100); Echo.W(".");
						FC.I.SetParameterValue(22, 1614, 1);	// line1.3: motor current
						Thread.Sleep(100); Echo.W(".");

						// start function and delay
						FC.I.SetParameterValue(171, 100, 1);	// start delay 10.0s
						Thread.Sleep(100); Echo.W(".");
						FC.I.SetParameterValue(172, 0, 1);		// start function = DC Hold after start delay time
						Thread.Sleep(100); Echo.W(".ОК");
						Echo.WL();

						step = 52;
						break;
					case 52:
						Echo.W("Автоустановка нуля угла двигателя...");
						FC.I.Start(1, 1);
						Thread.Sleep(1000);
						Echo.W("3..");
						Thread.Sleep(1000);
						Echo.W("2..");
						Thread.Sleep(1000);
						Echo.W("1..");
						Thread.Sleep(1000);
						Echo.W(".OК");
						FC.I.Stop(1);
						Echo.WL();
						
						step = 53;
						break;
					case 53:
						Echo.W("Загрузка параметров...");
						// attach resolver
						FC.I.SetParameterValue(102, 3, 1);	// set to MCB 103
						Thread.Sleep(100); Echo.W(".");
						// start function and delay
						FC.I.SetParameterValue(171, 0, 1);	// start delay 0.0s
						Thread.Sleep(100); Echo.W(".");
						FC.I.SetParameterValue(172, 5, 1);	// start function = VVC+/Flux clockwise
						Thread.Sleep(100); Echo.W(".");

						// resolver parameters
						FC.I.SetParameterValue(141, 0, 1);	// motor angle offset = 0;
						Thread.Sleep(100); Echo.W(".");
						FC.I.SetParameterValue(1759, 1, 1);	// switch resolver on
						for (int i = 0; i < 8; i++) { Thread.Sleep(250); Echo.W("."); }
						Echo.WL(".OK");

						Echo.WL();
						Echo.WL("Ноль угла двигателя выставлен автоматически.", ConsoleColor.Green);
						Echo.WL();
						Echo.Warn("ВНИМАНИЕ! Не трогайте ротор двигателя до завершения шага.");
						Echo.WL();
						step = 60;
						break;
					case 60:		////////
						Echo.W("Автонастройка смещения...");
						int cnt = 0, cntTen = 0;
						int min = 65536, max = 0;
						for (int i = 0; i < 100; i++)
						{
							resPos = FC.I.GetParameterValue(1620, 1);
							min = Math.Min(resPos, min);
							max = Math.Max(resPos, max);
							
							Thread.Sleep(100);
							if (++cnt == 10)
							{
								cntTen++;
								cnt = 0;
								Echo.W(11 - cntTen + "..");
							}
						}

						if (min < 32768)
						{
							resShift = 65536 - min;
						}
						else
						{
							resShift = 65536 - max;
						}
						
						Echo.WL(".OK");
						Echo.WL(string.Format(
							"Угол за 10 секунд (мин..макс) = {0}..{1} инкр.",
							min,
							max
							));
						step = 63;
						break;
					case 63:
						FC.I.SetParameterValue(141, resShift, 1);		// motor angle offset
						Thread.Sleep(100);
						Echo.WL();
						Echo.WL("Смещение резольвера выставлено автоматически.", ConsoleColor.Green);
						Echo.WL("Двигатель готов для пробного пуска.", ConsoleColor.Green);
						Echo.WL();

						step = 70;
						break;

					case 70:	//////////////////////////////////////////////////////////
						Echo.T("Шаг 5: Пробный пуск двигателя");
						Echo.WL();
						menuChoise = Echo.WaitMenuChoise(new string[] {
						    "1. Старт двигателя.",
						    "2. Задать параметры PID-регулятора.",
						    "3. Завершить работу программы.",
						    });
						switch (menuChoise)
						{
						    case 1: step = 80; break;
						    case 2: step = 90; break;
							case 3: step = 9999; break;
						}
						break;
					case 80:		////////
						Echo.Warn("ВНИМАНИЕ! Если во время пуска двигателя происходит ошибка...");
						Echo.Warn("          или двигатель ведет себя неадекватно - попробуйте:");
						Echo.Warn("       а. Выключить силовое питание и поменять полярность sin/cos резольвера.");
						Echo.Warn("       б. Задать более мягкие параметры PID-регулятора.");
						Echo.WL();
						Echo.WL("Введите частоту вращения двигателя в об/мин.", ConsoleColor.Yellow);
						Echo.WL("Введите 0 или пустую строку, чтобы остановить двигатель.", ConsoleColor.Yellow);
						Echo.WL();
						nomSpeed = FC.I.GetParameterValue(125, 1);
						FC.I.SetParameterValue(303, (int)(nomSpeed * 1000.0), 1);
						Thread.Sleep(100);
						step = 82;
						break;
					case 82:
						reference = Echo.WaitValueEnter("  Частота вращения", 0.0, "об/мин");
						if (reference == 0)
						{
							FC.I.Stop(1);
							step = 70;
							Echo.WL();
						}
						else
						{
							FC.I.Start((int)(100.0 * reference / (double)nomSpeed), 1);
						}
						break;
					case 90:		////////
						pidKp	= FC.I.GetParameterValue(702, 1) / 1000.0;
						pidKi	= FC.I.GetParameterValue(703, 1) / 10.0;
						pidDiff	= FC.I.GetParameterValue(704, 1) / 10.0;

						Echo.WL("Введите числовой параметр отделяя десятичный знак запятой.", ConsoleColor.Yellow);
						Echo.WL("Чтобы оставить параметр без изменения, ничего не вводите, сразу нажмите Enter.", ConsoleColor.Yellow);
						Echo.WL();
						pidKp	= Echo.WaitValueEnter("PID пропорциональная (Kp)", pidKp,	"");
						pidKi	= Echo.WaitValueEnter("PID интегральная     (Ki)", pidKi,	"мс");
						pidDiff	= Echo.WaitValueEnter("PID дифференциальная     ", pidDiff,	"мс");
						Echo.WL();

						menuChoise = Echo.WaitMenuChoise(new string[] {
							"1. Загрузить в ПЧ заданные выше параметры.",
							"2. Не загружать параметры, вернуться к пробному пуску.",
							"3. Задать параметры еще раз."
							});
						switch (menuChoise)
						{
							case 1: step = 95; break;
							case 2: step = 70; break;
							case 3: step = 90; break;
						}
						break;
					case 95:
						Echo.W("Загрузка параметров...");
						FC.I.SetParameterValue(702, (int)(pidKp * 1000.0), 1);
						Thread.Sleep(100); Echo.W(".");
						FC.I.SetParameterValue(703, (int)(pidKi * 10.0), 1);
						Thread.Sleep(100); Echo.W(".");
						FC.I.SetParameterValue(704, (int)(pidDiff * 10.0), 1);
						Thread.Sleep(100); Echo.WL(".ОК");
						Echo.WL();
						step = 70; 
						break;

					default:	//////////////////////////////////////////////////////////
						isRun = false;
						break;
				} // switch(step)
			} while (isRun);

			FC.I.Close();
		}
    }
}
