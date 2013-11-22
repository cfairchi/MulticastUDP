namespace MulticastUDP.packets {
    public class SamplePacket : Packet {
        public double DoubleValue {get;set;}
        public string StringValue {get;set;}
        public DateTime DateTimeValue {get;set}
        
        public override PACKETTYPE PacketType {get { return PACKETTYPE.SAMPLE; }}
        
        public SamplePacket() : base() {}
        public SamplePacket(bool isHeartBeat) : base(isHeartBeat) {}
        public SamplePacket(byte[] theBytes) : base(theBytes){}
        
        public SamplePackdet(SamplePacket thePacket) : base(thePacket) {
            DoubleValue
        }
        
    }
}   
