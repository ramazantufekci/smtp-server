/*
 * Created by SharpDevelop.
 * Date: 6.08.2024
 * Time: 13:31
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace smtp
{
	class Program
	{
		public static void Main(string[] args)
		{
            int port = 25;

            // TCP listener olusturma
            
            TcpListener server = new TcpListener(port);

            // Sunucuyu baslatma
            server.Start();
            Console.WriteLine("SMTP sunucusu baslatildi...");

            while (true)
            {
                // Gelen baglantilari kabul etme
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Yeni bir istemci baglandi...");

                // Istemci ile iletisim kurma
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead;

                // Sunucu baglanti mesaji
                string welcomeMessage = "220 localhost Simple SMTP Server\r\n";
                byte[] welcomeBuffer = Encoding.ASCII.GetBytes(welcomeMessage);
                stream.Write(welcomeBuffer, 0, welcomeBuffer.Length);

                // Istemciden gelen verileri okuma
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    string request = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("Istemciden gelen: " + request);

                    // SMTP komutlarina yanit verme
                    if (request.StartsWith("HELO") || request.StartsWith("EHLO"))
                    {
                        string response = "250 Hello\r\n";
                        byte[] responseBuffer = Encoding.ASCII.GetBytes(response);
                        stream.Write(responseBuffer, 0, responseBuffer.Length);
                    }
                    else if (request.StartsWith("MAIL FROM:"))
                    {
                        string response = "250 OK\r\n";
                        byte[] responseBuffer = Encoding.ASCII.GetBytes(response);
                        stream.Write(responseBuffer, 0, responseBuffer.Length);
                    }
                    else if (request.StartsWith("RCPT TO:"))
                    {
                    	string ramazan = request.Trim("RCPT TO:<".ToCharArray());
                    	ramazan = ramazan.Split('@')[0];
                    	Console.WriteLine(ramazan);
                        string response = "250 OK\r\n";
                        byte[] responseBuffer = Encoding.ASCII.GetBytes(response);
                        stream.Write(responseBuffer, 0, responseBuffer.Length);
                    }
                    else if (request.StartsWith("DATA"))
                    {
                        string response = "354 End data with <CR><LF>.<CR><LF>\r\n";
                        byte[] responseBuffer = Encoding.ASCII.GetBytes(response);
                        stream.Write(responseBuffer, 0, responseBuffer.Length);
                    }
                    else if (request.EndsWith("\r\n.\r\n"))
                    {
                        string response = "250 OK: Message accepted for delivery\r\n";
                        byte[] responseBuffer = Encoding.ASCII.GetBytes(response);
                        stream.Write(responseBuffer, 0, responseBuffer.Length);
                    }
                    else if (request.StartsWith("QUIT"))
                    {
                        string response = "221 Bye\r\n";
                        byte[] responseBuffer = Encoding.ASCII.GetBytes(response);
                        stream.Write(responseBuffer, 0, responseBuffer.Length);
                        break;
                    }
                    else
                    {
                        string response = "500 Command not recognized\r\n";
                        byte[] responseBuffer = Encoding.ASCII.GetBytes(response);
                        stream.Write(responseBuffer, 0, responseBuffer.Length);
                    }
                }

                // Istemci baglantisini kapatma
                client.Close();
                Console.WriteLine("Istemci baglantisi kapatildi.");
            }
		}
	}
}
