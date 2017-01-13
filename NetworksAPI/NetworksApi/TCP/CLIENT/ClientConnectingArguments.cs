namespace NetworksApi.TCP.CLIENT
{
    using System;

    public class ClientConnectingArguments : EventArgs
    {
        private string EVENT_MESSAGE;

        public ClientConnectingArguments(string EVENT_MESSAGE)
        {
            this.EVENT_MESSAGE = EVENT_MESSAGE;
        }

        public string EventMessage =>
            this.EVENT_MESSAGE;
    }
}

