using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
namespace Client
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
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
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.messagesGroupBox = new System.Windows.Forms.GroupBox();
            this.DESTextBox = new System.Windows.Forms.TextBox();
            this.messagesRichTextBox = new System.Windows.Forms.RichTextBox();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.loginButton = new System.Windows.Forms.Button();
            this.loginTextBox = new System.Windows.Forms.TextBox();
            this.loginLable = new System.Windows.Forms.Label();
            this.sendTabPage = new System.Windows.Forms.TabPage();
            this.userLabel = new System.Windows.Forms.Label();
            this.userTextBox = new System.Windows.Forms.TextBox();
            this.enterCheckBox = new System.Windows.Forms.CheckBox();
            this.messageTextBox = new System.Windows.Forms.TextBox();
            this.sendButton = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.PassLable = new System.Windows.Forms.Label();
            this.passTextBox = new System.Windows.Forms.TextBox();
            this.privateTextBox = new System.Windows.Forms.TextBox();
            this.statusStrip.SuspendLayout();
            this.messagesGroupBox.SuspendLayout();
            this.sendTabPage.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 505);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(534, 22);
            this.statusStrip.TabIndex = 3;
            this.statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(73, 17);
            this.toolStripStatusLabel.Text = "Zaloguj się...";
            // 
            // messagesGroupBox
            // 
            this.messagesGroupBox.Controls.Add(this.privateTextBox);
            this.messagesGroupBox.Controls.Add(this.DESTextBox);
            this.messagesGroupBox.Controls.Add(this.messagesRichTextBox);
            this.messagesGroupBox.Location = new System.Drawing.Point(12, 32);
            this.messagesGroupBox.Name = "messagesGroupBox";
            this.messagesGroupBox.Size = new System.Drawing.Size(510, 349);
            this.messagesGroupBox.TabIndex = 5;
            this.messagesGroupBox.TabStop = false;
            this.messagesGroupBox.Text = "Wiadomości";
            // 
            // DESTextBox
            // 
            this.DESTextBox.Location = new System.Drawing.Point(16, 288);
            this.DESTextBox.Name = "DESTextBox";
            this.DESTextBox.Size = new System.Drawing.Size(478, 20);
            this.DESTextBox.TabIndex = 13;
            this.DESTextBox.Visible = false;
            this.DESTextBox.TextChanged += new System.EventHandler(this.DESTextBox_TextChanged);
            // 
            // messagesRichTextBox
            // 
            this.messagesRichTextBox.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.messagesRichTextBox.Location = new System.Drawing.Point(6, 19);
            this.messagesRichTextBox.Name = "messagesRichTextBox";
            this.messagesRichTextBox.ReadOnly = true;
            this.messagesRichTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.messagesRichTextBox.Size = new System.Drawing.Size(498, 324);
            this.messagesRichTextBox.TabIndex = 6;
            this.messagesRichTextBox.Text = "";
            this.messagesRichTextBox.TextChanged += new System.EventHandler(this.MessagesRichTextBox_TextChanged);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 6);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(32, 19);
            // 
            // loginButton
            // 
            this.loginButton.Location = new System.Drawing.Point(447, 12);
            this.loginButton.Name = "loginButton";
            this.loginButton.Size = new System.Drawing.Size(75, 23);
            this.loginButton.TabIndex = 4;
            this.loginButton.Text = "Zaloguj";
            this.loginButton.UseVisualStyleBackColor = true;
            this.loginButton.Click += new System.EventHandler(this.LoginButton_Click);
            // 
            // loginTextBox
            // 
            this.loginTextBox.Location = new System.Drawing.Point(179, 14);
            this.loginTextBox.Name = "loginTextBox";
            this.loginTextBox.Size = new System.Drawing.Size(100, 20);
            this.loginTextBox.TabIndex = 1;
            // 
            // loginLable
            // 
            this.loginLable.AutoSize = true;
            this.loginLable.Location = new System.Drawing.Point(137, 17);
            this.loginLable.Name = "loginLable";
            this.loginLable.Size = new System.Drawing.Size(36, 13);
            this.loginLable.TabIndex = 0;
            this.loginLable.Text = "Login:";
            // 
            // sendTabPage
            // 
            this.sendTabPage.Controls.Add(this.userLabel);
            this.sendTabPage.Controls.Add(this.userTextBox);
            this.sendTabPage.Controls.Add(this.enterCheckBox);
            this.sendTabPage.Controls.Add(this.messageTextBox);
            this.sendTabPage.Controls.Add(this.sendButton);
            this.sendTabPage.Location = new System.Drawing.Point(4, 22);
            this.sendTabPage.Name = "sendTabPage";
            this.sendTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.sendTabPage.Size = new System.Drawing.Size(490, 86);
            this.sendTabPage.TabIndex = 0;
            this.sendTabPage.Text = "Wyślij Tekst";
            this.sendTabPage.UseVisualStyleBackColor = true;
            // 
            // userLabel
            // 
            this.userLabel.AutoSize = true;
            this.userLabel.Location = new System.Drawing.Point(6, 62);
            this.userLabel.Name = "userLabel";
            this.userLabel.Size = new System.Drawing.Size(52, 13);
            this.userLabel.TabIndex = 9;
            this.userLabel.Text = "Wyślij do:";
            // 
            // userTextBox
            // 
            this.userTextBox.Location = new System.Drawing.Point(64, 57);
            this.userTextBox.Name = "userTextBox";
            this.userTextBox.Size = new System.Drawing.Size(100, 20);
            this.userTextBox.TabIndex = 10;
            // 
            // enterCheckBox
            // 
            this.enterCheckBox.AutoSize = true;
            this.enterCheckBox.Checked = true;
            this.enterCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.enterCheckBox.Location = new System.Drawing.Point(196, 61);
            this.enterCheckBox.Name = "enterCheckBox";
            this.enterCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.enterCheckBox.Size = new System.Drawing.Size(207, 17);
            this.enterCheckBox.TabIndex = 11;
            this.enterCheckBox.Text = "Przycisk \"ENTER\" wysyła wiadomość";
            this.enterCheckBox.UseVisualStyleBackColor = true;
            this.enterCheckBox.CheckedChanged += new System.EventHandler(this.EnterCheckBox_CheckedChanged);
            // 
            // messageTextBox
            // 
            this.messageTextBox.Location = new System.Drawing.Point(6, 6);
            this.messageTextBox.Multiline = true;
            this.messageTextBox.Name = "messageTextBox";
            this.messageTextBox.Size = new System.Drawing.Size(478, 45);
            this.messageTextBox.TabIndex = 8;
            this.messageTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MessageTextBox_KeyDown);
            // 
            // sendButton
            // 
            this.sendButton.Location = new System.Drawing.Point(409, 57);
            this.sendButton.Name = "sendButton";
            this.sendButton.Size = new System.Drawing.Size(75, 23);
            this.sendButton.TabIndex = 12;
            this.sendButton.Text = "Wyślij";
            this.sendButton.UseVisualStyleBackColor = true;
            this.sendButton.Click += new System.EventHandler(this.SendButton_Click);
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.sendTabPage);
            this.tabControl.Enabled = false;
            this.tabControl.Location = new System.Drawing.Point(18, 387);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(498, 112);
            this.tabControl.TabIndex = 7;
            // 
            // PassLable
            // 
            this.PassLable.AutoSize = true;
            this.PassLable.Location = new System.Drawing.Point(285, 17);
            this.PassLable.Name = "PassLable";
            this.PassLable.Size = new System.Drawing.Size(39, 13);
            this.PassLable.TabIndex = 2;
            this.PassLable.Text = "Hasło:";
            // 
            // passTextBox
            // 
            this.passTextBox.Location = new System.Drawing.Point(330, 14);
            this.passTextBox.Name = "passTextBox";
            this.passTextBox.PasswordChar = '•';
            this.passTextBox.Size = new System.Drawing.Size(100, 20);
            this.passTextBox.TabIndex = 3;
            // 
            // privateTextBox
            // 
            this.privateTextBox.Location = new System.Drawing.Point(16, 314);
            this.privateTextBox.Name = "privateTextBox";
            this.privateTextBox.Size = new System.Drawing.Size(478, 20);
            this.privateTextBox.TabIndex = 14;
            this.privateTextBox.Visible = false;
            this.privateTextBox.TextChanged += new System.EventHandler(this.privateTextBox_TextChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(534, 527);
            this.Controls.Add(this.passTextBox);
            this.Controls.Add(this.PassLable);
            this.Controls.Add(this.loginLable);
            this.Controls.Add(this.loginButton);
            this.Controls.Add(this.loginTextBox);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.messagesGroupBox);
            this.Controls.Add(this.statusStrip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Client";
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.messagesGroupBox.ResumeLayout(false);
            this.messagesGroupBox.PerformLayout();
            this.sendTabPage.ResumeLayout(false);
            this.sendTabPage.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.GroupBox messagesGroupBox;
        private System.Windows.Forms.RichTextBox messagesRichTextBox;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.Button loginButton;
        private System.Windows.Forms.TextBox loginTextBox;
        private System.Windows.Forms.Label loginLable;
        private System.Windows.Forms.TabPage sendTabPage;
        private System.Windows.Forms.CheckBox enterCheckBox;
        private System.Windows.Forms.TextBox messageTextBox;
        private System.Windows.Forms.Button sendButton;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.Label PassLable;
        private System.Windows.Forms.TextBox passTextBox;
        private System.Windows.Forms.Label userLabel;
        private System.Windows.Forms.TextBox userTextBox;

        private String configFile = "config.txt"; // nazwa pliku z konfiguracją
        private String ip; // adres IP serwera
        private int port; // port na którym nasłuchuję serwer
        private bool enter = true; // enter wysyła wiadomość
        private String login; // login użytkownika
        private bool logged = false;  // czy zalogowany
        private TcpClient tcpClient; // połączenie z serwerem
        private int bufferSize = 102400; // bufor dla danych przychodzących
        private byte[] sessionDES; // klucz szyfrujący DES
        private String serverPublicRSA; // publiczny klucz RSA serwera
        private String userPrivateRSA; // prywatny klucz RSA użytkownika
        private String userPublicRSA;
        private TextBox DESTextBox;
        private TextBox privateTextBox; // publiczny klucz RSA użytkownika
    }
}

