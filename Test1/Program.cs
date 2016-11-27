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
        static byte[] ReadFromFile(string name) {
            FileStream fs = new FileStream(name, FileMode.Open, FileAccess.Read);
            BinaryReader sr = new BinaryReader(fs,Encoding.ASCII);                      //ЛИБО АСКИ ЛИБО НЕ РАБОТАЕТ,ВОТ ЭТО ВОТ Я НЕ ПОНЯЛ 
            byte[] b = new byte[fs.Length];
            b= sr.ReadBytes((int)fs.Length);
            sr.Close();
            fs.Close();
            return b;
        }

        static string ReadFromTextFile(string name)
        {
            string str = "";
            FileStream fs = new FileStream(name, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs,enc);                      //ЛИБО АСКИ ЛИБО НЕ РАБОТАЕТ,ВОТ ЭТО ВОТ Я НЕ ПОНЯЛ 
            str = sr.ReadToEnd();
            sr.Close();
            fs.Close();
            return str;
        }

        //запись в файл с именем name текста text
        static void WriteInTextFile(string name, string text)
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
        static void WriteInTextFile(string name, BigInteger text)
        {
            File.Delete(name);
            FileStream fStream = new FileStream(name, FileMode.OpenOrCreate);
            StreamWriter streamWriter = new StreamWriter(fStream,enc);
            streamWriter.BaseStream.Seek(0, SeekOrigin.End);
            streamWriter.Write(text);
            streamWriter.Close();
            fStream.Close();
        }

        static void WriteInFile(string name, byte[] text)
        {
            File.Delete(name);
            FileStream fStream = new FileStream(name, FileMode.OpenOrCreate);
            fStream.Write(text,0,text.Length);
            fStream.Close();
        }

        //преобразование числа из 10-чной с.с в 2-чную
        static string DecToBin(BigInteger dec)
        {
            string res="";
            while(dec>0)                                      //
            {                                                   //
                res+= dec % 2;                                  //вычисляем сколько едениц в двоичном представлении
                dec /= 2;                                       //
            }                                                   //
            return res;
        }

        //кол-во вхождений символа ch в строке str
        static int HowManyContains(string str,char ch)
        {
            int res = 0;
            for (int i =0; i< str.Length; i++)
            {
                if (str[i] == ch) res++;
            }
            return res;
        }

        //возводит число num в степень pow по модулю mod(Метод повторяющихся возведения в квадрат и умножения)
        static BigInteger BigIntegerPowMod_eff(BigInteger x, BigInteger pow, BigInteger mod)
        {
            BigInteger res = 1;
            string pow_bin = DecToBin(pow);                         //двоичная запись числа pow
            int count_one = HowManyContains(pow_bin, '1');          //сколько вхождений 1 в двоичной записи

            BigInteger[] arr = new BigInteger[count_one];           //массив конечных множителей
            BigInteger[] arr_pow = new BigInteger[count_one];       //массив степений,чьи позиции соответсвуют каждому множителю
            BigInteger max_pow = pow_bin.Length;                      //максимальная степень
            int buf_i = count_one - 1;                  //
            for (int i = 0; i < pow_bin.Length; i++)    //
            {                                           //
                if (pow_bin[i] == '1')                  //записываем какие степени будут в массив
                {                                       //
                    arr_pow[buf_i] = i;                 //и сразу забиваем массив значений начальным значением
                    arr[buf_i] = x;                     //
                    buf_i--;                            //
                }                                       //
            }                                           //

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

        //Вероятностный тест простоты Миллера-Рабина 
        static bool MillersRabinsTest(BigInteger n)
        {
            bool flag_prime = false;
            bool flag_composite=false;                  //флаг,true если число составное
            BigInteger n_minus = n - 1;                 //n-1=2^r*d
            BigInteger r = 0;                           //степень двойки
            BigInteger k = 0;                           //кол-во раундов
            BigInteger d = n_minus;                     //множитель из формулы выше
            while (d % 2 == 0) { r++; d /= 2; }         //считаем r и d
            while (n != 1) { k++; n /= 2; }   //подобие логарифма по основанию 2,log(2)n_temp=k
            for (BigInteger i = 0; i < k; i++)          //
            {
                BigInteger a = n + 1;                         //создаем возможного свидетеля простоты
                while (a >= n_minus+1)                              //
                {
                    a = getRandom(bytelen);                         //ищем а<n, случайным образом
                    if (a < 0) a = plusforbytes(bytelen) + a;       //решение проблемы с переводом байтов в BigInteger
                }
                BigInteger x = BigIntegerPowMod_eff(a, d, n_minus + 1); //x=a^d mod n, эффективный способ
                if (x != 1 && x != n_minus)                             //Если x не равен 1 или n-1
                {
                    bool temp_flag = false;                         //флаг продолжения работы,true если число не простое НО и не составное (еще не определили)
                    for (BigInteger j = 0; j < r - 1; j++)          //
                    {
                        x = myPow(x, 2) % (n_minus + 1);                //x^2 mod n
                        if (x == 1) flag_composite = true;              //если х равен 1 то составное
                        if (x == n_minus) temp_flag = true;             //если х равен n-1 то продолжить проверку с другими возможными свидетелями
                    }
                    if (!temp_flag) flag_composite = true;          //если флаг продолжения работы не поднят,то число составное
                }
                if (flag_composite) i = k;                      //если число составное то закончим цикл
                else if (i == k - 1) flag_prime = true;         //если цикл закончился,а предположение о простоте числа не опровергнуто,то число простое
            }
            return flag_prime;
        }

        //создание случайного простого числа BigInteger (ВОЗОЖНО МОЖНО УПРОСТИТЬ ЛОГИКУ)
        static BigInteger getPrime()
        {
            BigInteger res = 1;                     //результирующая переменная
            while (res==1)                          //
            {
                BigInteger n = 0;                                               //создаем число
                while (n % 2 == 0 || n == 1) n = getRandom(bytelen);            //ищем нечетное число
                if (n < 0) n = plusforbytes(bytelen) + n;                       //решение проблемы с переводом байтов в BigInteger

                BigInteger m = 0;                                               //создаем число для параллельной проверки
                while (m % 2 == 0 || m == 1 || m==n) m = getRandom(bytelen);    //ищем нечетное число
                if (m < 0) m = plusforbytes(bytelen) + m;                       //решение проблемы с переводом байтов в BigInteger

                Task<bool> task_1 = new Task<bool>(() => { return MillersRabinsTest(m); });
                task_1.Start();
                if (MillersRabinsTest(n)) res = n;
                task_1.Wait();
                if (task_1.Result) res = m;
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
        static void CreateKeys()
        {
            Console.Write($"{"0%",3}");                 //*просто процент выполнения*
            Task<BigInteger> p_ = new Task<BigInteger>(getPrime);
            p_.Start();
            Thread.Sleep(1);
            BigInteger q = getPrime();                  //создание 2-го простого числа
            p_.Wait();
            BigInteger p = p_.Result;                   //создание 1-го простого числа
            Console.CursorLeft -= 3;                    //*просто процент выполнения*
            Console.Write($"{"10%",3}");                //*
            while (q==p) q = getPrime();                //изменение 2-го простого числа если оно совпадает с 1-ым
            Console.CursorLeft -= 3;                    //*
            Console.Write($"{"20%",3}");                //*просто процент выполнения*
            n = q * p;                                  //общая часть ключа
            BigInteger eiler = (q - 1) * (p - 1);       //Вычисление функции Эйлера
            e = PrimesRand(3, 65521);                       //рандомно берем открытую экспоненту
            while(eiler % e == 0) e = PrimesRand(3, 65521); //
            BigInteger d_buf = 1;
            BigInteger k = 0;
            while (d_buf % e != 0)                  //вычисляем
            {                                       //секретную
                k++;                                //экспоненту d
                d_buf = (k * eiler + 1);
            }
            d = d_buf/e;                            //записываем результат в глобальную переменную
        }

        //шифрование числа num
        static BigInteger Encrypt_num(BigInteger num)
        {
            return BigIntegerPowMod_eff(num, e, n); ;       //используем метод эффективного возведения num в степень e по модулю n
        }

        //расшифровка числа num
        static BigInteger Decrypt_num(BigInteger num)
        {
            return BigIntegerPowMod_eff(num, d, n);         //используем метод эффективного возведения num в степень d по модулю n
        }

        //Копировать из массива байт с позиции а(включительно) до b(исключительно)
        static byte[] CopyTo(byte[] bt, int a, int b)
        {
            byte[] bt_ = new byte[b - a];
            for (int i = a; i < b; i++) bt_[i - a] = bt[i];
            return bt_;
        } 

        static void Main(string[] args)
        {
            Console.WriteLine("Choose action:");                                        //
            Console.WriteLine("1.Encrypt with create new keys");                        //
            Console.WriteLine("2.Encrypt without create new keys");                     //меню выбора действия
            Console.WriteLine("3.Decrypt");                                             //
            Console.WriteLine("0.Exit");                                                //
            string ch;
            ch=Console.ReadLine();
            while (ch != "1" && ch != "2" && ch != "0" && ch != "3")
            {
                Console.CursorTop--;
                Console.WriteLine("Invalid value! Try again");              //реакция на не правильный ввод команды
                ch = Console.ReadLine();
            }
            if (ch == "1")                                              //Шифруем текст из файла
            {
                int time = DateTime.Now.Minute * 60 + DateTime.Now.Second;  //записываем исходное время,для подсчета времени работы программы
                Console.Write("Enter key length(in bit):");                 //
                string z = "";                                              //
                z += Console.ReadLine();                                    //задаем длину ключа в битах
                while (z == "" || Convert.ToInt32(z)%16!=0 || z.Length>4)   //
                {                                                           //
                    z = "";                                                     //
                    Console.Write("Invalid value! Try again:");                 //защита от дурака
                    z += Console.ReadLine();                                    //
                }                                                           //
                Console.Write("File name:");                                //спрашиваем имя файла
                string name;                                                //имя файла
                name = Console.ReadLine();                                  //
                while (!File.Exists(name))                              //проверка на существование файла
                {
                    Console.Write("File not exists,try again:");        //иначе спросить снова имя файла
                    name = Console.ReadLine();                          //
                }
                Console.Write("Encrypting:");                               //
                bytelen = Convert.ToInt32(z)/16;                            //устанавливаем глобалную длину массивов байтов(кол-во бит делим на 2 и переводим в байты)
                CreateKeys();                                               //создание ключей
                byte[] e_ = e.ToByteArray();                                //переводим все ключи в массивы байтов
                byte[] d_ = d.ToByteArray();                                //
                byte[] n_ = n.ToByteArray();                                //
                if (d_.Length > n_.Length)                                  //уравнение длины ключа n до длины ключа d
                {
                    byte[] x = new byte[d_.Length];                             //если длина n меньше длины d
                    for (int i = 0; i < n_.Length; i++) x[i] = n_[i];           //
                    for (int i = n_.Length + 1; i < x.Length; i++) x[i] = 0;    //
                    n_ = x;                                                     //
                }
                if (d_.Length < n_.Length)                                  //если длина n больше длины d
                {
                    byte[] x = new byte[d_.Length];                             //
                    for (int i = 0; i < x.Length; i++) x[i] = n_[i];            //
                    n_ = x;                                                     //
                } 
                byte[] dn = new byte[d_.Length * 2];                        //массив который вместит ключи d и n
                for (int i = 0; i < d_.Length; i++) dn[i] = d_[i];                      //записываем ключ d
                for (int i = d_.Length; i < dn.Length; i++) dn[i] = n_[i - d_.Length];  //записываем ключ n
                if (e_.Length == 3)                                         //(значение не превышает 2 байта)
                {
                    byte[] x = new byte[2];                                     //если лишний нулевой байт,то удалим его
                    x[0] = e_[0]; x[1] = e_[1];                                 //
                    e_ = x;                                                     //
                }
                else if (e_.Length == 1)                                    //если значение уестилось в 1 байт,то допишем нулевой байт
                {
                    byte[] x = new byte[2];                                     //
                    x[0] = e_[0]; x[1] = 0;                                     //
                    e_ = x;                                                     //
                }
                byte[] en = new byte[e_.Length +n_.Length];                 //массив содержащий ключи e и n
                for (int i = 0; i < e_.Length; i++) en[i] = e_[i];                      //записываем ключ e
                for (int i = e_.Length; i < en.Length; i++) en[i] = n_[i - e_.Length];  //записываем ключ n
                WriteInFile("open_key", en);                            //
                WriteInFile("secret_key", dn);                          //сохраняем в файлы ключи(+общая часть ключа)
                byte[] str = ReadFromFile(name);                        //читаем побайтно исходный файл
                int len = str.Length;                                   //длина исходного слова(байты)
                BigInteger[] arr_crypt = new BigInteger[len];           //создание массива для записи в него зашифрованных байтов
                for (int i = 0; i < len; i+=8)                          //ПРОЦЕСС ШИФРОВАНИЯ
                {
                    double iter_double = i+1;                                   //*сохраняем целочисленные переменные в вещественные для правильного вычисления прцоцента
                    int progress = (int)(iter_double/len * 80);                 //*отображает прогресс
                    Console.CursorLeft -= 3;                                    //*
                    Console.Write($"{20+progress+"%",3}");                      //* 

                    bool[] flag = new bool[8];                                  //массив флагов для обозначения кол-ва потоков
                    for (int j = 0; j < 8; j++) flag[j] = false;                //заполняе их false
                    byte[] b = new byte[8];                                     //массив байт куда запишем элементы входной строки
                    for (int j = 0; j < 8; j++)                                 //проверка сколько можно прочитать символов после i-го
                    {
                        if ((i + j) < len)                                      //если есть что читать
                        {
                            b[j] = str[i + j];                                  //читаем
                            flag[j] = true;                                     //и подниамем флаг
                        }
                        else j = 7;                                             //иначе закончить цикл
                    }    
                    Task<BigInteger>[] task = new Task<BigInteger>[7];                      //массив тасков
                    task[0] = new Task<BigInteger>(() => { return Encrypt_num(b[1]); });    //Task это параллельная задача
                    task[1] = new Task<BigInteger>(() => { return Encrypt_num(b[2]); });    //Task будет вызван если прочитали слово для task'а
                    task[2] = new Task<BigInteger>(() => { return Encrypt_num(b[3]); });    //
                    task[3] = new Task<BigInteger>(() => { return Encrypt_num(b[4]); });    //НА КАЖДЫЙ ТАСК  
                    task[4] = new Task<BigInteger>(() => { return Encrypt_num(b[5]); });    //СТАТИЧЕСКИ ЗАДАЕМ СООТВЕТСТВИЕ
                    task[5] = new Task<BigInteger>(() => { return Encrypt_num(b[6]); });    //С ШИФРУЕМЫМИ ЭЛЕМЕНТАМИ МАССИВА
                    task[6] = new Task<BigInteger>(() => { return Encrypt_num(b[7]); });    //
                    for (int j = 1; j < 8; j++)                                         //7 раз проверяем флаги
                    {
                        if (flag[j])                                                    //если флаг поднят,то запустить task
                            task[j - 1].Start();                                        //
                        else j = 7;                                                     //иначе закончить цикл
                    }
                    arr_crypt[i] = Encrypt_num(b[0]);                                   //записываем в массив зашифрованных чисел 1 число
                    for (int j = 1; j < 8; j++)                                         //потом через проверку флагов дописываем результаты из потоков
                    {
                        if (flag[j])                                                    //если флаг поднят
                        {
                            task[j - 1].Wait();                                         //ждем завершения вычислений
                            arr_crypt[i + j] = task[j - 1].Result;                      //запсиываем
                        }
                        else j = 7;                                                     //иначе закончить цикл
                    }
                }
                byte[] name_ = Encoding.ASCII.GetBytes(name);                       //запишем имя файла в массив байтов
                int lenname = name_.Length;                                         //длина имени
                byte[] outBob = new byte[arr_crypt.Length*bytelen*2+1+lenname];           //создание конечной строки(размер исходного файла*на длину ключа в байтах+размер имени+имя файла)
                outBob[0] = Convert.ToByte(lenname);                                //записываем в 0 элемент длину имени
                for (int i = 1; i < lenname + 1; i++) outBob[i] = name_[i - 1];     //записываем имя
                int iter = 0;                                                       //итератор для конечного массива байт
                for (int i = 0; i < len; i++)                                       //
                {
                    byte[] BY = new byte[bytelen * 2];                                          //создаем массив байтов длины в байтах одного заифрованного байта
                    byte[] x  = arr_crypt[i].ToByteArray();                                     //преобразовали BigInteger в массив байтов
                    if (x.Length <= bytelen * 2)                                                //если длина массива оказалась меньше ожидаемой,то дополняем нулями
                    {
                        for (int j = 0; j < x.Length; j++) BY[j] = x[j];                            //
                        for (int j = x.Length; j < BY.Length; j++) BY[j] = 0;                       //
                    }
                    if (x.Length > bytelen * 2)                                                 //Если оказалась больше,то уберем незначащие нули
                        BY = CopyTo(x, 0, BY.Length);                                               //
                    for (int j = 0; j < bytelen * 2; j++) outBob[j + iter+lenname+1] = BY[j];   //добавляем в конечный массив массив этой итерации цикла
                    iter += bytelen * 2;                                                        //наращиваем итератор(прибавляем столько,сколько весит 1 зашифрованный байт)
                }
                WriteInFile("Bob", outBob);                                         //запись в файл зашифрованного сообщения
                Console.WriteLine();                                                //
                time = DateTime.Now.Minute * 60 + DateTime.Now.Second - time;       //вычисляе затраченное время
                Console.WriteLine($"Your time {time + "sec",7}. Ready");            //выводим время и сообщение о завершении работы
            }
            /**/
            /**/
            else if (ch == "2")                                         //при выборе 2 мы шифруем текст по уже имеющися ключам
            {
                int time = DateTime.Now.Minute * 60 + DateTime.Now.Second;  //записываем исходное время,для подсчета времени работы программы
                Console.Write("File name:");                                //спрашиваем имя файла
                string name;                                                //запишем это в name
                name = Console.ReadLine();                                  //
                while (!File.Exists(name))                                  //проверка на существование файла
                {
                    Console.Write("File not exists,try again:");                //если такого нет
                    name = Console.ReadLine();                                  //то спросить снова имя файла
                }
                Console.Write($"Encrypting:{"0%",3}");                  //
                byte[] open_key = ReadFromFile("open_key");             //буфферная строка,для определения глобальной длины массива байтов
                bytelen = (open_key.Length-2) / 2;                      //устанавливаем глобалную длину массивов байтов
                byte[] n_ = CopyTo(open_key, 2, open_key.Length);       //в массив байтов из входной строки копируем часть с ключом n
                byte[] e_ = CopyTo(open_key, 0, 2);                     //в массив байтов из входной строки копируем часть с ключом e
                n = new BigInteger(n_);                                 //преобразуем в число
                if (n < 0) n = plusforbytes(bytelen * 2) + n;           //исправляем если отрицательное
                e = new BigInteger(e_);                                 //преобразуем в число
                if (e < 0) e  = plusforbytes(2) + e;                    //исправляем если отрицательное
                byte[] str = ReadFromFile(name);                            //читаем байты из файла
                int len = str.Length;                                       //длина в байтах
                BigInteger[] arr_crypt = new BigInteger[len];               //создание массива для записи в него зашифрованных кодов символов
                for (int i = 0; i < len; i+=8)                              //ПРОЦЕСС ШИФРОВАНИЯ
                {
                    double iter_double = i+1;                                   //*сохраняем целочисленные переменные в вещественные для правильного вычисления прцоцента
                    int progress = (int)(iter_double / len * 100);              //*отображает прогресс
                    Console.CursorLeft -= 3;                                    //*
                    Console.Write($"{progress + "%",3}");                       //*

                    bool[] flag = new bool[8];                                  //массив флагов для обозначения кол-ва потоков
                    for (int j = 0; j < 8; j++) flag[j] = false;                //заполняе их false
                    byte[] b = new byte[8];                                     //массив байт куда запишем элементы входной строки
                    for (int j = 0; j < 8; j++)                                 //проверка сколько можно прочитать символов после i-го
                    {
                        if ((i + j) < len)                                      //если есть что читать
                        {
                            b[j] = str[i + j];                                  //читаем
                            flag[j] = true;                                     //и подниамем флаг
                        }
                        else j = 7;                                             //иначе закончить цикл
                    }
                    Task<BigInteger>[] task = new Task<BigInteger>[7];                      //массив тасков
                    task[0] = new Task<BigInteger>(() => { return Encrypt_num(b[1]); });    //Task это параллельная задача
                    task[1] = new Task<BigInteger>(() => { return Encrypt_num(b[2]); });    //Task будет вызван если прочитали слово для task'а
                    task[2] = new Task<BigInteger>(() => { return Encrypt_num(b[3]); });    //
                    task[3] = new Task<BigInteger>(() => { return Encrypt_num(b[4]); });    //НА КАЖДЫЙ ТАСК  
                    task[4] = new Task<BigInteger>(() => { return Encrypt_num(b[5]); });    //СТАТИЧЕСКИ ЗАДАЕМ СООТВЕТСТВИЕ
                    task[5] = new Task<BigInteger>(() => { return Encrypt_num(b[6]); });    //С ШИФРУЕМЫМИ ЭЛЕМЕНТАМИ МАССИВА
                    task[6] = new Task<BigInteger>(() => { return Encrypt_num(b[7]); });    //
                    for (int j = 1; j < 8; j++)                                         //7 раз проверяем флаги
                    {
                        if (flag[j])                                                    //если флаг поднят,то запустить task
                            task[j - 1].Start();                                        //
                        else j = 7;                                                     //иначе закончить цикл
                    }
                    arr_crypt[i] = Encrypt_num(b[0]);                                   //записываем в массив зашифрованных чисел 1 число
                    for (int j = 1; j < 8; j++)                                         //потом через проверку флагов дописываем результаты из потоков
                    {
                        if (flag[j])                                                    //если флаг поднят
                        {
                            task[j - 1].Wait();                                         //ждем завершения вычислений
                            arr_crypt[i + j] = task[j - 1].Result;                      //запсиываем
                        }
                        else j = 7;                                                     //иначе закончить цикл
                    }
                }
                byte[] name_ = Encoding.ASCII.GetBytes(name);                       //запишем имя файла в массив байтов
                int lenname = name_.Length;                                         //длина имени
                byte[] outBob = new byte[arr_crypt.Length * bytelen * 2 + 1 + lenname];       //создание конечной строки
                outBob[0] = Convert.ToByte(lenname);                                //записываем в 0 элемент длину имени
                for (int i = 1; i < lenname + 1; i++) outBob[i] = name_[i - 1];     //записываем имя
                int iter = 0;                                                       //итератор для выходного массива
                for (int i = 0; i < len; i++)                                       //
                {
                    byte[] BY = new byte[bytelen * 2];                                          //создаем массив байтов длины в байтах одного заифрованного байта
                    byte[] x = arr_crypt[i].ToByteArray();                                      //преобразовали BigInteger в массив байтов
                    if (x.Length <= bytelen * 2)                                                //если длина массива оказалась меньше ожидаемой,то дополняем нулями
                    {
                        for (int j = 0; j < x.Length; j++) BY[j] = x[j];                            //
                        for (int j = x.Length; j < BY.Length; j++) BY[j] = 0;                       //
                    }
                    if (x.Length > bytelen * 2)                                                 //Если оказалась больше,то уберем незначащие нули
                        BY = CopyTo(x, 0, BY.Length);                                               //
                    for (int j = 0; j < bytelen * 2; j++) outBob[j + iter + lenname + 1] = BY[j];   //добавляем в конечный массив массив этой итерации цикла
                    iter += bytelen * 2;                                                        //наращиваем итератор(прибавляем столько,сколько весит 1 зашифрованный байт)
                }
                WriteInFile("Bob", outBob);                                 //запись в файл зашифрованного сообщения
                Console.WriteLine();
                time = DateTime.Now.Minute * 60 + DateTime.Now.Second - time;       //подсчет времени работы программы
                Console.WriteLine($"Your time {time + "sec",7}. Ready");            //
            }
            /**/
            /**/
            if (ch == "3")                                         //Расшифруем текст из файла Bob
            {
                int time = DateTime.Now.Minute*60 + DateTime.Now.Second;    //записывае время начала работы прораммы
                Console.CursorTop--;                                        //
                Console.Write($"Decrypt:{"0%",3}");                         //
                byte[] str_buff = ReadFromFile("secret_key");               //в этот массив читае данные из секретного ключа
                byte[] n_ = new byte[str_buff.Length/2];                    //массив общей части(половина от всей длины)
                for (int i = str_buff.Length / 2; i < str_buff.Length; i++) //читаем вторую половину входного массива
                    n_[i- str_buff.Length / 2] = str_buff[i];                   //и записываем в массив общего ключа
                byte[] d_ = new byte[str_buff.Length / 2];                  //массив секретно части(половина от всей длины)
                for (int i = 0; i < str_buff.Length/2; i++)                 //читаем первую половину входного массива
                    d_[i] = str_buff[i];                                        //и записываем в ассив секретного ключа
                bytelen = str_buff.Length / 4;                              //устанавливаем глобалную длину массивов байтов
                lenforword = str_buff.Length;                               //длинна входного массива
                n = new BigInteger(n_);                                     //записываем общую часть ключей
                if (n < 0) n = plusforbytes(bytelen * 2) + n;               //поправка если отрицательное число
                d = new BigInteger(d_);                                     //записываем секретный ключ
                if (d < 0) d = plusforbytes(bytelen * 2) + d;               //поправка если отрицательное число

                byte[] str = ReadFromFile("Bob");                           //читаем из файла массив байтов с зашифрованным сообщением
                int lenname = Convert.ToInt32(str[0]);                      //считывае длину имени файла в который запишем расшифрованное сообщение
                byte[] name_ = new byte[lenname];                           //массив для получения имени
                for (int i = 1; i < lenname + 1; i++) name_[i - 1] = str[i];//записываем в массив имя
                string name = Encoding.ASCII.GetString(name_);              //переводим имя из массива байтов в строку
                int cursor_top_temp = Console.CursorTop;                    //Сохраняем позицию указателя от верхнего края
                int cursor_left_temp = Console.CursorLeft - 3;              //Сохраняем позицию указателя от левого края

                byte[] arr_dcrypt = new byte[str.Length/(bytelen*2)];       //конечный массив
                int iter = 0;                                               //сколько байт расшифровали()
                for (int i = lenname + 1; i < str.Length; i += bytelen * 16)//читаем весь входной массив до конца
                {
                    bool[] flag = new bool[8];          //инициализация флагов,если true,то Task с тем же номером выполнится
                    for (int j = 0; j < 7; j++)         //все флаги забиываем false
                        flag[j] = false;                    //
                    BigInteger[] BI = new BigInteger[8];//инициализцаия переменных куда считаем слова(1 слово в шифре = 1 символ в изначальном тексте)
                    byte[][] b = new byte[8][];         //массив массивов байтов(из 1 массива получи 1 байт после расшифровки)
                    for (int j=0;j<8;j++)               //задаем ширину матрицы байтов
                        b[j]= new byte[bytelen * 2];    //
                    for (int g = 0; g < 8; g++)         //проверка сколько можно байт прочитать(при g=0 всегда true)
                    {
                        if ((i + bytelen * 2*g) < str.Length)                                 //если можем считать еще 1 массив байтов,после g-го
                        {
                            for (int j = 0; j < bytelen * 2; j++) b[g][j] = str[j + i + bytelen * 2*g]; //считываем массив из входного массива
                            BI[g] = new BigInteger(b[g]);                                               //преобразуе в число
                            if (BI[g] < 0) BI[g] = plusforbytes(bytelen * 2) + BI[g];                   //поправка если отрицательное число
                            flag[g] = true;                                                             //поднимаем флаг что прочитали g слов
                        }
                    }
                    Task<BigInteger>[] task = new Task<BigInteger>[7];                      //массив task'ов
                    task[0] = new Task<BigInteger>(() => { return Decrypt_num(BI[1]); });   //Task это параллельная задача
                    task[1] = new Task<BigInteger>(() => { return Decrypt_num(BI[2]); });   //Task будет вызван если прочитали слово для task'а
                    task[2] = new Task<BigInteger>(() => { return Decrypt_num(BI[3]); });   //
                    task[3] = new Task<BigInteger>(() => { return Decrypt_num(BI[4]); });   //НА КАЖДЫЙ ТАСК  
                    task[4] = new Task<BigInteger>(() => { return Decrypt_num(BI[5]); });   //СТАТИЧЕСКИ ЗАДАЕМ СООТВЕТСТВИЕ
                    task[5] = new Task<BigInteger>(() => { return Decrypt_num(BI[6]); });   //С РАСШИФРОВЫВАЕМЫМИ ЭЛЕМЕНТАМИ МАССИВА
                    task[6] = new Task<BigInteger>(() => { return Decrypt_num(BI[7]); });   //
                    
                    int[] res = new int[8];                             //в эти переменные запишем расшифрованный байт
                    for (int j = 0; j < 7; j++)                         //по флагам запускаем параллельные task'и
                        if (flag[j + 1]) task[j].Start();               //если прочитано j+1 слово-запускаем task в параллельный процесс(итого j+1 прцоессов)
                    BigInteger x= Decrypt_num(BI[0]);                   //в основном потоке расшифровываем один из массивов
                    res[0] = (int)x;                                    //расшифровываем слово

                    for (int j = 0; j < 7; j++)                         //записываем рузельтаты с запущенных параллельных процессов
                        if (flag[j + 1])                                //если прочитано j+1 слов
                        {
                            task[j].Wait();                             //ждем завершения процесса task[j]
                            res[j + 1] = (int)task[j].Result;           //записываем расшифрованный результат 
                        }

                    for (int j = 0; j < 8; j++)                             //и записываем все 
                        if (flag[j]) arr_dcrypt[iter + j] = (byte)res[j];   //в выходной массив


                    iter += 8;                                              //увеличиваем итератор на 8
                    double iter_double = iter - 1;                                          //*сохраняем целочисленные переменные в вещественные для правильного вычисления прцоцента
                    int progress = (int)((iter_double * lenforword) / str.Length * 100);    //*вычисляем процент
                    Console.SetCursorPosition(cursor_left_temp, cursor_top_temp);           //*переходим на заданную позицию
                    Console.WriteLine($"{progress/2 + "%",3}");                             //*выводим процент выполнения
                }
                WriteInFile(name, arr_dcrypt);                               //запись результата в файл
                time = DateTime.Now.Minute * 60 + DateTime.Now.Second - time;//вычисление времени работы программы
                Console.WriteLine($"Your time {time+"sec", 7}. Ready");      //
            }
            else if (ch == "0") return;                                         //выход из программы

            Console.ReadKey();
        }
    }
}
