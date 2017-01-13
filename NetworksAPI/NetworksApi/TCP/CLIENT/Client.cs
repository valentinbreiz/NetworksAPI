namespace NetworksApi.TCP.CLIENT
{
    using NetworksApi.DATA.ENCRYPTION;
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public class Client
    {
        private TcpClient CLIENT_CONNECTION = null;
        private string CLIENT_NAME = string.Empty;
        private NetworkStream CLIENT_STREAM = null;
        private Thread CLIENT_THREAD = null;
        private bool IS_CLIENT_CONNECTED = false;
        private bool IS_ENCRYPTION_ENABLED;
        private BinaryReader MESSAGE_READER = null;
        private BinaryWriter MESSAGE_WRITER = null;
        private IPAddress SERVER_IP = null;
        private int SERVER_PORT = 0;
        private Thread TransferThread = null;

        public event OnClientConnectedDelegate OnClientConnected;

        public event OnClientConnectingDelegate OnClientConnecting;

        public event OnClientDisconnectedDelegate OnClientDisconnected;

        public event OnClientErrorDelegate OnClientError;

        public event OnClientFileSendingDelegate OnClientFileSending;

        public event OnClientReceivedDelegate OnDataReceived;

        public void Connect()
        {
            if (!this.IS_CLIENT_CONNECTED)
            {
                this.CLIENT_THREAD = new Thread(new ThreadStart(this.PerformConnection));
                this.CLIENT_THREAD.Start();
            }
        }

        public void Disconnect()
        {
            try
            {
                this.CLIENT_CONNECTION.Close();
            }
            catch (Exception)
            {
            }
        }

        private void HandleConnection()
        {
            if (this.IS_ENCRYPTION_ENABLED)
            {
                this.CLIENT_NAME = Encryption.Encrypt(this.CLIENT_NAME);
            }
            this.MESSAGE_WRITER.Write(this.CLIENT_NAME);
            this.IS_CLIENT_CONNECTED = true;
            this.OnClientConnectedFunction(new ClientConnectedArguments("Connected"));
            string data = "";
            while (true)
            {
                try
                {
                    data = this.MESSAGE_READER.ReadString();
                    if (this.IS_ENCRYPTION_ENABLED)
                    {
                        data = Encryption.Decrypt(data);
                    }
                    this.OnDataReceivedFunction(new ClientReceivedArguments(data));
                }
                catch (Exception)
                {
                    break;
                }
            }
            this.IS_CLIENT_CONNECTED = false;
        }

        protected virtual void OnClientConnectedFunction(ClientConnectedArguments e)
        {
            this.OnClientConnected(this, e);
        }

        protected virtual void OnClientConnectingFunction(ClientConnectingArguments e)
        {
            this.OnClientConnecting(this, e);
        }

        protected virtual void OnClientDisconnectedFunction(ClientDisconnectedArguments e)
        {
            this.OnClientDisconnected(this, e);
        }

        protected virtual void OnClientErrorFunction(ClientErrorArguments e)
        {
            this.OnClientError(this, e);
        }

        protected virtual void OnClientFileSendingFunction(ClientFileSendingArguments e)
        {
            this.OnClientFileSending(this, e);
        }

        protected virtual void OnDataReceivedFunction(ClientReceivedArguments e)
        {
            this.OnDataReceived(this, e);
        }

        private void PerformConnection()
        {
            try
            {
                this.CLIENT_CONNECTION = new TcpClient();
                this.OnClientConnectingFunction(new ClientConnectingArguments("Connecting"));
                this.CLIENT_CONNECTION.Connect(new IPEndPoint(this.SERVER_IP, this.SERVER_PORT));
                this.CLIENT_STREAM = this.CLIENT_CONNECTION.GetStream();
                this.MESSAGE_READER = new BinaryReader(this.CLIENT_STREAM);
                this.MESSAGE_WRITER = new BinaryWriter(this.CLIENT_STREAM);
                this.HandleConnection();
                this.MESSAGE_WRITER.Close();
                this.MESSAGE_READER.Close();
                this.CLIENT_STREAM.Close();
                this.CLIENT_CONNECTION.Close();
                this.OnClientDisconnectedFunction(new ClientDisconnectedArguments("Disconnected"));
            }
            catch (Exception exception)
            {
                this.OnClientErrorFunction(new ClientErrorArguments("Client Error", exception.ToString()));
            }
        }

        public void Send(string Data)
        {
            if (!this.IS_CLIENT_CONNECTED)
            {
                throw new Exception("Client Is Not Connected");
            }
            if (this.IS_ENCRYPTION_ENABLED)
            {
                Data = Encryption.Encrypt(Data);
            }
            this.MESSAGE_WRITER.Write(Data);
        }

        public void SendFile(string FilePath)
        {
            if (!this.IS_CLIENT_CONNECTED)
            {
                throw new Exception("Client Must Connect First");
            }
            if (this.TransferThread == null)
            {
                this.TransferThread = new Thread(new ParameterizedThreadStart(this.TransferFile));
                this.TransferThread.Start(FilePath);
            }
            else if (!this.TransferThread.IsAlive)
            {
                this.TransferThread = new Thread(new ParameterizedThreadStart(this.TransferFile));
                this.TransferThread.Start(FilePath);
            }
            else
            {
                this.OnClientErrorFunction(new ClientErrorArguments("File Sending In Progress Please Wait", "File Sending In Progress Please Wait"));
            }
        }

        private void TransferFile(object path)
        {
            byte num3;
            string data = ">>FILE>>FILE>>FILE<<";
            FileStream stream = new FileStream((string) path, FileMode.Open, FileAccess.Read);
            long length = stream.Length;
            long num2 = length;
            string str2 = ((string) path).Remove(0, ((string) path).LastIndexOf('\\') + 1);
            if (this.IS_ENCRYPTION_ENABLED)
            {
                data = Encryption.Encrypt(data);
            }
            this.MESSAGE_WRITER.Write(data);
            this.MESSAGE_WRITER.Write(str2);
            this.MESSAGE_WRITER.Write(length.ToString());
            byte[] buffer = new byte[100];
            this.OnClientFileSendingFunction(new ClientFileSendingArguments(num2, length));
            while (length > 100L)
            {
                num3 = 0;
                while (num3 < 100)
                {
                    buffer[num3] = (byte) stream.ReadByte();
                    num3 = (byte) (num3 + 1);
                }
                this.MESSAGE_WRITER.Write(buffer, 0, 100);
                length -= 100L;
                this.OnClientFileSendingFunction(new ClientFileSendingArguments(num2, length));
            }
            for (num3 = 0; num3 < length; num3 = (byte) (num3 + 1))
            {
                buffer[num3] = (byte) stream.ReadByte();
            }
            this.MESSAGE_WRITER.Write(buffer, 0, (byte) length);
            stream.Close();
            this.OnClientFileSendingFunction(new ClientFileSendingArguments(num2, length));
        }

        public string ClientName
        {
            get
            {
                return this.CLIENT_NAME;
            }
            set
            {
                this.CLIENT_NAME = value;
            }
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

        public bool IsConnected =>
            this.IS_CLIENT_CONNECTED;

        public string ServerIp
        {
            get
            {

                return this.SERVER_IP.ToString();
            }
            set
            {
                this.SERVER_IP = IPAddress.Parse(value);
            }
        }

        public string ServerPort
        {
            get
            {
                return this.SERVER_PORT.ToString();
            }
            set
            {
                this.SERVER_PORT = int.Parse(value);
            }
        }
    }
}

