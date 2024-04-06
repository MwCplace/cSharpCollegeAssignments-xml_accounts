using System.Xml;
using System.Xml.Linq;
using static System.Console;

class ConsoleDecoration
{
    public static void ErrorMsg(string err)
    {
        ForegroundColor = ConsoleColor.Red;
        Write(err);
        ForegroundColor = ConsoleColor.Gray;
    }

    public static void WarningMsg(string err)
    {
        ForegroundColor = ConsoleColor.DarkYellow;
        Write(err);
        ForegroundColor = ConsoleColor.Gray;
    }

    public static void ResultMsg(string text)
    {
        ForegroundColor = ConsoleColor.Green;
        Write(text);
        ForegroundColor = ConsoleColor.Gray;
    }

    public static void TitleMsg(string text)
    {
        ForegroundColor = ConsoleColor.Blue;
        Write(text);
        ForegroundColor = ConsoleColor.Gray;
    }
}

class ConsoleHandling : ConsoleDecoration
{
    public static string tryAgainText = ". Попробуйте еще раз\n";
    public static string[] rules = {
        "Во время любого ввода вы можете ввести число \"0\" и программа завершится",
        "Во время любого ввода вы можете ввести слово \"п\" и будут показаны правила программы",
        "При завершении ввода данных нажимайте клавишу Enter",
        "При сообщении \"Введите: да/нет\" если вы согласны, введите слово \"да\", иначе введите любое другое слово или символ",
        "Программа не может хранить более 1-ого текста, имейте это ввиду"
    };

    // other
    // проверяет на ключевые слова и позволяет избежать ввода
    public static bool CheckForKeywords(string text)
    {
        if (text == "0")
        {
            Environment.Exit(0);
        }
        else if (text == "м")
        {
            return true;
        }
        else if (text == "п")
        {
            TitleMsg("\nПРАВИЛА\n");
            foreach (string i in rules) // вывести все правила в консоль
            {
                WriteLine("\t" + i);
            }
            return false;
        }
        else if (text == " ")
        {
            ErrorMsg("Такой опции нет!" + tryAgainText + "\n\n");
            return true;
        }
        else
        {
            return false;
        }
        return false;
    }

    public static int GetOption()
    {
        string str = ReadLine();

        if (CheckForKeywords(str)) // ЗНАЮ ПОЧЕМУ ВЫВОДИТ Введено не число ПРИ ВЫБОРЕ п, НО НЕ ЗНАЮ КАК ИСПРАВИТЬ ТАК, ЧТОБЫ НЕ СОЗДАВАТЬ МНОГО КОДА
        {
            return -1;
        }

        int num = 0;
        try
        {
            num = Convert.ToInt32(str);
        }
        catch (Exception ex) // проверка ввода (если не число, то вывести ошибку и попросить ввести данные снова
        {
            if (ex is FormatException)
            {
                ErrorMsg("Введено не число" + tryAgainText);
                return -1;
            }
        }

        if (num < 0) // проверка ввода (если число не положительное, то вывести ошибку и попросить ввести данные снова
        {
            ErrorMsg("Число должно быть положительным" + tryAgainText);
            return -1;
        }

        return num;
    }

    private static string CheckPath(string path)
    {
        try
        {
            FileInfo fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
            {
                return null;
            }
        }
        catch
        {
            return null;
        }

        return path;
    }

    public static string GetPath(string message)
    {
        WriteLine(message);
        string path = ReadLine();
        if (CheckForKeywords(path))
        {
            return null;
        }

        path = CheckPath(path);
        if (path == null)
        {
            ErrorMsg("Такого файла не существует\n");
            return null;
        }

        return path;
    }
}

class XmlAccounts : ConsoleHandling
{
    public void Copy(string pathFrom, string PathTo)
    {
        string content = File.ReadAllText(pathFrom);
        File.WriteAllText(PathTo, content);
    }

    public int GetNumberOfPasswords(string fileName)
    {
        XDocument doc = XDocument.Load(fileName);
        int count = doc.Descendants().Count(e => e.Name.LocalName == "password"); // doc.Descendants() - получить все узлы. Count - подсчет. лямбда - принимает параметр е и имеет тело в котором условие (e.Name.LocalName == "password") которое возвращает bool. При true в ф-ии Count счетчик +1
        return count;
    }

