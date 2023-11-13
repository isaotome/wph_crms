using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Configuration;
using System.Net;
using System.Net.Sockets;

namespace CrmsDao {
    public class SendMail {
        //SMTPサーバー名
        private string smtpServer = ConfigurationManager.AppSettings["SmtpServer"];

        //POPサーバー名
        public string popServer = ConfigurationManager.AppSettings["PopServer"];

        //送信元アドレス
        private string senderAddress = ConfigurationManager.AppSettings["SenderAddress"];
        
        //SMTPのポート番号
        private int smtpPort = Int32.Parse(string.IsNullOrEmpty(ConfigurationManager.AppSettings["SmtpPort"]) ? "25" : ConfigurationManager.AppSettings["SmtpPort"]);

        //SMTP認証の有無
        private bool smtpCredential = !string.IsNullOrEmpty(ConfigurationManager.AppSettings["SmtpCredential"]) && ConfigurationManager.AppSettings["SmtpCredential"].Equals("true");

        //送信元ID(SMTP認証の場合またはPOP Before SMTPの場合)
        private string senderUserId = ConfigurationManager.AppSettings["SenderUserId"];

        //送信元パスワード(SMTP認証の場合またはPOP Before SMTPの場合)
        private string senderPassword = ConfigurationManager.AppSettings["SenderPassword"];

        //SSLの有無
        private bool smtpSsl = !string.IsNullOrEmpty(ConfigurationManager.AppSettings["SmtpSSL"]) && ConfigurationManager.AppSettings["SmtpSSL"].Equals("true");

        //POP Before SMTPの有無
        private bool popBeforeSmtp = !string.IsNullOrEmpty(ConfigurationManager.AppSettings["PopBeforeSmtp"]) && ConfigurationManager.AppSettings["PopBeforeSmtp"].Equals("true");

        //POPのポート番号
        private int popPort = Int32.Parse(string.IsNullOrEmpty(ConfigurationManager.AppSettings["PopPort"]) ? "25" : ConfigurationManager.AppSettings["PopPort"]);
        
        private TcpClient client;
        private NetworkStream stream;

        /// <summary>
        /// コンストラクタ
        /// POP Before SMTPの場合、ここでログインしておく
        /// </summary>
        public SendMail() {
            if (popBeforeSmtp) {
                PopCheck();
            }
        }

        /// <summary>
        /// メール送信
        /// </summary>
        /// <param name="title">件名</param>
        /// <param name="address">送信先アドレス</param>
        /// <param name="body">本文</param>
        public void Send(string title, string address, string body) {
            if(string.IsNullOrEmpty(address)) {
                return;
            }
            
            try {
                SmtpClient smtp = new SmtpClient();
                smtp.Host = smtpServer;
                smtp.Port = smtpPort;
                if (smtpCredential)
                {
                    smtp.Credentials = new NetworkCredential(senderAddress, senderPassword);
                }
                smtp.EnableSsl = smtpSsl;
                MailMessage oMsg = new MailMessage(senderAddress, address, title, body);
                smtp.Send(oMsg);
            } catch(Exception e){
                ////Mod 2014/08/13 arc amii エラーログ対応 Exceptionを設定し、ログ出力を行うよう修正
                OutputLogData.exLog = e;
                OutputLogData.procName = "メール送信";
                throw e;
            }
        }
        private bool PopCheck() {
            try
            {
                login();
                logout();
            }
            catch (Exception e)
            {
                ////Mod 2014/08/13 arc amii エラーログ対応 Exceptionを設定し、ログ出力を行うよう修正
                OutputLogData.exLog = e;
                OutputLogData.procName = "サーバー接続";
                throw e;
            }
          
            return true;

        }

        private void login() {
            //TcpClientの作成
            this.client = new TcpClient();
            //タイムアウトの設定
            this.client.ReceiveTimeout = 10000;
            this.client.SendTimeout = 10000;

            string msg = "";

            try {
                //サーバーに接続
                client.Connect(popServer, popPort);
                //ストリームの取得
                this.stream = client.GetStream();
                //受信
                msg = ReceiveData(stream);

                //USERの送信
                SendData(this.stream, "USER " + senderUserId + "\r\n");
                //受信
                msg = ReceiveData(this.stream);

                //PASSの送信
                SendData(this.stream, "PASS " + senderPassword + "\r\n");
                //受信
                msg = ReceiveData(this.stream);
            } catch(Exception e) {
                throw new Exception(e.ToString());
            }
        }

        private void logout() {
            string msg = "";
            try {
                //QUITの送信
                SendData(this.stream, "QUIT\r\n");
                //受信
                msg = ReceiveData(this.stream);
            } catch {
                throw;
            } finally {
                //切断
                this.client.Close();
            }
        }

        //データを受信する
        private string ReceiveData(
            NetworkStream stream,
            bool multiLines,
            int bufferSize,
            Encoding enc) {
            byte[] data = new byte[bufferSize];
            int len;
            string msg = "";
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            //すべて受信する
            //(無限ループに陥る恐れあり)
            do {
                //受信
                len = stream.Read(data, 0, data.Length);
                ms.Write(data, 0, len);
                //文字列に変換する
                msg = enc.GetString(ms.ToArray());
            }
            while(stream.DataAvailable ||
                ((!multiLines || msg.StartsWith("-ERR")) &&
                !msg.EndsWith("\r\n")) ||
                (multiLines && !msg.EndsWith("\r\n.\r\n")));

            ms.Close();

            //"-ERR"を受け取った時は例外をスロー
            if (msg.StartsWith("-ERR"))
                //throw new ApplicationException("Received Error");
                throw new ApplicationException(msg);
            //表示
            Console.Write("S: " + msg);

            return msg;
        }
        private string ReceiveData(NetworkStream stream,
            bool multiLines,
            int bufferSize) {
            return ReceiveData(stream, multiLines, bufferSize,
                Encoding.GetEncoding(50220));
        }
        private string ReceiveData(NetworkStream stream,
            bool multiLines) {
            return ReceiveData(stream, multiLines, 256);
        }
        private string ReceiveData(NetworkStream stream) {
            return ReceiveData(stream, false);
        }

        //データを送信する
        private void SendData(NetworkStream stream,
            string msg,
            Encoding enc) {
            //byte型配列に変換
            byte[] data = enc.GetBytes(msg);
            //送信
            stream.Write(data, 0, data.Length);

            //表示
            Console.Write("C: " + msg);
        }
        private void SendData(NetworkStream stream,
            string msg) {
            SendData(stream, msg, Encoding.ASCII);
        }

    }
}
