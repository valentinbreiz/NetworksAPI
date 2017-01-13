namespace NetworksApi.TCP.SERVER
{
    using NetworksApi.DATA.ENCRYPTION;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public class Server
    {
        private Socket ACCEPTED_SOCKET = null;
        private List<string> CLIENT_NAMES = null;
        private TcpListener CONNECTION_LISTENER = null;
        private Hashtable CONNECTIONS_TABLE = null;
        private string FILES_LOCATION_PATH;
        private bool IS_ENCRYPTION_ENABLED = false;
        private List<string> LIST_OF_IP_ADDRESSES = null;
        private Thread LISTENING_THREAD = null;
        private int NUMBER_OF_CONNECTIONS = 0;
        private IPAddress SERVER_IP;
        private int SERVER_PORT;

        public event OnConnectedDelegate OnClientConnected;

        public event OnDisconnectedDelegate OnClientDisconnected;

        public event OnReceivedDelegate OnDataReceived;

        public event OnErrorDelegate OnServerError;

        public Server(string LISTENING_IP_ADDRESS, string LISTENING_PORT)
        {
            this.SERVER_IP = IPAddress.Parse(LISTENING_IP_ADDRESS);
            this.SERVER_PORT = int.Parse(LISTENING_PORT);
        }

        public void BroadCast(string Data)
        {
            foreach (DictionaryEntry entry in this.CONNECTIONS_TABLE)
            {
                NetworkStream output = new NetworkStream((Socket) entry.Key);
                BinaryWriter writer = new BinaryWriter(output);
                if (this.IS_ENCRYPTION_ENABLED)
                {
                    Data = Encryption.Encrypt(Data);
                }
                writer.Write(Data);
                writer.Close();
                output.Close();
            }
        }

        public void DisconnectClient(string ClientName)
        {
            foreach (DictionaryEntry entry in this.CONNECTIONS_TABLE)
            {
                if (((string) entry.Value) == ClientName)
                {
                    try
                    {
                        ((Socket) entry.Key).Close();
                        break;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private void HandleConnection(object SOCKET)
        {
            Socket socket = (Socket) SOCKET;
            NetworkStream input = new NetworkStream(socket);
            BinaryReader reader = new BinaryReader(input);
            string data = reader.ReadString();
            if (this.IS_ENCRYPTION_ENABLED)
            {
                data = Encryption.Decrypt(data);
            }
            string item = ((IPEndPoint) socket.RemoteEndPoint).Address.ToString();
            string str3 = "";
            this.CONNECTIONS_TABLE.Add(socket, data);
            this.CLIENT_NAMES.Add(data);
            this.LIST_OF_IP_ADDRESSES.Add(item);
            this.NUMBER_OF_CONNECTIONS++;
            this.OnClientConnectedFunction(new ConnectedArguments(data, item, this.CLIENT_NAMES, this.LIST_OF_IP_ADDRESSES));
            while (true)
            {
                try
                {
                    str3 = reader.ReadString();
                    if (this.IS_ENCRYPTION_ENABLED)
                    {
                        str3 = Encryption.Decrypt(str3);
                    }
                    if (str3.Contains(">>FILE>>FILE>>FILE<<"))
                    {
                        byte[] buffer;
                        string str4 = reader.ReadString();
                        long num = long.Parse(reader.ReadString());
                        FileStream stream2 = new FileStream(this.FILES_LOCATION_PATH + @"\" + str4, FileMode.Create, FileAccess.Write);
                        while (num > 100L)
                        {
                            buffer = reader.ReadBytes(100);
                            stream2.Write(buffer, 0, 100);
                            num -= 100L;
                        }
                        buffer = reader.ReadBytes((int) num);
                        stream2.Write(buffer, 0, (int) num);
                        stream2.Close();
                    }
                    else
                    {
                        this.OnDataReceivedFunction(new ReceivedArguments(data, item, this.CLIENT_NAMES, this.LIST_OF_IP_ADDRESSES, str3));
                    }
                }
                catch (Exception)
                {
                    break;
                }
            }
            reader.Close();
            input.Close();
            this.CONNECTIONS_TABLE.Remove(socket);
            this.CLIENT_NAMES.Remove(data);
            this.LIST_OF_IP_ADDRESSES.Remove(item);
            socket.Close();
            this.NUMBER_OF_CONNECTIONS--;
            this.OnClientDisconnectedFunction(new DisconnectedArguments(data, item, this.CLIENT_NAMES, this.LIST_OF_IP_ADDRESSES));
        }

        protected virtual void OnClientConnectedFunction(ConnectedArguments e)
        {
            this.OnClientConnected(this, e);
        }

        protected virtual void OnClientDisconnectedFunction(DisconnectedArguments e)
        {
            this.OnClientDisconnected(this, e);
        }

        protected virtual void OnDataReceivedFunction(ReceivedArguments e)
        {
            this.OnDataReceived(this, e);
        }

        protected virtual void OnServerErrorFunction(ErrorArguments e)
        {
            this.OnServerError(this, e);
        }

        private void RunServer()
        {
            try
            {

                this.CONNECTION_LISTENER = new TcpListener(this.SERVER_IP, this.SERVER_PORT);
                this.CONNECTION_LISTENER.Start();
                goto Label_0059;
            Label_0027:
                this.ACCEPTED_SOCKET = this.CONNECTION_LISTENER.AcceptSocket();
                new Thread(new ParameterizedThreadStart(this.HandleConnection)).Start(this.ACCEPTED_SOCKET);
            Label_0059:

                goto Label_0027;
            }
            catch (Exception exception)
            {
                this.OnServerErrorFunction(new ErrorArguments("Server Error", exception.ToString()));
            }
        }

        public void SendTo(string ClientName, string Data)
        {
            foreach (DictionaryEntry entry in this.CONNECTIONS_TABLE)
            {
                if (((string) entry.Value) == ClientName)
                {
                    NetworkStream output = new NetworkStream((Socket) entry.Key);
                    BinaryWriter writer = new BinaryWriter(output);
                    if (this.IS_ENCRYPTION_ENABLED)
                    {
                        Data = Encryption.Encrypt(Data);
                    }
                    writer.Write(Data);
                    writer.Close();
                    output.Close();
                    break;
                }
            }
        }

        public void Start()
        {
            this.NUMBER_OF_CONNECTIONS = 0;
            this.CONNECTIONS_TABLE = new Hashtable();
            this.CLIENT_NAMES = new List<string>();
            this.LIST_OF_IP_ADDRESSES = new List<string>();
            this.LISTENING_THREAD = new Thread(new ThreadStart(this.RunServer));
            this.LISTENING_THREAD.Start();
        }

        public void Stop()
        {
            try
            {
                this.CONNECTION_LISTENER.Stop();
            }
            catch (Exception)
            {
            }
            try
            {
                this.ACCEPTED_SOCKET.Close();
            }
            catch (Exception)
            {
            }
            foreach (DictionaryEntry entry in this.CONNECTIONS_TABLE)
            {
                try
                {
                    ((Socket) entry.Key).Close();
                }
                catch (Exception)
                {
                }
            }
            this.LIST_OF_IP_ADDRESSES.Clear();
            this.CLIENT_NAMES.Clear();
            this.CONNECTIONS_TABLE.Clear();
        }

        public bool EncryptionEnabled
        {
            get
            {
                return this.IS_ENCRYPTION_ENABLED;
            }
            set
            {
                this.IS_ENCRYPTION_ENABLED = value;
            }
        }

        public string FilesPath
        {
            get
            {
                return this.FILES_LOCATION_PATH;
            }
            set
            {
                this.FILES_LOCATION_PATH = value;
            }
        }

        public int NumberOfConnections =>
            this.NUMBER_OF_CONNECTIONS;
    }
}