    // проверка текста файла на соответствие формату логин + пароль
    public static bool FileHasValidFormatting(string path)
    {
        // открыть файл, если не открывается - вернуть ложь, иначе - истина
        try
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
        }
        catch
        {
            return false;
        }
        return true;
    }

    // получить список логинов И паролей
    public static Dictionary<string, string> GetAccounts(string path)
    {
        // записать в массив пары логин + пароль
        // для этого подойдет словарь
        Dictionary<string, string> accounts = new Dictionary<string, string>();
        XmlDocument doc = new XmlDocument();
        doc.Load(path);

        XmlNodeList logins = doc.SelectNodes("//login");
        XmlNodeList passwords = doc.SelectNodes("//password");

        try
        {
            for (int i = 0; i < logins.Count; i++)
            {
                accounts.Add(logins[i].InnerText, passwords[i].InnerText);
            }
        }
        catch
        {
            accounts.Clear();
        }

        return accounts;
    }

    // получить список логинов ИЛИ паролей
    public static string[] GetLoginsOrPasswords(Dictionary<string, string> accounts, bool logins)
    {
        string[] result = new string[accounts.Count]; // длина массива будет в 2 раза меньше, чем длина словаря

        if (logins)
        {
            for (int i = 0; i < accounts.Count; i++)
            {
                result[i] = accounts.Keys.ElementAt(i); // записываем логины
            }
        }
        else
        {
            for (int i = 0; i < accounts.Count; i++)
            {
                result[i] = accounts.Values.ElementAt(i); // записываем пароли
            }
        }

        return result;
    }

    // возвращает индексы дубликатов
    public static HashSet<int> GetDuplicates(string[] lines)
    {
        HashSet<int> duplicatesIndexes = new HashSet<int>(); // только уникальные индексы

        for (int i = 0; i < lines.Length; i++)
        {
            for (int j = i + 1; j < lines.Length; j++)
            {
                if (lines[i] == lines[j]) // если слова равны
                {
                    if (!duplicatesIndexes.Contains(i)) // если таких нет еще нет в списке индексов дубликатов, то
                    {
                        duplicatesIndexes.Add(i); // добавить
                    }
                    if (!duplicatesIndexes.Contains(j))
                    {
                        duplicatesIndexes.Add(j);
                    }
                }
            }
        }

        return duplicatesIndexes;
    }

    // показывает дубликаты
    public static void OutputDuplicates(HashSet<int> duplicatesIndexes, string[] nonDuplicates, string[] duplicates)
    {
        foreach (int line in duplicatesIndexes)
        {
            WriteLine($"___ На строке {line + 1} ___");
            WarningMsg("\t" + nonDuplicates[line] + " ");
            ErrorMsg(duplicates[line] + "\n");
        }
    }
    
    public static bool isDuplicate(string change, string[] lines)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            if (change == lines[i])
            {
                return true;
            }
        }
        return false;
    }

    // заменить текст
    public static void ChangeDuplcate(string path, int index, string change)
    {
        // открыть файл, найти заменяемый элемент и заменить его, сохранить изменения
        XmlDocument doc = new XmlDocument();
        doc.Load(path);

        XmlNodeList passwords = doc.SelectNodes("//password");
        passwords[index].InnerText = change;
        doc.Save(path);
    }

    public static string Join(string[] lines)
    {
        string s = "";
        for (int i = 0; i < lines.Length; i++)
        {
            s += lines[i];

            if (i % 2 == 1 || i < lines.Length - 1) // если индекс нечетный, это значит что слово является паролем и после него должена последовать новая строка. А также не ставить перенос строки после последнего элемента
            {
                s += "\n";
            }
            else // если индекс четный, это значит что слово является логином и после него должен стоять пробел
            {
                s += " ";
            }
        }
        return s;
    }

    public static bool LoginHasDuplicate(string login, string[] logins)
    {
        bool HasDuplicates = false;

        for (int i = 0; i < logins.Length; i++)
        {
            if (login == logins[i]) // если строки равны
            {
                HasDuplicates = true;
                break;
            }
        }

        return HasDuplicates;
    }

    public static void CopyContent(string content, string fileName)
    {
        File.WriteAllText(fileName, content);
    }
    // открыть документ, создать узел, сохранить документ
    public static void addNewAccount(string path, string log, string pass)
    {
        // загружаем документ
        XmlDocument doc = new XmlDocument();
        doc.Load(path);

        // создаем элемент user
        XmlElement userNode = doc.CreateElement("user");

        // создаем элементы log и pass, даем им текст
        XmlElement logNode = doc.CreateElement("login");
        XmlElement passNode = doc.CreateElement("password");
        logNode.InnerText = log;
        passNode.InnerText = pass;
        // прикрепляем элементы log и pass к элементу user
        userNode.AppendChild(logNode);
        userNode.AppendChild(passNode);

        // прикрепляем элемент user после последнего элемента корня документа
        XmlNode last = doc.DocumentElement.LastChild;
        doc.DocumentElement.InsertAfter(userNode, last);

        doc.Save(path);
    }
}

