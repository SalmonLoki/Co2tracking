namespace Co2tracking {
    public interface IDataProcessor {
        int[] decryptData(ref byte[] key, ref byte[] dataBuffer);
		
        bool checkEndOfMessage(ref int[] data);

        bool checkCheckSum(ref int[] data);

        void dataProcessing(ref int[] data);
    }
}