using System.ComponentModel;
using com.csf.netutils.packets;

namespace com.csf.NetUtils.udp {
    
    // The HeartBeat class is responsible for sending out periodic "HeartBeat" packets.
    // Currently configured to send heartbeat every 10 seconds.  Allows clients to 
    // detect servers that exist on the the same multicast address/port
    public class HeartBeat {
        private Packet.PACKETTYPE thePacketType, UDPMultiCastServer theUDPServer);
        private BackgroundWorker m_HeartBeatWorker;
        private UDPMultiCastServer m_Server;
        
        public HeartBeat(Packet.PACKETTYPE thePacketType, UDPMultiCastServer theUDPServer) {
            m_Server = theUDPServer;
            m_PacketType = thePacketType;
            m_HeartBeatWorker = new BackgroundWorker();
            m_HeartBeatWorker.WorkerSupportsCancellation = true;
            m_HeartBeatWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.HeartBeatWorker_DoWorker);
        }
        
        public void startHeartBeat() {
            m_HeartBeatWorker.RunWorkerAsync();
        }
        
        public void stopHeartBeat() {
            m_HeartBeatWorker.CancelAsync();
        }
        
        private void HeartBeatWorker_DoWork(object sender, DoWorkEventArgs e){
            while (!m_HeartBeatWorker.CancellationPending) {
                packet hbPacket = null;
                switch (m_PacketType) {
                    case Packet.PACKETTYPE.AIRCRAFTPACKET:
                        hbPakcet = new AIRCRAFTPACKET(true);
                        break;
                }
                if (hbPacket != null) m_Server.broadcastPacket(hbPacket);
                SystemThreading.Thread.Sleep(10000);
            }
        }
    }


}