class Start : XmlAccounts
{
    // меню для изменения дубликатов
    public static int DuplicateChangeMenu(string path, HashSet<int> duplicatesIndexes, string[] nonDuplicates, string[] duplicates)
    {
        TitleMsg("\nМЕНЮ ИЗМЕНЕНИЯ ДУБЛИКАТОВ\n");
        OutputDuplicates(duplicatesIndexes, nonDuplicates, duplicates);
        // пока не является действенным индексом - просить ввести другой индекс
        int index;
        string doContinue = "да";
        while (doContinue == "да")
        {
            index = GetOption() - 1; // выбор пары
            if (!duplicatesIndexes.Contains(index) && index != -2)
            {
                ErrorMsg("Данный индекс недоступен. Попробуйте еще раз\n");
            }
            else
            {
                string change = ReadLine(); // замена
                ChangeDuplcate(path, index, change);
                return 0;
            }
        }
        return 1;
    }

    public static void CreateNewAccountMenu(string path)
    {
        TitleMsg("\nМЕНЮ СОЗДАНИЯ НОВОГО АККАУНТА\n");
        string optionStr = "да";
        Dictionary<string, string> accounts;

        while (optionStr == "да")
        {
            // загружаем документ
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            // достаем аккаунты из документа
            accounts = GetAccounts(path);
            // достаем логины
            string[] logins = GetLoginsOrPasswords(accounts, true);

            // проверяем ввод на ключ слова
            Write("Введите логин: ");
            string log = ReadLine();
            if (CheckForKeywords(log))
            {
                break;
            }

            // проверка на то, имеет ли новый логин дубликаты
            while (LoginHasDuplicate(log, logins)) // если да, то вывести ошибку и отправить пользователя в меню
            {
                ErrorMsg("Такой логин уже существует" + tryAgainText + "\n");
                Write("Введите логин: ");
                log = ReadLine();
                if (CheckForKeywords(log))
                {
                    break;
                }
            }

            // проверяем ввод на ключ слова
            Write("Введите пароль: ");
            string pass = ReadLine();
            if (CheckForKeywords(pass))
            {
                break;
            }

            // внедрение/добавление нового элемента, сохранение изменений
            addNewAccount(path, log, pass);
            ResultMsg("Новый аккаунт добавлен!\n");

            // подтвердить, что пользовать хочет продолжать добавлять
            WriteLine("Продолжить? да/нет");
            optionStr = ReadLine();
            CheckForKeywords(optionStr);
            break;
        }
    }

    public static void UserDialogWrap1()
    {
        while (true)
        {
            string path1 = GetPath("\nВведите путь к файлу(откуда скопировать): ");
            if (path1 == null)
            {
                break; // вернуться в меню
            }
            string path2 = GetPath("\nВведите путь к файлу(куда скопировать): ");
            if (path2 == null)
            {
                break; // вернуться в меню
            }
            //string path1 = "XMLFile1.xml";
            //string path2 = "XMLFile2.xml";

            XmlAccounts xmlAccounts = new XmlAccounts();
            xmlAccounts.Copy(path1, path2);
            ResultMsg("Копирование прошло успешно!\n");

            break;
        }
    }

