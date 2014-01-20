using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UserGen
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // zmiana kolorów tła oraz tekstu
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write("Podaj login: ");
                String login = Console.ReadLine(); // wczytanie loginu
                String password = ReadPassword(); // wczytanie hasła
                password = CalculateSHA256(password, new ASCIIEncoding()); // wygenerowanie skrótu hasła
                String privateKey = ""; // klucz prywatny RSA
                String publicKey = ""; // klucz publiczny RSA
                GenerateRSAKey(ref privateKey, ref publicKey); // wygenerowanie klucza prywatnego oraz publicznego RSA
                Console.WriteLine("Wygewnerowano parę kluczy RSA.");
                Console.WriteLine("Użytkownik został wygenerowany.");

                // zapisanie danych do plików
                StreamWriter client = new StreamWriter("private-" + login + ".txt");
                client.WriteLine(password);
                client.WriteLine(privateKey);
                client.WriteLine(publicKey);
                client.Close();
                StreamWriter server = new StreamWriter("public-" + login + ".txt");
                server.WriteLine(publicKey);
                server.Close();

                // zakończenie pracy programu
                Console.WriteLine("Dane zostały zapisane.");
                Console.ReadKey();
                Console.Clear();
                Environment.Exit(0);
            }
            catch
            {
                Console.WriteLine("Wystąpił bład!");
                Console.ReadKey();
                Console.Clear();
                Environment.Exit(0);
            }
        }

        // wczytanie hasła
        private static String ReadPassword()
        {
            String text = "\rPodaj Hasło: ";
            String password = ""; // podane hasło
            char passChar = ' '; // wciśnięty znak
            while (true)
            {
                Console.Write(text);
                Console.ForegroundColor = ConsoleColor.Black; // brak widoczności hasła
                passChar = Console.ReadKey().KeyChar;
                Console.ForegroundColor = ConsoleColor.White; // ponowna widoczność hasła
                if (passChar == '\r') break; // do wciśnięcia ENTER
                password += passChar.ToString();
            }
            Console.WriteLine();
            return password;
        }

        // skrócenia stringu algorytmem SHA256
        private static String CalculateSHA256(String text, Encoding encoder)
        {
            byte[] buffer = encoder.GetBytes(text); // zamiana stringu na tablice bajtów
            SHA256CryptoServiceProvider cryptoTransformSHA256 = new SHA256CryptoServiceProvider(); // skrócenie hasła
            return BitConverter.ToString(cryptoTransformSHA256.ComputeHash(buffer)).Replace("-", "");
        }

        // generacja klucz pary kluczy RSA
        private static void GenerateRSAKey(ref String privateKey, ref String publicKey)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.KeySize = 1024; // rozmiar klucza
            RSAParameters key = rsa.ExportParameters(true); // generacja pary kluczy
            rsa.ImportParameters(key);
            privateKey = rsa.ToXmlString(true); // eksport klucza prywatnego do zmiennej
            publicKey = rsa.ToXmlString(false); // eksport klucza publicznego do zmiennej
            rsa.Dispose();
        }
    }
}
