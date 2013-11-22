namespace MulticastUDP {
    public static class NetUtils {
        
        // Returns first valid IP Address 
        // If Multiple IP Addresses are assigned to host machine, additional code may be required to choose
        // desired network interface
        public static IPAddress = getMyIPAddress() {
            IPHostEntry iphe = Dns.GetHostEntry(Dns.GetHostName());
            for(int i=0; i < iphe.AddressList.Length;++i){
                string ip = iphe.AddressList[i].ToString();
                if (ip!= "::1") return iphe.AddressList[i];
            }
            return null;
        }
    }
}
