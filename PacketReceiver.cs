using com.csf.netutils.packets;

namespace com.csf.netutils {
    public interface PacketReceiver {
        void onPacketReceived(Packet thePacket);
        void onFeedStopped(Packet thePacket);
        void onStatusUpdate(string theStatus);
    }
}
