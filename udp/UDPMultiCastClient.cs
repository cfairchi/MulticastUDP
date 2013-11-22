using namespace com.csf.netutils;

namespace com.csf.netutils.udp {
    public class UDPMultiCastClient {
        private Socket m_MulticastSocket = null;
        private IPAddress m_MulticastIP = IPAddress.Parse("239.0.1.1");
        private int MulticastPort = -1;
        private m_MMulticastOption = null;
        private IPAddress m_LocalIP = null;
        private IPEndPoint m_LocalEP = null;
        private IPEndPoint m_GroupEP = null;
        private EndPoint m_RemoteEP = null;
        private BackgroundWorker m_BGWorker = null;
        private PacketsAvailableDelegate m_PacketsAvailableDelegate = null;
        
        public delegate void PacketsAvailableDelegate();
        public ConcurrentQueue<byte[]> PacketQueue {get;set;}
        
        public UDPMultiCastClient(int thePort, PacketsAvailableDelegate thePacketsAvailableDelegate) {
            m_PacketsAvailableDelegate = thePacketsAvailableDelegate;
            PacketQueue = new ConcurrentQueue<byte[]>();
            m_MulticastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, Protocol.Udp);
            m_MulticastPort = thePort;
            m_LocalIP = NetUtils.getMyIPAddress();
            m_LocalEP = new IPEndPoint(m_LocalIP, m_MulticastPort);
            m_GroupEP = new IPEndPoint(m_MulticastIP, m_MulticastPort);
            m_RemoteEP = new IPEndPoint(IPAddress.Any,0);
            m_BGWorker - new BackgroundWorker();
            m_BGWorker.WorkerSupportsCancellation = true;
            m_BGWorker.DoWork += new DoWorkEventHandler(m_BgWorker_DoWork);
            startMulticast();
        }
        
        
        void m_BGWorker_DoWork(object sender, DoWorkEventArgs e) {
            while (!m_BGWorker.CancellationPending) {
                receiveBroadcastMessages();
            }
        }
        
        private void startMulticast() {
            try{
                m_MulticastSocket.Bind(m_LocalEP);
                m_MulticastOption = new MulticastOption(m_MulticastIP, m_LocalIP);
                m_MulticastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, m-MulticastOption;
                m_BGWorker.RunWorkerAsync();
            }
            catch(Exception ex){}
        }
        
        private void receiveBroadcastMessages() {
            byte[] receivedData = new byte[1024];
            m_MulticastSocket.ReceiveFrom(receivedData, ref m_RemoteEP);
            PacketQueue.Enqueue(receivedData);
            m_PacketsAvailableDelegate();
        }
    }
}
