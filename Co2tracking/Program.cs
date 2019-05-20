using System;
using System.Linq;
using HidSharp;

namespace Co2tracking {
    internal static class Program {
        private const int VendorId = 0x04d9;
        private const int ProductId = 0xa052;

        private static Co2DeviceHandler _co2DeviceHandler;
        private static DataProcessor _dataProcessor;

        static void Main(string[] args) {
            string[] unused = args;
            UseHidSharp();
        }

/*
        private static void tryShowAlldevices() {
            DeviceList devices = DeviceList.Local;
            devices.Changed += (sender, e) => Console.WriteLine(value: "Device list changed.");
            Device[] allDeviceList = devices.GetAllDevices().ToArray();
            Console.WriteLine(value: "All device list:");
            foreach (Device dev in allDeviceList) {
                Console.Write(dev + " @ " + dev.DevicePath + " : ");
                switch (dev) {
                    case HidDevice device: {
                        Console.Write(value: "HidDevice");
                        foreach (string serialPort in (device.GetSerialPorts())) {
                            Console.Write("    " + serialPort);
                        }
                        break;
                    }
                    default:
                        Console.Write("not HidDevice ");
                        break;
                }
                Console.WriteLine();
            }
        }
*/

        private static void DeviceLoop(HidStream stream) {
            //the device won't send anything before receiving this packet 
            byte[] reportId = { 0x00 };
            byte[] key = { 0xc4, 0xc6, 0xc0, 0x92, 0x40, 0x23, 0xdc, 0x96 };
            byte[] request = reportId.Concat(key).ToArray();
            try {
                _co2DeviceHandler.sendSetFeatureSetupRequest(stream, request);
            } catch (Exception ex) {
                Console.WriteLine(value: "Unable to send SetFeatureSetupRequest");
                throw ex;
            }

            while (true) {
                byte[] receivedData = _co2DeviceHandler.readData(stream);
                if (receivedData.Length == 0) {
                    Console.WriteLine(value: "Unable to read data");
                    break;
                }
                if (receivedData.Length != 9) {
                    Console.WriteLine(value: "transferred amount of bytes != expected bytes amount");
                    break;
                }
                
                Console.Write(value: "received data: ");
                foreach (byte t in receivedData) Console.Write(t + " ");
                Console.WriteLine();
                var temp = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                for (var i = 0; i < 8; i++) {
                    temp[i] = receivedData[i + 1];
                }
                receivedData = temp;                
                Console.Write(value: "received data: ");
                foreach (byte t in receivedData) Console.Write(t + " ");
                Console.WriteLine();

                int[] data = _dataProcessor.decryptData(ref key, ref receivedData);
                Console.Write(value: "decrypt data: ");
                foreach (int t in data)
                    Console.Write(t + " ");
                Console.WriteLine();        

                if (!_dataProcessor.checkEndOfMessage(ref data)) {
                    Console.WriteLine(value: "Unexpected data from device");
                    continue;
                }

                if (!_dataProcessor.checkCheckSum(ref data)) {
                    Console.WriteLine(value: "checksum error");
                    continue;
                }
                
                _dataProcessor.dataProcessing(ref data);
            }
        }
        
        private static void loop() {
            while (true) {
                HidDevice hidDevice = _co2DeviceHandler.connectDevice(VendorId, ProductId);
                HidStream stream = _co2DeviceHandler.openStream(hidDevice);
                DeviceLoop(stream);
                _co2DeviceHandler.closeStream(stream);         
            }
        }
    
        private static void UseHidSharp() {
            _co2DeviceHandler = new Co2DeviceHandler();
            _dataProcessor = new DataProcessor();
            loop();
        }
    }
}