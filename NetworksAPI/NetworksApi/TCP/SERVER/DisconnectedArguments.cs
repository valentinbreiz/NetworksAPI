namespace NetworksApi.TCP.SERVER
{
    using System;
    using System.Collections.Generic;

    public class DisconnectedArguments : EventArgs
    {
        private string CLIENT_IP;
        private string CLIENT_NAME;
        private List<string> LIST_OF_CLIENTS;
        private List<string> LIST_OF_IP_ADDRESSES;

        public DisconnectedArguments(string CLIENT_NAME, string CLIENT_IP, List<string> LIST_OF_CLIENTS, List<string> LIST_OF_IP_ADDRESSES)
        {
            this.CLIENT_NAME = CLIENT_NAME;
            this.CLIENT_IP = CLIENT_IP;
            this.LIST_OF_CLIENTS = LIST_OF_CLIENTS;
            this.LIST_OF_IP_ADDRESSES = LIST_OF_IP_ADDRESSES;
        }

        public string Ip =>
            this.CLIENT_IP;

        public List<string> ListOfClients =>
            this.LIST_OF_CLIENTS;

        public List<string> ListOfIpAddresses =>
            this.LIST_OF_IP_ADDRESSES;

        public string Name =>
            this.CLIENT_NAME;
    }
}

