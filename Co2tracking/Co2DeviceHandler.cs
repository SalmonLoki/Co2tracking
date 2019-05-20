using System;
using HidSharp;

namespace Co2tracking {
    public class Co2DeviceHandler : ICo2DeviceHandler {
        public HidDevice connectDevice(int vendorId, int productId) {
            DeviceList devices = DeviceList.Local;
            devices.Changed += (sender, e) => Console.WriteLine(value: "Device list changed.");
            
            HidDevice hidDevice = devices.GetHidDeviceOrNull(vendorID: vendorId, productID: productId);
            if (hidDevice == null) {
                throw new Exception(message: "Device not found");
            }
            Console.WriteLine(vendorId + ":" + productId + " connected");
            return hidDevice;
        }

        public HidStream openStream(HidDevice hidDevice) {
            if (!hidDevice.TryOpen(out HidStream stream)) {
                throw new Exception(message: "Stream not created");
            }
            Console.WriteLine(value: "Stream created");
            stream.ReadTimeout = 3000; //The maximum amount of time, in milliseconds, to wait for the device to send some data
            stream.WriteTimeout = 3000; //The maximum amount of time, in milliseconds, to wait for the device to receive the data.
            return stream;
        }

        public void closeStream(HidStream stream) {
            stream.Close();
        }

        public void sendSetFeatureSetupRequest(HidStream stream, byte[] buffer) {
            //The buffer of data to send. Place the Report ID in the first byte
            stream.SetFeature(buffer);
        }

        public byte[] readData(HidStream stream) {
            byte[] buffer = stream.Read();
            return buffer;
        }
    }
}