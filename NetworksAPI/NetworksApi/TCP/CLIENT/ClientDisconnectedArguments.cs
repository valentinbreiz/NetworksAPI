namespace NetworksApi.TCP.CLIENT
{
    using System;

    public class ClientDisconnectedArguments : EventArgs
    {
        private string EVENT_MESSAGE;

        public ClientDisconnectedArguments(string EVENT_MESSAGE)
        {
            this.EVENT_MESSAGE = EVENT_MESSAGE;
        }

        public string EventMessage =>
            this.EVENT_MESSAGE;
    }
}

