namespace NetworksApi.TCP.CLIENT
{
    using System;

    public class ClientReceivedArguments : EventArgs
    {
        private string DATA;

        public ClientReceivedArguments(string DATA)
        {
            this.DATA = DATA;
        }

        public string ReceivedData =>
            this.DATA;
    }
}

