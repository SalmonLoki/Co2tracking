using System;
using System.Linq;
using HidSharp;

namespace Co2tracking
{
    class Program
    {
        private static int vendorId = 0x04d9; //
        private static int productId = 0xa052; //

        private static Co2DeviceHandler co2DeviceHandler;
        private static DataProcessor dataProcessor;
        static void Main(string[] args)
        {
            //tryShowAlldevices();
            UseHidSharp();
        }

        private static void tryShowAlldevices()
        {
            var devices = DeviceList.Local;
            devices.Changed += (sender, e) => Console.WriteLine("Device list changed.");
            var allDeviceList = devices.GetAllDevices().ToArray();
            Console.WriteLine("All device list:");
            foreach (Device dev in allDeviceList)
            {
                Console.Write(dev + " @ " + dev.DevicePath+ " : ");
                if (dev is HidDevice)
                {
                    Console.Write("HidDevice");
                    foreach (var serialPort in
                        (((HidDevice)dev).GetSerialPorts()))
                    {
                        Console.Write("    " + serialPort);
                    }
                }
                else
                {
                    Console.Write("not HidDevice ");
                }
                Console.WriteLine();
            }
        }


        private static void deviceLoop(HidStream stream) {
            //the device won't send anything before receiving this packet 
            //var key = new byte[] { 0xc4, 0xc6, 0xc0, 0x92, 0x40, 0x23, 0xdc, 0x96 };
            var key = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            try
            {
                co2DeviceHandler.sendSetFeatureSetupRequest(stream, key);
            }
            catch (Exception ex)
            {
                Console.WriteLine(value: "Unable to send SetFeatureSetupRequest");
                throw ex;
                return;
            }

            while (true)
            {
                byte[] receivedData = co2DeviceHandler.readData(stream);
                if (receivedData.Length == 0) {
                    Console.WriteLine(value: "Unable to read data");
                    break;
                }
                if (receivedData.Length != 9) {
                    Console.WriteLine(value: "transferred amount of bytes != expected bytes amount");
                    break;
                }
                Console.Write("received data: ");
                for (int i = 0; i < receivedData.Length; i++)
                    Console.Write(receivedData[i] + " ");
                Console.WriteLine();
                var temp = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                for (int i = 0; i < 8; i++)
                {
                    temp[i] = receivedData[i+1];
                }
                receivedData = temp;
                
                Console.Write("received data: ");
                for (int i = 0; i < receivedData.Length; i++)
                    Console.Write(receivedData[i] + " ");
                Console.WriteLine();
                
                
                int[] data = dataProcessor.decryptData(key, ref receivedData);
                Console.Write("decrypt data: ");
                for (int i = 0; i < data.Length; i++)
                    Console.Write(data[i] + " ");
                Console.WriteLine();        

                if (!dataProcessor.checkEndOfMessage(ref data)) {
                    Console.WriteLine(value: "Unexpected data from device");
                    continue;
                }

                if (!dataProcessor.checkCheckSum(ref data)) {
                    Console.WriteLine(value: "checksum error");
                    continue;
                }
                
                dataProcessor.dataProcessing(ref data);
            }
        }
        
        private static void loop() {
            while (true) {
                var hidDevice = co2DeviceHandler.connectDevice(vendorId, productId);
                var stream = co2DeviceHandler.openStream(hidDevice);
                deviceLoop(stream);
                co2DeviceHandler.closeStream(stream);         
            }
        }
        private static void UseHidSharp() {
            co2DeviceHandler = new Co2DeviceHandler();
            dataProcessor =new DataProcessor();
            loop();
        }
    }
}