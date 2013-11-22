using namespace com.csf.netutils;

namespace com.csf.netutils.udp {
    public class m_MulticastSocket = null;
        private Socket m_MulticastSocket = null;
        private IPAddress m_MulticastIP = IPAddress.Parse("239.0.1.1");
        private int MulticastPort = -1;
        private m_MMulticastOption = null;
        private IPAddress m_LocalIP = null;
        private IPEndPoint m_LocalEP = null;
        private IPEndPoint m_GroupEP = null;
        
        public UDPMulticastServer(int thePort) {
            m_MulticastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, Protocol.Udp);
            m_MulticastPort = thePort;
            m_LocalIP = NetUtils.getMyIpAddress();
            m_LocalEP = new IPEndPoint(m_LocalIP,m_MulticastPort);
            m_GroupEP = new IPEndPoint(m_MulticastIP,m_MulticastPort);
            joinMulticastGroup();
        }
        
        private void joinMulticastGroup() {
            try {
                m_MulticastSocket = new Socket(AddressFamily.InterNetwork, Socket.Dgram, Protocol.Udp);
                IPEndPoint iep = new IPEndPoint(IPAddress.Any, m_MulticastPort);
                m_MulticastSocket.Bind(iep);
                m_MulticastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(m_MulticastIP));
                m_MulticastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 50);
            }
            catch(Exception ex){}
        }
        
        public void broadcastPacket(Packet thePacket) {
            try {
                m_MulticastSocket.Send(thePacket.toByteArray(), m_GroupEP);
            }
            catch(Exception ex) {}
        }
        
}