    public static void UserDialogWrap2()
    {
        while (true)
        {
            string path = GetPath("Укажите путь к файлу: ");
            if (path == null)
            {
                break; // вернуться в меню
            }
            //string path = "XMLFile2.xml";

            XmlAccounts xmlAccounts = new XmlAccounts();
            ResultMsg("Кол-во паролей: " + xmlAccounts.GetNumberOfPasswords(path) + "\n");
            break;
        }
    }

    public static void UserDialogWrap3()
    {
        while (true)
        {
            // получить название файла, проверить на существование и отсутствие исключений
            string path = GetPath("Укажите название файла: ");
            if (path == null)
            {
                break; // вернуться в меню
            }
            //string path = "XMLFile2.xml";

            if (path == null)
            {
                ErrorMsg("Файла не существует" + tryAgainText);
                break;
            }

            if (!FileHasValidFormatting(path))
            {
                ErrorMsg("Нарушен формат аккаунтов в файле");
                break;
            }

            // получить массив аккаунтов, если есть дубликаты - попросить переделать файл и попробовать снова 
            Dictionary<string, string>  accountsDic = GetAccounts(path);
            if (accountsDic.Count < 1)
            {
                ErrorMsg("Аккаунты в файле имеют общие логины\nПожалуйста замените их");
                break;
            }

            ResultMsg("Файл принят!\n");

            // получить массив паролей и если есть дубликаты - предложить заменить их
            string[] passwords = GetLoginsOrPasswords(accountsDic, false);
            HashSet<int> duplicatesIndexes = GetDuplicates(passwords);

            string[] logins = GetLoginsOrPasswords(accountsDic, true);
            string[] accounts = new string[accountsDic.Count];
            accountsDic.Values.CopyTo(accounts, 0); // перепись элементов словаря в массив (чтобы не переделывать фукнции DuplicateChangeMenu и CreateNewAccountMenu
            if (duplicatesIndexes.Count > 0)
            {
                foreach (var account in accountsDic)
                {
                    WarningMsg(account.Key + " " + account.Value + "\n");
                }
                ErrorMsg("Аккаунты в файле имеют общие пароли. Заменить? да/нет\n");
                string option = ReadLine();
                if (CheckForKeywords(option))
                {
                    Menu();
                    break;
                }
                while (option == "да")
                {
                    // обновление
                    accountsDic = GetAccounts(path);
                    accountsDic.Values.CopyTo(accounts, 0);
                    accountsDic.Values.CopyTo(accounts, 0);
                    logins = GetLoginsOrPasswords(accountsDic, true);
                    passwords = GetLoginsOrPasswords(accountsDic, false);

                    int code = DuplicateChangeMenu(path, GetDuplicates(passwords), logins, passwords);
                    if (code == 0)
                    {
                        ResultMsg("Изменения сохранены!\n");
                    }
                    else
                    {
                        break;
                    }
                }
            }
            CreateNewAccountMenu(path);
            break;
        }
    }

    public static void Menu()
    {
        string[] exercises =
        {
            "Скопировать содержимое файла в другой файл", // 1
            "Вывести количество паролей", // 2
            "Записать в файл новый логин и пароль" // 3
        };

        TitleMsg("\nГЛАВНОЕ МЕНЮ\n");
        WriteLine("Выберите опцию:");

        WriteLine("\t0 - Завершить программу");
        for (int i = 0; i < exercises.Length; i++)
        {
            WriteLine($"\t{i + 1} - {exercises[i]}");
        }

        Write("Ваш выбор: ");
        int option = GetOption();
        if (option != -1)
        {
            switch (option)
            {
                case 0:
                    Environment.Exit(0);
                    break;
                case 1:
                    UserDialogWrap1();
                    break;
                case 2:
                    UserDialogWrap2();
                    break;
                case 3:
                    UserDialogWrap3();
                    break;
                default:
                    ErrorMsg("Такой опции нет!\n");
                    break;
            }
        }

        Menu();
    }

    public static void Main()
    {
        CheckForKeywords("п");
        Menu();
    }
}