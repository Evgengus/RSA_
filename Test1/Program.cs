using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading;

namespace Test1
{
    class Program
    {

        //константы и объявление ключей
        static BigInteger d;        //секретная экспонента
        static BigInteger n;        //общая часть ключей
        static BigInteger e;        //открытая экспонента
        static int bytelen;         //половина размера ключа
        static int lenforword;      //длина слова в котором зашифрован 1 символ
        static Encoding enc = Encoding.GetEncoding("utf-8");    //кодировка utf-8

        //BigInteger рандои
        static BigInteger getRandom(int length)
        {
            RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();
            byte[] data = new byte[length];
            random.GetBytes(data);
            return new BigInteger(data);
        }

        //чтение из файла
        static string ReadFromFile(string name) {
            string str = "";
            FileStream fs = new FileStream(name, FileMode.OpenOrCreate, FileAccess.Read);
            StreamReader sr = new StreamReader(fs,enc);
            str = sr.ReadToEnd();
            sr.Close();
            fs.Close();
            return str;
        }

        //запись в файл с именем name текста text
        static void WriteInFile(string name, string text)
        {
            File.Delete(name);
            FileStream fStream = new FileStream(name, FileMode.OpenOrCreate);
            StreamWriter streamWriter = new StreamWriter(fStream,enc);
            streamWriter.BaseStream.Seek(0, SeekOrigin.End);
            streamWriter.Write(text);
            streamWriter.Close();
            fStream.Close();
        }

        //запись в файл с именем name текста num text1[i] encrypt to text2[i],len-длина массивов
        static void WriteInFile(string name,BigInteger[] text1,BigInteger[] text2,int len)
        {
            File.Delete(name);
            FileStream fStream = new FileStream(name, FileMode.OpenOrCreate);
            StreamWriter streamWriter = new StreamWriter(fStream, enc);
            streamWriter.BaseStream.Seek(0, SeekOrigin.End);
            for (int i = 0; i < len; i++)
            {
                streamWriter.Write($"{i+".",-4}" + $" num {text1[i], 7}"   + $" encrypt to {text2[i],6}" + "\r\n");
            }
            streamWriter.Close();
            fStream.Close();
        }

        //запись в файл с именем name текста text
        static void WriteInFile(string name, BigInteger text)
        {
            File.Delete(name);
            FileStream fStream = new FileStream(name, FileMode.OpenOrCreate);
            StreamWriter streamWriter = new StreamWriter(fStream,enc);
            streamWriter.BaseStream.Seek(0, SeekOrigin.End);
            streamWriter.Write(text);
            streamWriter.Close();
            fStream.Close();
        }

        //возводит число num в степень pow по модулю mod(Метод повторяющихся возведения в квадрат и умножения)(ВОЗМОЖНО МОЖНО УПРОСТИТЬ)
        static BigInteger BigIntegerPowMod_eff(BigInteger x, BigInteger pow, BigInteger mod)
        {
            BigInteger res = 1;

            int iter = 0;
            int count_one = 0;
            BigInteger pow_buf = pow;   
            while (pow_buf > 0)         //
            {                           //
                iter++;                 //вычисляем сколько разрядов в двоичном представлении
                pow_buf /= 2;           //
            }

            BigInteger[] BinArr = new BigInteger[iter];         //массив 0 и 1
            for (int i = 0; i < iter; i++)                      //
            {                                                   //
                BinArr[i] = pow % 2;                            //вычисляем сколько едениц в двоичном представлении
                if (BinArr[i] == 1) count_one++;                //и записываем 0 и 1 в массив
                pow /= 2;                                       //
            }                                                   //
            BigInteger[] arr = new BigInteger[count_one];           //массив конечных множителей
            BigInteger[] arr_pow = new BigInteger[count_one];       //массив степений,чьи позиции соответсвуют каждому множителю
            BigInteger max_pow = 0;                                 //максимальная степень
            int buf_i = count_one - 1;          //
            for (int i = 0; i < iter; i++)      //
            {                                   //
                if (BinArr[i] == 1)             //записываем какие степени будут в массив
                {                               //
                    if (i > max_pow) max_pow = i;
                    arr_pow[buf_i] = i;         //и сразу забиваем массив значений начальным значением
                    arr[buf_i] = x;             //
                    buf_i--;                    //
                }                               //
            }                                   //

            for (int i = 0; i < max_pow; i++)                       //
                for (int j = 0; j < count_one; j++)                 //
                    if (arr_pow[j] > 0)                             //Если этот множитель еще нужно возводить в квадрат то:
                    {                                               //возводим число в квадрат,если оно первое(у первого максимальная степень)
                        if (j == 0)                                 //
                            arr[j] = (arr[j] * arr[j]) % mod;       //
                        else                                        //если число не первое,то просто копируем предыдущее значение
                            arr[j] = arr[j - 1];                    //
                        arr_pow[j]--;                               //
                    }                                               //
            for (int i = 0; i < count_one; i++) res = (res * arr[i]) % mod;     //перемножаем все множители по модулю
            return res;
        }

