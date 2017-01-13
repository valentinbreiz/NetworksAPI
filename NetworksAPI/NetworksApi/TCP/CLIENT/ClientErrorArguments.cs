namespace NetworksApi.TCP.CLIENT
{
    using System;

    public class ClientErrorArguments : EventArgs
    {
        private string ERROR_MSG;
        private string THROWN_EXCEPTION;

        public ClientErrorArguments(string ERROR_MSG, string THROWN_EXCEPTION)
        {
            this.ERROR_MSG = ERROR_MSG;
            this.THROWN_EXCEPTION = THROWN_EXCEPTION;
        }

        public string ErrorMessage =>
            this.ERROR_MSG;

        public string Exception =>
            this.THROWN_EXCEPTION;
    }
}

