using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace Server
{
    public class Server
    {
        private TcpListener tcpListener; // port nasłuchujący połączenia przychodzące
        private Thread listenThread; // wątek nasłuchujący połączenia od użytkowników
        private Dictionary<String, NetworkStream> connections; // para użytkownik - połączenie
        private Dictionary<String, byte[]> DESDict; // para użytkownik - klucz DES
        private Dictionary<String, String> RSADict; // para użytkownik - klucz publiczny RSA
        private String localIP; // ip serwera
        private int port; // port serwera
        private String configFile = "config.txt"; // plik konfiguracyjny
        private String privateKey; // klucz prywatny serwera

        public Server()
        {
            Console.ForegroundColor = ConsoleColor.White;
            try
            {
                // ustawienie parametrów z pliku
                String[] config = File.ReadAllLines(configFile);
                port = Convert.ToInt32(config[0]);
                privateKey = config[1];
            }
            catch
            {
                // bład podczas wczytywania z pliku
                WriteLog("Nie można wczytać pliku z konfiguracją: " + configFile, true);
                WriteLog("Wciśnij dowolny klawisz aby zakończyć działanie programu...", false);
                Console.ReadKey();
                Console.Clear();
                Environment.Exit(0);
            }
            connections = new Dictionary<String, NetworkStream>();
            DESDict = new Dictionary<String, byte[]>();
            RSADict = new Dictionary<String, String>();
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            // szukanie adresu ip servera
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }

            tcpListener = new TcpListener(IPAddress.Any, port);
            listenThread = new Thread(new ThreadStart(ListenForClients));
            listenThread.Start();
        }

        // nasłuchiwanie użytkowników
        private void ListenForClients()
        {
            try
            {
                tcpListener.Start(); // rozpoczęcie nasłuchiwania
            }
            catch
            {
                // nie można otworzyć portu
                WriteLog("Port " + port + " jest obecnie używany przez inny proces...", true);
                WriteLog("Wciśnij dowolny klawisz aby zakończyć działanie programu...", false);
                Console.ReadKey();
                Console.Clear();
                Environment.Exit(0);
            }

            WriteLog("Server uruchomiony...", true);
            WriteLog("Server jest gotowy...", false);
            WriteLog("Mój adres IP to: " + localIP + "...", true);
            WriteLog("Nasłuchuję na porcie: " + port + "...", true);
            WriteLog("Czekam na połączenia...\n", false);

            while (true)
            {
                // czekanie na połączenie
                TcpClient mediumAccessPoint = tcpListener.AcceptTcpClient();
                NetworkStream stream = mediumAccessPoint.GetStream();
                byte[] idMessage = new byte[1024]; // bufor dla wiadomości przychodzących
                int bytesRead;
                try
                {
                    bytesRead = stream.Read(idMessage, 0, 1024); // otrzymana wiadomość
                }
                catch
                {
                    // bład podczas odbioru danych
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.White;
                WriteLog("Nawiązano połączenie: " +
                        IPAddress.Parse(((IPEndPoint)mediumAccessPoint.Client.RemoteEndPoint).Address.ToString()), true);

                ASCIIEncoding ascii = new ASCIIEncoding(); // dekoder ascii
                String decrypted = null; // odszyfrowana wiadomość
                try
                {
                    decrypted = DecryptRSA(Convert.ToBase64String(idMessage, 0, bytesRead),
                        privateKey, ascii); // zdeszyfrowanie wiadomości
                }
                catch
                {
                    // błąd podczas deszyfracji
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    WriteLog("Błąd podczas deszyfrowania wiadomości od: " +
                        IPAddress.Parse(((IPEndPoint)mediumAccessPoint.Client.RemoteEndPoint).Address.ToString()), true);
                    try
                    {
                        stream.Close();
                    }
                    catch { }
                    continue;
                }

                // wydzielenia par argument:wartość
                String[] decryptedSplit = null;
                String[] message = new String[2];
                String[] value = new String[2];
                try
                {
                    decryptedSplit = decrypted.Split(':');
                    for (int i = 0; i < 2; i++)
                    {
                        message[i] = decryptedSplit[i * 2];
                        value[i] = decryptedSplit[i * 2 + 1];
                    }
                    if (!message[0].Equals("USER")) throw new Exception(); // zły argument
                    if (!message[1].Equals("PUBKEY")) throw new Exception(); // zły argument
                }
                catch
                {
                    // wiadomość nie zgadza się z szablonem
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    WriteLog("Nie rozpoznana komenda: " + decrypted, true);
                    try
                    {
                        stream.Close();
                    }
                    catch { }
                    continue;
                }

                // sprawdzienie czy klucz publiczny jest taki sam jak na dysku
                try
                {
                    String key = File.ReadAllLines("users//public-" + value[0] + ".txt")[0]; // wczytanie klucza publicznego
                    if (!value[1].Equals(key)) // sprawdzenie poprawności przesłanego klucza
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        WriteLog("Nieprawidłowy klucz publiczny użytkownika: " + value[0], true);
                        try
                        {
                            stream.Close();
                        }
                        catch { }
                        continue;
                    }
                }
                catch
                {
                    // błąd podczas wczytywania pliku
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    WriteLog("Nie można odnaleźć danych użytkownika: " + value[0], true);
                    try
                    {
                        stream.Close();
                    }
                    catch { }
                    continue;
                }

                // generacja i przekazanie klucza symetrycznego użytkownikowi
                Random random = new Random();
                byte[] DESKey = new byte[24]; // klucz 3DES
                random.NextBytes(DESKey);
                String sendKey = "DESKEY:" + Convert.ToBase64String(DESKey); // string do wysłania
                Console.ForegroundColor = ConsoleColor.White;
                WriteLog("Wygenerowano klucz symetryczny: " + Convert.ToBase64String(DESKey), true);
                stream.Write(Convert.FromBase64String(EncryptRSA(sendKey, value[1], ascii)), 0, 128); // wyslanie klucza użytkownikowi

                // zalogowanie użytkownika
                if (connections.ContainsKey(value[0])) connections.Remove(value[0]);
                connections.Add(value[0], stream); // dodanie loginu
                if (DESDict.ContainsKey(value[0])) DESDict.Remove(value[0]);
                DESDict.Add(value[0], DESKey); // dodanie klucza sesji
                if (RSADict.ContainsKey(value[0])) RSADict.Remove(value[0]);
                RSADict.Add(value[0], value[1]); // dodanie klucza publicznego

                Console.ForegroundColor = ConsoleColor.Green;
                WriteLog("Zalogowano: " + value[0], true);

                Thread clientThread = new Thread(new ParameterizedThreadStart(ReadAndWrite)); // wątek nasłuchujący zalogowanych użytkowników
                clientThread.Start(new Tuple<String, NetworkStream>(value[0], stream));
            }
        }

        // nasłuchiwanie zalogowanych użytkowników
        private void ReadAndWrite(object interfaceInfo)
        {
            NetworkStream clientStream = ((Tuple<String, NetworkStream>)interfaceInfo).Item2;
            String user = ((Tuple<String, NetworkStream>)interfaceInfo).Item1; // nazwa użytkownika
            int bytesRead; // ilość odebranych bajtów
            int buffersize = 102400; // rozmiar bufora dla wiadomości
            byte[] buffer = new byte[buffersize]; // bufor

            // nasłuchiwanie wiadomości od zalogowanego użytkownika
            while (true)
            {
                try
                {
                    bytesRead = clientStream.Read(buffer, 0, buffersize); // wiadomość przychodząca
                    if (bytesRead == 0) break;
                }
                catch
                {
                    break;
                }

                byte[] encrypted = new byte[bytesRead]; // zaszyfrowana wiadomość
                Array.Copy(buffer, encrypted, bytesRead);
                ASCIIEncoding ascii = new ASCIIEncoding(); // dekoder ascii
                UnicodeEncoding unicode = new UnicodeEncoding(); // dekoder unicode

                // odszyfrowanie wiadomości
                String decrypted = null;
                try
                {
                    decrypted = DecryptDES(Convert.ToBase64String(encrypted), DESDict[user]);
                }
                catch
                {
                    // bład podczas deszyfrowania
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    WriteLog("Nie można odszyfrować wiadomości od: " + user, true);
                    continue;
                }

                // wydzielenia par argument:wartość
                String[] decryptedSplit = null;
                String[] message = new String[3];
                String[] value = new String[3];
                bool logout = false;
                try
                {
                    decryptedSplit = decrypted.Split(':');
                    for (int i = 0; i < 3; i++)
                    {

                        message[i] = decryptedSplit[i * 2];
                        value[i] = decryptedSplit[i * 2 + 1];
                    }
                    if (!message[0].Equals("USER")) throw new Exception("Bad Data"); // zły argument
                    if (!message[1].Equals("MESSAGE"))
                    {
                        if (!(message[1].Equals("LOGOUT") && value[1].Equals("LOGOUT")))
                            throw new Exception("Bad Data"); // zły argument
                        else logout = true;

                    }
                    if (!message[2].Equals("SHA2")) throw new Exception("Bad Data"); // zły argument
                }
                catch
                {
                    // wiadomość nie zgadza się z szablonem
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    WriteLog("Nie rozpoznana komenda: " + decrypted, true);
                    continue;
                }

                // Sprwdzenie poprawności skrótu
                try
                {
                    String sign = DecryptRSA(value[2], privateKey, ascii); // odszyfrowanie skróte kluczem prywatnym
                    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                    rsa.KeySize = 1024;
                    rsa.FromXmlString(RSADict[user]);
                    if (!rsa.VerifyData(unicode.GetBytes(message[0] + ":" + value[0] + ":" + message[1] + ":" + value[1]),
                        CryptoConfig.MapNameToOID("SHA256"), Convert.FromBase64String(sign))) // sprawdzenie poprawności skrótu
                        throw new CryptographicException("Bad Data"); // zły skrót przesłanej wiadomości
                }
                catch
                {
                    // zły skrót przesłanej wiadomości
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    WriteLog("Błąd podczas weryfikacji podpisu wiadomości od: " + user, true);
                    continue;
                }

                // wylogowanie użytkownika
                if (logout) break;

                // przekazanie wiadomośći do odbiorcy
                if (user.Equals(value[0])) Thread.Sleep(50);
                try
                {
                    String data = "USER:" + user + ":MESSAGE:" + value[1]; // wiadomość do wysłania
                    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                    rsa.KeySize = 1024;
                    rsa.FromXmlString(privateKey);
                    String sign = Convert.ToBase64String(rsa.SignData(unicode.GetBytes(data),
                        CryptoConfig.MapNameToOID("SHA256"))); // podpisanie wiadomości kluczem prywatnym serwera
                    String encryptedDigest = EncryptRSA(sign, RSADict[value[0]],
                        ascii); // podpisanie wiadomości kluczem publicznym użytkownika
                    byte[] toSend = Convert.FromBase64String(EncryptDES(data + ":SHA2:" + encryptedDigest,
                        DESDict[value[0]])); // dane do wysłania
                    connections[value[0]].Write(toSend, 0, toSend.Length); // wysłanie danych
                    Console.ForegroundColor = ConsoleColor.White;
                    WriteLog("Przekazano wiadomość: " + user + " -> " + value[0], true);
                }
                catch
                {
                    // brak użytkownika w pamięci
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    WriteLog("Nie można przekazać wiadomości: " + user + " -> " + value[0], true);
                    WriteLog("Adresat nie isnieje lub nie jest zalogowany.", false);
                    continue;
                }
            }
            // wylogowanie użytkownika
            try
            {
                connections[user].Close(); // zamknięcie połączenia
            }
            catch { }
            try
            {
                connections.Remove(user); // usunięcie z pamięci połączenie
                RSADict.Remove(user); // usunięcie z pamięci klucza RSA
                DESDict.Remove(user); // usunięcie z pamięci klucza 3DES
            }
            catch { }
            Console.ForegroundColor = ConsoleColor.Red;
            WriteLog("Wylogowano: " + user, true);
        }

        // skracanie stringu z użyciem metody SHA256
        private static String CalculateSHA256(String text, Encoding encoder)
        {
            byte[] buffer = encoder.GetBytes(text); // zakodowanie wiadomości
            SHA256CryptoServiceProvider cryptoTransformSHA256 = new SHA256CryptoServiceProvider(); // skrócenie wiadomości
            return BitConverter.ToString(cryptoTransformSHA256.ComputeHash(buffer)).Replace("-", "");
        }

        // funkcja szyfrująca dane algorytmem RSA
        private String EncryptRSA(String data, String key, Encoding encoder)
        {
            // podzielenie wiadomości na bloki o długości 117 bajtów
            byte[] decryptedMessage = encoder.GetBytes(data); // zdekodowanie wiadomości
            int blocksNumber = (int)Math.Ceiling((double)decryptedMessage.Length / (double)117); // liczba bloków
            byte[][] blocks = new byte[blocksNumber][];
            // wydzieleni bloków danych
            for (int i = 0; i < blocksNumber - 1; i++)
            {
                blocks[i] = new byte[117];
                Array.Copy(decryptedMessage, i * 117, blocks[i], 0, 117);
            }
            blocks[blocksNumber - 1] = new byte[decryptedMessage.Length - (blocksNumber - 1) * 117];
            Array.Copy(decryptedMessage, (blocksNumber - 1) * 117, blocks[blocksNumber - 1], 0,
                decryptedMessage.Length - (blocksNumber - 1) * 117);

            // zaszyfrowanie bloków
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.KeySize = 1024;
            rsa.FromXmlString(key);
            byte[] toReturn = new byte[128 * blocksNumber];
            for (int i = 0; i < blocksNumber; i++)
            {
                Array.Copy(rsa.Encrypt(blocks[i], false), 0, toReturn, i * 128, 128); // skopiowanie bloków do jednej tablicy
            }
            rsa.Dispose();
            return Convert.ToBase64String(toReturn);
        }

        // funkcja deszyfrująca dane algorytmem RSA
        private String DecryptRSA(String data, String key, Encoding encoder)
        {
            // wydzielenie bloków z wiadomości
            byte[] encryptedMessage = Convert.FromBase64String(data); // zdekodowanie wiadomości
            int blocksNumber = encryptedMessage.Length / 128; // liczba bloków
            byte[][] blocks = new byte[blocksNumber][];

            // zdeszyfrowanie bloków
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.KeySize = 1024;
            rsa.FromXmlString(key);
            String toReturn = "";
            for (int i = 0; i < blocksNumber; i++)
            {
                blocks[i] = new byte[128];
                Array.Copy(encryptedMessage, i * 128, blocks[i], 0, 128); // skopiowanie danych do jednego bloku
                toReturn += encoder.GetString(rsa.Decrypt(blocks[i], false)); // odszyfrowanie danych
            }
            rsa.Dispose();
            return toReturn;
        }

        // wypisanie zdarzenia
        private void WriteLog(String text, bool file)
        {
            String message = DateTime.Now.ToLongDateString() + ", " + DateTime.Now.ToLongTimeString() + " - " + text; // wiadomość
            Console.WriteLine(message); // wypisanie wiadomości na ekranie
            // zapisanie do pliku
            if (file)
            {
                try
                {
                    System.IO.StreamWriter log = File.AppendText("log.txt"); // otwarcie pliku do zapisu
                    log.WriteLine(message); // zapis do pliku
                    log.Close(); // zamknięcie pliku
                }
                catch
                {
                    // błąd podczas zapisu do pliku
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(DateTime.Now.ToLongDateString() + ", " + DateTime.Now.ToLongTimeString() + " - " +
                        "Błąd podczas zapisu do pliku z dziennikiem zdarzeń.");
                }
            }
        }

        // funkcja szyfrująca dane algorytmem 3DES
        private String EncryptDES(String data, byte[] key)
        {
            TripleDESCryptoServiceProvider cryptoProvider = new TripleDESCryptoServiceProvider();
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream,
                cryptoProvider.CreateEncryptor(key, key), CryptoStreamMode.Write); // zaszyfrowanie wiadomości
            StreamWriter streamWriter = new StreamWriter(cryptoStream);
            streamWriter.Write(data);
            streamWriter.Flush();
            cryptoStream.FlushFinalBlock();
            streamWriter.Flush();
            return Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
        }

        // funkcja deszyfrująca dane algorytmem 3DES
        private String DecryptDES(String data, byte[] key)
        {
            TripleDESCryptoServiceProvider cryptoProvider = new TripleDESCryptoServiceProvider();
            MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(data));
            CryptoStream cryptoStream = new CryptoStream(memoryStream,
                cryptoProvider.CreateDecryptor(key, key), CryptoStreamMode.Read); // odszyfrowanie wiadomości
            StreamReader streamReader = new StreamReader(cryptoStream);
            return streamReader.ReadToEnd();
        }

        static void Main(String[] args)
        {
            new Server();
        }
    }
}