        //создание случайного простого числа в пределах от a до b методом перебора делителей(ИСПОЛЬЗУЕТСЯ В СОЗДАНИИ ОТКРЫТОЙ ЭКСПОНЕНТЫ)
        static int PrimesRand(int a, int b)
        {
            int res = 0;
            bool prime = false;
            Random rand_num = new Random(DateTime.Now.Millisecond);
            while (!prime)
            {
                res = rand_num.Next(a, b);
                bool prime_buf = true;
                for (int i = 2; (prime_buf) && ((i * i) <= res); i++)
                    if ((res % i) == 0) prime_buf = false;
                if (prime_buf) prime = true;
            }
            return res;
        }

        //создание случайного простого числа BigInteger (ВОЗОЖНО МОЖНО УПРОСТИТЬ ЛОГИКУ)
        static BigInteger getPrime()
        {
            bool flag_prime = false;                //флаг простоты (true если число простое)
            BigInteger res = 1;                     //результирующая переменная
            while (!flag_prime)                     //
            {
                bool flag_composite = false;                //флаг,true если число составное
                BigInteger n = 0;                           //создаем число
                while (n % 2 == 0) n = getRandom(bytelen);  //ищем нечетное число
                if (n < 0) n = plusforbytes(bytelen) + n;   //решение проблемы с переводом байтов в BigInteger
                BigInteger n_temp = n;                      //временная переменная,которая в итоге превратится в 1
                BigInteger n_minus = n - 1;                 //n-1=2^r*d
                BigInteger r = 0;                           //степень двойки
                BigInteger k = 0;                           //кол-во раундов
                BigInteger d = n_minus;                     //множитель из формулы выше
                while (d % 2 == 0) { r++; d /= 2; }         //считаем r и d
                while (n_temp != 1) { k++; n_temp /= 2; }   //подобие логарифма по основанию 2,log(2)n_temp=k
                for (BigInteger i = 0; i < k; i++)          //
                {
                    BigInteger a = n+1;                         //создаем возможного свидетеля простоты
                    while (a >= n)                              //
                    {
                        a = getRandom(bytelen);                         //ищем а<n, случайным образом
                        if (a < 0) a = plusforbytes(bytelen) + a;       //решение проблемы с переводом байтов в BigInteger
                    }
                    BigInteger x = BigIntegerPowMod_eff(a, d, n);   //x=a^d mod n, эффективный способ
                    if (x != 1 && x != n_minus)                     //Если x не равен 1 или n-1
                    {
                        bool temp_flag = false;                         //флаг продолжения работы,true если число не простое НО и не составное (еще не определили)
                        for (BigInteger j = 0; j < r - 1; j++)          //
                        {
                            x = myPow(x, 2) % n;                            //x^2 mod n
                            if (x == 1) flag_composite = true;              //если х равен 1 то составное
                            if (x == n_minus) temp_flag = true;             //если х равен n-1 то продолжить проверку с другими возможными свидетелями
                        }
                        if (!temp_flag) flag_composite = true;          //если флаг продолжения работы не поднят,то число составное
                    }
                    if (flag_composite) i = k;                      //если число составное то закончим цикл
                    else if (i == k - 1) flag_prime = true;         //если цикл закончился,а предположение о простоте числа не опровергнуто,то число простое
                }
                if (flag_prime) res = n;                        //если число простое,то записать в результат
            }
            return res;
        }

