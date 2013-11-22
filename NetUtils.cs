namespace com.csf.netutils {
    public static class NetUtils {
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
