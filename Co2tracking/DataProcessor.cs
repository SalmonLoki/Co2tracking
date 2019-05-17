using System;

namespace Co2tracking
{
    public class DataProcessor : IDataProcessor {
        public int[] decryptData(byte[] key, ref byte[] dataBuffer) {
            var cstate = new[] { 0x48, 0x74, 0x65, 0x6D, 0x70, 0x39, 0x39, 0x65 };
            var shuffle = new[] { 2, 4, 0, 7, 1, 6, 5, 3 };
			
            var phase1 = new[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            for (var counter = 0; counter < shuffle.Length; counter++) {
                phase1[shuffle[counter]] = dataBuffer[counter];
            }    
			
            var phase2 = new[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            for (var i = 0; i < 8; i++) {
                phase2[i] = phase1[i] ^ key[i];
            }
			
            var phase3 = new[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            for (var i = 0; i < 8; i++) {
                phase3[i] = ((phase2[i] >> 3) | (phase2[(i - 1 + 8) % 8] << 5)) & 0xff;
            }
			
            var tmp = new[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            for (var i = 0; i < 8; i++) {
                tmp[i] = ((cstate[i] >> 4) | (cstate[i] << 4)) & 0xff;
            }
			
            var result = new[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            for (var i = 0; i < 8; i++) {
                result[i] = (0x100 + phase3[i] - tmp[i]) & 0xff;
            }
            return result;    
        }

        void swap_char(ref byte[] a,  int i,  int j)
        {
            byte t = a[i];
            a[i] = a[j];
            a[j] = t;
        }
        
        public int[] ecode_buf( byte[] magic_table, ref byte[] buf)
        {
            var result = new[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            
            swap_char(ref buf, 0, 2);
            swap_char(ref buf, 1, 4);
            swap_char(ref buf, 3, 7);
            swap_char(ref buf, 5, 6);

            for (int i = 0; i < 8; ++i)
            {
                buf[i] ^= magic_table[i];
            }

            int tmp = buf[7] << 5;
            result[7] = (buf[6] << 5) | (buf[7] >> 3);
            result[6] = (buf[5] << 5) | (buf[6] >> 3);
            result[5] = (buf[4] << 5) | (buf[5] >> 3);
            result[4] = (buf[3] << 5) | (buf[4] >> 3);
            result[3] = (buf[2] << 5) | (buf[3] >> 3);
            result[2] = (buf[1] << 5) | (buf[2] >> 3);
            result[1] = (buf[0] << 5) | (buf[1] >> 3);
            result[0] = tmp | (buf[0] >> 3);

            string s = "Htemp99e";
            char[] magic_word = s.ToCharArray();
            for (int i = 0; i < 8; ++i)
            {
                result[i] -= (magic_word[i] << 4) | (magic_word[i] >> 4);
            }

            return result;
        }

        public bool checkEndOfMessage(ref int[] data) {
            return data[4] == 0x0d;
        }
		
        public bool checkCheckSum(ref int[] data) {
            return data[0] + data[1] + data[2] == data[3];
        }
		
        private double decodeTemperature(int t) {
            return t * 0.0625 - 273.15;
        }
				
        private void writeHeartbeat() {
            string curTimeLong = DateTime.Now.ToLongTimeString();
            Console.WriteLine(curTimeLong);
        }

        public void dataProcessing(ref int[] data) {
            int Data = (data[1] << 8) + data[2];
			
            switch (data[0]) {
                case 0x50d:					
                    Console.WriteLine("Relative Concentration of CO2: " + Data);
                    writeHeartbeat();
                    break;	
				
                case 0x42d:					
                    Console.WriteLine("Ambient Temperature: " + decodeTemperature(Data));
                    writeHeartbeat();
                    break;
            }
        }    
    }
}