        //перевод BigInteger из 10-ричной с.с. в 16-ричную и запись в строку
        static string DecToHex(BigInteger arr)
        {
            BigInteger BG = arr;                                        //эту строку,наверное,можно будет удалить в дальнейшем
            byte[] BY = BG.ToByteArray();                               //преобразовали BigInteger в массив байтов
            string s = "";                                              //
            s = BitConverter.ToString(BY).Replace("-", "");             //записываем массив байтов в строку в 16 ричной системе,удаляя "-"
            return s;
        }

        //перевод строки из 16-ричной с.с. в 10-ричную и запись в BigInteger
        static BigInteger HexToDec(string hex)
        {
            byte[] BN=new byte[hex.Length/2];                           //Создаем массив вдвое короче длины строки с 16-ричным кодом(2 символа=1 байт) 
            int j = 0;                                                  //
            for (int i = 0; i < hex.Length; i+=2)                       //
            {
                string str = "";                                            //
                str += hex[i];                                              //прибавляем 1 символ
                str += hex[i + 1];                                          //прибавляем 2 символ (2 символа = 1 элемент массива байтов)
                BN[j] = Convert.ToByte(Convert.ToInt32(str, 16));           //конвертируем из 16-ричной в 10-ричную
                j++;                                                        //
            }
            BigInteger BI = new BigInteger(BN);                         //создаем экземпляр BigInteger из массива байтов
            if (BI < 0) BI = plusforbytes(bytelen * 2) + BI;            //проблема при переводе из массива байтов в BigInteger,эта строка приводит все в порядок                                  
            return BI;
        }

        //возведение в степень
        static BigInteger myPow(BigInteger x, BigInteger pow)
        {
            BigInteger res = 1;
            for (int i = 1; i <= pow; i++)
            {
                res *= x;
            }
            return res;
        }

        //вычисляет максимально возможное число для массива байт
        static BigInteger plusforbytes(int x)
        {
            BigInteger res = 0;
            for (int i = 0; i < x; i++)
            {
                res = 256 * myPow(256, i);
            }
            return res;
        }

        //алгоритм создания экспонент
        static void Crypto()
        {
            Console.Write($"{"0%",3}");                 //*просто процент выполнения*
            BigInteger p = 1;                           //
            BigInteger q = 1;                           //
            while (p == 1) p = getPrime();              //создание 1-го простого числа
            Console.CursorLeft -= 3;                    //*просто процент выполнения*
            Console.Write($"{"10%",3}");                //
            while (q == 1 || p==q) q = getPrime();      //создание 2-го простого числа
            Console.CursorLeft -= 3;                    //
            Console.Write($"{"20%",3}");                //*просто процент выполнения*
            n = q * p;                                  //общая часть ключа
            Console.CursorLeft -= 3;                    //
            Console.Write($"{"35%",3}");                //*просто процент выполнения*
            BigInteger eiler = (q - 1) * (p - 1);       //Вычисление функции Эйлера
            Console.CursorLeft -= 3;                    //
            Console.Write($"{"45%",3}");                //*просто процент выполнения*
            e = PrimesRand(3, 65521);                       //рандомно берем открытую экспоненту
            while(eiler % e == 0) e = PrimesRand(3, 65521); //
            Console.CursorLeft -= 3;                    //
            Console.Write($"{"60%",3}");                //*просто процент выполнения*
            BigInteger d_buf = 1;
            BigInteger k = 0;
            while (d_buf % e != 0)                  //вычисляем
            {                                       //секретную
                k++;                                //экспоненту d
                d_buf = (k * eiler + 1);
            }
            d = d_buf/e;                            //записываем результат в глобальную переменную
            Console.CursorLeft -= 3;                //
            Console.Write($"{"75%",3}");            //*просто процент выполнения*
        }

