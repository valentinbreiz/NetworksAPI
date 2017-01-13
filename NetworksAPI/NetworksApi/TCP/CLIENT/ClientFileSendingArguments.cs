namespace NetworksApi.TCP.CLIENT
{
    using System;

    public class ClientFileSendingArguments : EventArgs
    {
        private long BYTES_LEFT;
        private long FILE_SIZE;
        private int PERCENTAGE_SENT;
        private long SENT_SIZE;

        public ClientFileSendingArguments(long FILE_SIZE, long SENT_SIZE)
        {
            this.FILE_SIZE = FILE_SIZE;
            this.SENT_SIZE = SENT_SIZE;
            this.BYTES_LEFT = FILE_SIZE - SENT_SIZE;
            this.PERCENTAGE_SENT = (int) Math.Ceiling((double) ((this.BYTES_LEFT * 100f) / ((float) FILE_SIZE)));
        }

        public long BytesLeft =>
            this.BYTES_LEFT;

        public long FileSize =>
            this.FILE_SIZE;

        public int TransferPercentage =>
            this.PERCENTAGE_SENT;
    }
}

