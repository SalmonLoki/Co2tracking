using HidSharp;

namespace Co2tracking {
    public interface ICo2DeviceHandler {
        HidDevice connectDevice(int vendorId, int productId);

        HidStream openStream(HidDevice hidDevice);

        void closeStream(HidStream stream);

        void sendSetFeatureSetupRequest(HidStream stream, byte[] buffer);

        byte[] readData(HidStream stream);
    }
}