        //шифрование числа num
        static BigInteger Encrypt_num(BigInteger num)
        {
            BigInteger res = BigIntegerPowMod_eff(num, e, n);   //используем метод эффективного возведения num в степень e по модулю n
            return res;                                         //
        }

        //расшифровка числа num
        static BigInteger Decrypt_num(BigInteger num)
        {
            BigInteger res = BigIntegerPowMod_eff(num, d, n);   //используем метод эффективного возведения num в степень d по модулю n
            return res;                                         //
        }


        static void Main(string[] args)
        {
            Console.WriteLine("Choose action");                                         //
            Console.WriteLine("1.Create keys and encrypt text from Alice");             //
            Console.WriteLine("2.Encrypt text from Alice withot create keys");          //меню выбора действия
            Console.WriteLine("3.Decrypt text from Bob");                               //
            Console.WriteLine("0.Exit");                                                //
            string ch;
            ch=Console.ReadLine();
            while (ch != "1" && ch != "2" && ch != "0" && ch != "3")
            {
                Console.CursorTop--;
                Console.WriteLine("Invalid value! Try again");              //реакция на не правильный ввод команды
                ch = Console.ReadLine();
            }
            if (ch == "1")                                              //Шифруем текст из файла Alice
            {
                Console.Write("Enter key length(in bit):");                 //
                string z = "";                                              //
                z += Console.ReadLine();                                    //задаем длину ключа в битах
                while (z == "" || Convert.ToInt32(z)%16!=0 || z.Length>4)   //
                {                                                           //
                    z = "";                                                     //
                    Console.Write("Invalid value! Try again:");                 //защита от дурака
                    z += Console.ReadLine();                                    //
                }                                                           //
                Console.Write("Encrypting:");                               //
                bytelen = Convert.ToInt32(z)/16;                            //устанавливаем глобалную длину массивов байтов(кол-во бит делим на 2 и переводим в байты)
                lenforword = bytelen * 4;                                   //устанавливаем глоабльную длину слова
                Crypto();                                                   //создание ключей
                string e_ = DecToHex(e);                                    //
                string d_ = DecToHex(d);
                string n_ = DecToHex(n);
                while (d_.Length > n_.Length) n_ += "00";
                while (d_.Length < n_.Length) n_ = n_.Remove(n_.Length-2);
                if (e_.Length > 4) e_=e_.Remove(4);                         //
                else if (e_.Length < 4) e_ += "00";                         //длина открытого ключа всегда 4 символа!
                WriteInFile("open_key.txt", e_+n_);                         //
                WriteInFile("secret_key.txt", d_+n_);                       //сохраняем в файлы ключи(+общая часть ключа)
                string str = ReadFromFile("Alice.txt");                     //читаем исходное слово из файла Alice.txt
                int len = str.Length;                                       //длина исходного слова
                BigInteger[] arr_crypt = new BigInteger[len];               //создания массива для записи в него заифрованных кодов символов
                /**/
                //BigInteger[] arrw1 = new BigInteger[len];                   //(del)
                //BigInteger[] arrw2 = new BigInteger[len];                   //(del)
                /**/
                for (int i = 0; i < len; i++)
                {
                    double iter_double = i;                                     //сохраняем целочисленные переменные в вещественные для правильного вычисления прцоцента
                    int progress = (int)(iter_double/len * 25);                 //отображает прогресс
                    Console.CursorLeft -= 3;                                    //
                    Console.Write($"{75+progress+"%",3}");                      //
                    int x =((int)str.ElementAt(i)*(i+1));                       //шифрование i-го элемента строки и запись результата в массив arr_crypt
                    arr_crypt[i] = Encrypt_num(x);                              //
                    /**/
                    //arrw1[i] = x;                                               //ЗАПИСЫВАЕМ ШИФРУЕМОЕ ЗНАЧЕНИЕ(del)
                    //arrw2[i] = arr_crypt[i];                                    //ШИФР (del)
                    /**/
                }
                /**/
                //WriteInFile("temp_in.txt", arrw1, arrw2, len);              //ЗАПИСЬ В ФАЙЛ ТАБЛИЦЫ ЗНАЧЕНИЙ И ИХ ШИФРОВ(del)
                /**/
                string outBob = "";                                         //создание конечной строки
                for (int i = 0; i < len; i++)
                {
                    byte[] BY = arr_crypt[i].ToByteArray();                     //преобразовали BigInteger в массив байтов
                    string s = "";                                              //
                    s = BitConverter.ToString(BY).Replace("-", "");             //записываем массив байтов в строку в 16 ричной системе,заменяя "-" на ""
                    while (s.Length < lenforword) s = s.Insert(s.Length, "0");  //добивание коротких слов нулями
                    if (s.Length > lenforword) s = s.Remove(lenforword);        //убираем незначащие нули в слишком длинных словах
                    outBob += s;                                                //запись в конечную строку
                }

                WriteInFile("Bob.txt", outBob);                             //запись в файл зашифрованного сообщения
                Console.WriteLine(); Console.WriteLine("Ready");            //
            }
            else if (ch == "2")                                         //при выборе 2 мы шифруем текст по уже имеющися ключам
            {
                Console.CursorTop--;
                Console.Write($"Encrypting:{"0%",3}");                               //
                string secret_key = ReadFromFile("secret_key.txt");         //буфферная строка,для определения глобальной длины массива байтов
                bytelen = secret_key.Length / 4;                            //устанавливаем глобалную длину массивов байтов
                lenforword = bytelen * 4;                                   //устанавливаем глоабльную длину слова
                string n_ = "";
                string e_ = "";
                e_ = ReadFromFile("open_key.txt");                          //считываем ключи из файла
                for (int i = 4; i < e_.Length; i++) n_ += e_[i];
                e_ = e_.Remove(4);
                e = HexToDec(e_+"00");
                n = HexToDec(n_);
                string str = ReadFromFile("Alice.txt");                     //читаем исходное слово из файла Alice.txt
                int len = str.Length;                                       //длина исходного слова
                BigInteger[] arr_crypt = new BigInteger[len];               //создания массива для записи в него заифрованных кодов символов
                for (int i = 0; i < len; i++)
                {
                    double iter_double = i;                                     //сохраняем целочисленные переменные в вещественные для правильного вычисления прцоцента
                    int progress = (int)(iter_double / len * 100);               //отображает прогресс
                    Console.CursorLeft -= 3;                                    //
                    Console.Write($"{progress + "%",3}");                  //
                    arr_crypt[i] = Encrypt_num(str.ElementAt(i) * (i + 1));     //шифрование i-го элемента строки и запись результата в массив arr_crypt
                }
                string outBob = "";                                         //создание конечной строки
                for (int i = 0; i < len; i++)                               //
                {
                    byte[] BY = arr_crypt[i].ToByteArray();                     //преобразовали BigInteger в массив байтов
                    string s = "";                                              //
                    s = BitConverter.ToString(BY).Replace("-", "");             //записываем массив байтов в строку в 16 ричной системе,заменяя "-" на ""
                    while (s.Length < lenforword) s = s.Insert(s.Length, "0");  //добивание коротких слов нулями
                    if (s.Length > lenforword) s = s.Remove(lenforword);        //убираем незначащие нули в слишком длинных словах
                    outBob += s;                                                //запись в конечную строку
                }

                WriteInFile("Bob.txt", outBob);                             //запись в файл зашифрованного сообщения
                Console.WriteLine(); Console.WriteLine("Ready");                                 //
            }
            else if (ch == "3")                                         //Расшифруем текст из файла Bob
            {
                Console.CursorTop--;                                        //
                Console.Write($"Decrypt:{"0%",3}");                         //
                string str_buff = ReadFromFile("secret_key.txt");           //буфферная строка,для определения глобальной длины массива байтов
                string n_ = "";
                for (int i = str_buff.Length / 2; i < str_buff.Length; i++) n_ += str_buff[i];
                str_buff = str_buff.Remove(str_buff.Length / 2);
                n = HexToDec(n_);
                d = HexToDec(str_buff);                                     // 
                bytelen = str_buff.Length / 4;                              //устанавливаем глобалную длину массивов байтов
                lenforword = bytelen * 4;                                   //устанавливаем глоабльную длину слова
                
                string str = ReadFromFile("Bob.txt");                       //

                int cursor_top_temp = Console.CursorTop;                    //Сохраняем позицию указателя от верхнего края
                int cursor_left_temp = Console.CursorLeft-3;                //Сохраняем позицию указателя от левого края

                FileStream fs = new FileStream("Bob.txt", FileMode.OpenOrCreate, FileAccess.Read);      //создание файлового потока
                StreamReader sr = new StreamReader(fs);                                                 //создание потока чтения
                string arr_dcrypt = "";                                     //конечная строка
                int iter = 1;                                               //позиция символа в расшифрованной строке
                while (!sr.EndOfStream)                                     //пока не будет достигнут конец файла
                {
                    byte[] BN = new byte[bytelen * 2];                          //создаем массив байтов,размер соответсвует полудлине слова(1 байт это 2 16-ричных символа)
                    string srt = "";                                            //
                    int buf = 2 * bytelen;                                      //кол-во байт в каждом слове
                    for (int j = 0; j < buf; j++)                               //считываем внутри цикла пары чисел(1 байт) в массив байтов
                    {
                        srt += Convert.ToChar(sr.Read());                           //читаем следующие 2 символа
                        srt += Convert.ToChar(sr.Read());                           //
                        BN[j] = Convert.ToByte(Convert.ToInt32(srt, 16));           //переводим их из 16ричной системы в число,и записываем в массив байтов
                        srt = "";                                                   //обнуление строки
                    }
                    BigInteger BI = new BigInteger(BN);                         //создаем экземпляр BigInteger из массива байтов
                    if (BI < 0) BI = plusforbytes(bytelen * 2) + BI;            //проблема при переводе из массива байтов в BigInteger,эта строка приводит все в порядок
                    BigInteger lol = Decrypt_num(BI);                           //расшифровываем в новый экземпляр BigInteger
                    int res = (int)(lol/iter);                                  //
                    iter++;                                                     //увеличиваем число,на которое делимм
                    arr_dcrypt = arr_dcrypt + Convert.ToChar(res).ToString();   //преобразуем код символа в символ,и вставляем в конечную строку


                    double iter_double = iter - 1;                                          //сохраняем целочисленные переменные в вещественные для правильного вычисления прцоцента
                    int progress = (int)((iter_double * lenforword) / str.Length * 100);    //вычисляем процент
                    Console.SetCursorPosition(cursor_left_temp, cursor_top_temp);           //переходим на заданную позицию
                    Console.WriteLine($"{progress + "%",3}");                               //выводим процент выполнения
                }
                sr.Close();                                                         //закрытие потоков
                fs.Close();
                WriteInFile("Alice.txt", arr_dcrypt);                               //запись результата в файл
                Console.WriteLine("Ready");                                         //

            }
            else if (ch == "0") return;                                         //выход из программы

            Console.ReadKey();
        }
    }
}
