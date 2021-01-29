using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Muzi_SocketAsyncLib
{
    public class AsyncSocketServer
    {
        IPAddress mIP;
        int mPort;
        TcpListener mServer;
        bool continua;

        List<TcpClient> mClient;
        public AsyncSocketServer()
        {
            mClient = new List<TcpClient>();
        }

        // Mette in ascolto il server
        public async void InizioAscolto(IPAddress ipaddr = null, int port = 23000)
        {
            int count = 0;
            //faccio dei controlli su IPAddress e sulle porte
            if (ipaddr == null)
            {
                //mIP = IPAddress.Any;
                ipaddr = IPAddress.Any;
            }
            if (port < 0 || port > 65535)
            {
                //mPort = 23000;
                port = 23000;
            }

            mIP = ipaddr;//aggiunte
            mPort = port;//aggiunte

            Debug.WriteLine("Avvio il server. IP: {0} - Porta: {1}", mIP.ToString(), mPort.ToString());
            //creare l'oggetto server
            mServer = new TcpListener(mIP, mPort);

            //avviare il server
            if (count == 0)
                mServer.Start();

            continua = true;
            while (continua)
            {
                // mettermi in ascolto
                TcpClient client = await mServer.AcceptTcpClientAsync();
                mClient.Add(client);
                Debug.WriteLine($"Client connessi: {mClient.Count}. Client connesso: {client.Client.RemoteEndPoint}");

                SendToAll();
                RiceviMessaggi(client);
            }

            count++;
        }
        public async void RiceviMessaggi(TcpClient client)
        {
            NetworkStream stream = null;
            StreamReader reader = null;
            try
            {
                stream = client.GetStream();
                reader = new StreamReader(stream);
                char[] buff = new char[512];

                //ricezione effettiva
                while (continua)
                {
                    Debug.WriteLine("Pronto ad ascoltare...");
                    int nBytes = await reader.ReadAsync(buff, 0, buff.Length);
                    if (nBytes == 0)
                    {
                        RemoveClient(client);
                        Debug.WriteLine("Client disconnesso.");
                        break;
                    }
                    string recvMessage = new string(buff, 0, nBytes);
                    Debug.WriteLine("Returned bytes: {0}. Messaggio: {1}", nBytes, recvMessage);

                    Rispondi(client, recvMessage);
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }

        private void Rispondi(TcpClient client, string msg)
        {
            string risposta = "";

            if (msg.ToLower() == "time")
                risposta = DateTime.Now.ToShortTimeString() + "\n";
            else if (msg.ToLower() == "date")
                risposta = DateTime.Now.ToShortDateString() + "\n";
            else
                risposta = "non ho capito, prova a ripetere" + "\n";

            SendToOne(client, risposta);
        }

        private void RemoveClient(TcpClient client)
        {
            if (mClient.Contains(client))
            {
                mClient.Remove(client);
            }
        }

        public void SendToAll()
        {
            try
            {
                //Random rnd = new Random();
                //rnd.Next(5, 6);
                //Thread.Sleep(Convert.ToInt32(rnd) * 1000);

                //byte[] buff = Encoding.ASCII.GetBytes($"sono {Convert.ToInt32(rnd)} secondi che aspetto");

                Thread.Sleep(5000);
                //byte[] buff = Encoding.ASCII.GetBytes($"ciao");
                byte[] buff = Encoding.ASCII.GetBytes($"sono {5} secondi che aspetto un messaggio \n");
                foreach (TcpClient item in mClient)
                    item.GetStream().WriteAsync(buff, 0, buff.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore: {ex.Message}");
            }
        }
        
        public void SendToOne(TcpClient client, string messaggio)
        {
            try
            {
                if (string.IsNullOrEmpty(messaggio))
                    return;

                byte[] buff = Encoding.ASCII.GetBytes(messaggio);
                client.GetStream().WriteAsync(buff, 0, buff.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore: {ex.Message}");
            }
        }

        public void ChiudiConnessione()
        {
            try
            {
                foreach (TcpClient item in mClient)
                {
                    item.Close();
                    RemoveClient(item);
                }
                mServer.Stop();
                mServer = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Errore: {ex.Message}");
            }
        }
    }
}
