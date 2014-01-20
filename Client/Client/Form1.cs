using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {
        public Form1(String[] args)
        {
            CheckForIllegalCrossThreadCalls = false; // używanie form w różnych wątkach
            try
            {
                // ustawienie parametrów z pliku (ip, port)
                String[] config = System.IO.File.ReadAllLines(configFile);
                ip = config[0];
                port = Convert.ToInt32(config[1]);
                serverPublicRSA = config[2];
            }
            catch
            {
                // bład podczas odczytu pliku
                MessageBox.Show("Nie można wczytać pliku z konfiguracją!\nProgram zostanie zamknięty.",
                    "Błąd", System.Windows.Forms.MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
            InitializeComponent(); // wczytanie okna
            if (args.Length > 0 && args[0].Equals("-debug"))
            {
                DESTextBox.Visible = true; // debug klucza sesji
                privateTextBox.Visible = true; // debug klucza prywtnego
            }
        }

        // zmiana form na dostosowane do logowania
        private void LoginForms()
        {
            loginTextBox.Enabled = true;
            passTextBox.Enabled = true;
            tabControl.Enabled = false;
            logged = false;
            messagesRichTextBox.Clear();
            loginTextBox.Clear();
            passTextBox.Clear();
            messageTextBox.Clear();
            userTextBox.Clear();
            loginButton.Text = "Zaloguj";
        }

        // zmiana form na dostosowane do wylogowania
        private void LogoutForms()
        {
            loginTextBox.Enabled = false;
            passTextBox.Enabled = false;
            tabControl.Enabled = true;
            logged = true;
            toolStripStatusLabel.Text = "Zostałeś zalogowany...";
            loginButton.Text = "Wyloguj";
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            if (!logged) // zaloguj
            {
                try
                {
                    // wczytanie danych użytkownika z pliku
                    String[] parms = System.IO.File.ReadAllLines("users\\private-" + loginTextBox.Text + ".txt");
                    ASCIIEncoding ascii = new ASCIIEncoding();
                    if (CalculateSHA256(passTextBox.Text, ascii).Equals(parms[0].ToUpper()))
                    {
                        // poprawne login i hasło
                        login = loginTextBox.Text;
                        userPrivateRSA = parms[1];
                        privateTextBox.Text = userPrivateRSA;
                        userPublicRSA = parms[2];
                        LogoutForms();
                        new Thread(ConnectServer).Start();
                    }
                    // złe hasło
                    else throw new IOException();
                }
                catch
                {
                    toolStripStatusLabel.Text = "Login lub/i hasło są nieprawidłowe...";
                }
            }
            else // wyloguj
            {
                Logout();
                try
                {
                    tcpClient.GetStream().Close();
                }
                catch { }
                try
                {
                    tcpClient.Close();
                }
                catch { }
                LoginForms();
                toolStripStatusLabel.Text = "Zostałeś wylogowany...";
            }
        }

        // automatyczne przewijanie tekstu
        private void MessagesRichTextBox_TextChanged(object sender, EventArgs e)
        {
            messagesRichTextBox.SelectionStart = messagesRichTextBox.Text.Length;
            messagesRichTextBox.ScrollToCaret();
        }

        // zmiana atrybuty "klawisz enter wysyła wiadomość"
        private void EnterCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            enter = !enter;
        }

        // wysłanie wiadomości gdy klikniemy enter
        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // sprawdzenie czy kliknięto enter podczas wysyłania wiadomości
            if (e.KeyCode == Keys.Enter & enter)
            {
                sendButton.PerformClick();
                e.SuppressKeyPress = true;
            }
        }

        // Przycisk wysyłania wiadomośći
        private void SendButton_Click(object sender, EventArgs e)
        {
            new Thread(SendMessage).Start();
        }

        // skracanie stringu z użyciem metody SHA256
        private static String CalculateSHA256(String text, Encoding encoder)
        {
            byte[] buffer = encoder.GetBytes(text);
            SHA256CryptoServiceProvider cryptoTransformSHA256 = new SHA256CryptoServiceProvider();
            return BitConverter.ToString(cryptoTransformSHA256.ComputeHash(buffer)).Replace("-", "");
        }

        // łączenie z serwerem
        private void ConnectServer()
        {
            try
            {
                tcpClient = new TcpClient();
                toolStripStatusLabel.Text = "Logowanie...";
                tcpClient.Connect(this.ip, this.port);

                // Wysłanie nazwy użytkownika oraz klucza publicznego do serwera
                ASCIIEncoding ascii = new ASCIIEncoding();
                UnicodeEncoding unicode = new UnicodeEncoding();
                String messageToSend = "USER:" + login + ":" + "PUBKEY:" + userPublicRSA;
                byte[] toSend = Convert.FromBase64String(EncryptRSA(messageToSend, serverPublicRSA, ascii));
                tcpClient.GetStream().Write(toSend, 0, toSend.Length);

                // Odebrania klucza sesji 3DES
                byte[] DESKey = new byte[128];
                tcpClient.GetStream().Read(DESKey, 0, 128);
                String decrypted = DecryptRSA(Convert.ToBase64String(DESKey), userPrivateRSA, ascii);
                if (!decrypted.StartsWith("DESKEY:")) throw new Exception("Incorrect data received");

                // zapisanie klucza do zmiennej
                sessionDES = Convert.FromBase64String(decrypted.Substring(7));
                DESTextBox.Text = Convert.ToBase64String(sessionDES);
                toolStripStatusLabel.Text = "Zostałeś zalogowany...";

                while (true)
                {

                    // Wczytywanie przychodzących wiadomości
                    byte[] buffer = new byte[bufferSize];
                    int bytesRead = tcpClient.GetStream().Read(buffer, 0, bufferSize);
                    byte[] encrypted = new byte[bytesRead];
                    Array.Copy(buffer, encrypted, bytesRead);

                    // odszyfrowanie wiadomości
                    String received = null;
                    try
                    {
                        received = DecryptDES(Convert.ToBase64String(encrypted), sessionDES);
                    }
                    catch
                    {
                        continue;
                    }

                    // wydzielenia par argument, wartość
                    String[] decryptedSplit = null;
                    String[] message = new String[3];
                    String[] value = new String[3];
                    try
                    {
                        decryptedSplit = received.Split(':');
                        for (int i = 0; i < 3; i++)
                        {

                            message[i] = decryptedSplit[i * 2];
                            value[i] = decryptedSplit[i * 2 + 1];
                        }
                        if (!message[0].Equals("USER")) throw new Exception();
                        if (!message[1].Equals("MESSAGE")) throw new Exception();
                        if (!message[2].Equals("SHA2")) throw new Exception();
                    }
                    catch
                    {
                        continue;
                    }

                    // Sprwdzenie poprawności skrótu
                    try
                    {
                        String sign = DecryptRSA(value[2], userPrivateRSA, ascii);
                        RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                        rsa.KeySize = 1024;
                        rsa.FromXmlString(serverPublicRSA);
                        if (!rsa.VerifyData(unicode.GetBytes(message[0] + ":" + value[0] + ":" + message[1] + ":" + value[1]),
                            CryptoConfig.MapNameToOID("SHA256"), Convert.FromBase64String(sign)))
                            throw new Exception("Bad Data");
                    }
                    catch
                    {
                        continue;
                    }

                    messagesRichTextBox.Text += "Odebrana wiadomość od " + value[0] + ":\n" +
                        unicode.GetString(Convert.FromBase64String(value[1])) + "\n\n";
                    toolStripStatusLabel.Text = "Odebrano Wiadomość...";
                }
            }
            catch
            {
                if (logged) toolStripStatusLabel.Text = "Błąd podczas łączenia z serwerem...";
                try
                {
                    tcpClient.GetStream().Close();
                }
                catch { }
                try
                {
                    tcpClient.Close();
                }
                catch { }
                LoginForms();
            }
        }

        // wysłanie wiadomości
        private void SendMessage()
        {
            try
            {
                if (messageTextBox.Text == "" || userTextBox.Text == "")
                {
                    toolStripStatusLabel.Text = "Brak wiadomości lub/i odbiorcy...";
                    return;
                }
                ASCIIEncoding ascii = new ASCIIEncoding();
                UnicodeEncoding unicode = new UnicodeEncoding();

                // utworzenie wiadomości
                String message = "USER:" + userTextBox.Text + ":MESSAGE:" + Convert.ToBase64String(unicode.GetBytes(messageTextBox.Text));

                // podpisanie wiadomości swoim kluczem prywatnym
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.KeySize = 1024;
                rsa.FromXmlString(userPrivateRSA);
                String sign = Convert.ToBase64String(rsa.SignData(unicode.GetBytes(message), CryptoConfig.MapNameToOID("SHA256")));

                // podpisanie wiadomości kluczem publicznym serwera
                String encryptedDigest = EncryptRSA(sign, serverPublicRSA, ascii);

                // zaszyfrowanie wiadomości przy użyciu 3DES
                byte[] toSend = Convert.FromBase64String(EncryptDES(message + ":SHA2:" + encryptedDigest, sessionDES));

                tcpClient.GetStream().Write(toSend, 0, toSend.Length);
                messagesRichTextBox.Text += "Wysłana wiadomość do " + userTextBox.Text + ":\n" + messageTextBox.Text + "\n\n";
                toolStripStatusLabel.Text = "Wysłano wiadomość...";
                messageTextBox.Clear();
            }
            catch
            {
                toolStripStatusLabel.Text = "Wiadomość nie mogła zostać wysłana...";
            }
        }

        // wylogowanie użytkownika
        private void Logout()
        {
            try
            {
                ASCIIEncoding ascii = new ASCIIEncoding();
                UnicodeEncoding unicode = new UnicodeEncoding();

                // utworzenie wiadomości
                String message = "USER:" + login + ":LOGOUT:LOGOUT";

                // podpisanie wiadomości swoim kluczem prywatnym
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.KeySize = 1024;
                rsa.FromXmlString(userPrivateRSA);
                String sign = Convert.ToBase64String(rsa.SignData(unicode.GetBytes(message), CryptoConfig.MapNameToOID("SHA256")));

                // podpisanie wiadomości kluczem publicznym serwera
                String encryptedDigest = EncryptRSA(sign, serverPublicRSA, ascii);

                // zaszyfrowanie wiadomości przy użyciu 3DES
                byte[] toSend = Convert.FromBase64String(EncryptDES(message + ":SHA2:" + encryptedDigest, sessionDES));

                tcpClient.GetStream().Write(toSend, 0, toSend.Length);
            }
            catch
            { }
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


        // zmiana kluczu sesji
        private void DESTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                sessionDES = Convert.FromBase64String(DESTextBox.Text);
            }
            catch { return; }
            toolStripStatusLabel.Text = "Zmieniono klucz sesji...";
        }

        // zmiana klucza prywatnego
        private void privateTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                userPrivateRSA = privateTextBox.Text;
            }
            catch { return; }
            toolStripStatusLabel.Text = "Zmieniono klucz prywatny...";
        }
    }
}