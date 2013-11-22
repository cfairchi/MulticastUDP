using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace MulticastUDP.packets {
    public abstract class Packet {
        private SortedDictionary<string, PropertyInfo> m_OrderedProperties = new SortedDictionary<string, PropertyInfo>();
        private ConcurrentDictionary<string,int> m_PropertyIndexes = new ConcurrentDictionary<string,int>();
        
        public const int MAX_STRING_LENGTH = 128;
        public Guid PacketId {get;set;}
        public string MachineName {get;set;}
        public string UserName {get;set;}
        public DateTime TimeStamp {get;set;}
        public bool IsHeartBeat {get;set;}

        public abstract PACKETTYPE PacketType { get; }
        public abstract void setDefaultValues();
        
        public enum PACKETTYPE {
            AIRCRAFTPACKET,
            UNKNOWN
        }
        
        public Packet() {
            setBaseDefaultValues();
            populateOrderedProperties();
        }
        
        public Packet(Packet thePacket) {
            PacketId = thePacket.PacketId;
            MachineName = thePacket.MachineName;
            UserName = thePacket.UserName;
            TimeStamp = thePacket.TimeStamp;
            IsHeartBeat = thePacket.IsHeartBeat;
        }
        
        public Packet(bool isHeartBeat) {
            setBaseDefaultValues();
            IsHeartBeat = isHeartBeat;
            populateOrderedProperties();
        }
        
        public Packet(byte[] theBytes){
            setBaseDefaultValues();
            populateOrderedProperties();
            parseBytes(theBytes);
        }
        
        public string getFeedId() {
            return UserName + "@" + MachineName + "." +PacketType.ToString();
        }
        
        private void setBaseDefaultValues() {
            PacketId = Guid.NewGuid();
            MachineName = Environment.MachineName;
            UserName = Environment.UserName;
            TimeStamp = DateTime.Now;
            IsHeartBeat = false;
            setDefaultValues();
        }
        
        public override string ToString() {
            return "[" + TimeStamp + "]" + MachineName + ":" + UserName;
        }
        
        public void populateRow(ref DataGridViewRow theRow) {
            foreach (PropertyInfo pi in GetType().GetProperties()) {
                if (theRow.DataGridView.Columns.Contains(pi.Name)) {
                    theRow.Cells[pi.Name].Value = pi.GetValue(this,null);
                }
            }
        }
        
        public void createColumns(ref DataGridView theDGV) {
            theDGV.Columns.Clear();
            foreach (PropertyInfo pi in GetType().GetProperties()) {
                DataGridViewColumn col = theDGV.Columns[theDGV.Columns.Add(pi.Name, pi.Name)];
                col.ValueType = pi.PropertyType;
            }
        }
        
        // Organizes all properties in name sorted order
        // This is required because GetType().GetProperties() does not garuntee order
        // and we are relying on the order being the same when converting to and from byte arrays
        private void populateOrderedProperties() {
            m_OrderedProperties = new SortedDictionary<string, PropertyInfo>();
            foreach (PropertyInfo pi in GetType().GetProperties()) {
                m_OrderedProperties.Add(pi.Name,pi);
            }
            
            int index = 4;
            foreach(KeyValuePair<string, PropertyInfo> kvp in m_OrderedProperties) {
                PropertyInfo pi = kvp.Value;
                m_PropertyIndexes.TryAdd(pi.Name, index);
                if (pi.PropertyType == typeof(string)) {index += MAX_STRING_LENGTH; }
                else if (pi.PropertyType == typeof(double)) { index += 8; }
                else if (pi.PropertyType == typeof(bool)) { index += 1; }
                else if (pi.PropertyType == typeof(Int16)) { index += 2; }
                else if (pi.PropertyType == typeof(Int32)) { index += 4; }
                else if (pi.PropertyType == typeof(Int64)) { index += 8; }
                else if (pi.PropertyType == typeof(int)) { index += 4; }
                else if (pi.PropertyType == typeof(Guid)) { index += 16; }
                else if (pi.PropertyType == typeof(PACKETTYPE)) { }
                else if (pi.PropertyType == typeof(DateTime)) { index += 8; }
            }
        }
        
        public void parseBytesForProperty(KeyValuePair<string, PropertyInfo> kvp, byte[] theBytes) {
            try {
                int index = m_PropertyIndexes[kvp.Value.Name];
                if (index > theBytes.Length) { return; }
                
                if (kvp.Value.PropertyType == typeof(string)) {
                    //Int32 stringLength = BitConverter.ToInt32(theBytes, index);
                    byte[] stringBytes = new byte[MAX_STRING_LENGTH];
                    Array.Copy(theBytes, index, stringBytes, 0, MAX_STRING_LENGTH);
                    String value = System.Text.Encoding.Default.GetString(stringBytes);
                    kvp.Value.SetValue(this,value,null);
                }
                else if (kvp.Value.PropertyType == typeof(double)) {
                    double value = BitConverter.ToDouble(theBytes,index);
                    kvp.Value.SetValue(this,value,null);
                }
                else if (kvp.Value.PropertyType == typeof(bool)) {
                    bool value = BitConverter.ToBoolean(theBytes,index);
                    kvp.Value.SetValue(this,value,null);
                }
                else if (kvp.Value.PropertyType == typeof(Int16)) {
                    Int16 value = BitConverter.Int16(theBytes,index);
                    kvp.Value.SetValue(this,value,null);
                }
                else if (kvp.Value.PropertyType == typeof(Int32)) {
                    Int32 value = BitConverter.Int32(theBytes,index);
                    kvp.Value.SetValue(this,value,null);
                }
                else if (kvp.Value.PropertyType == typeof(Int64)) {
                    Int64 value = BitConverter.Int64(theBytes,index);
                    kvp.Value.SetValue(this,value,null);
                }
                else if (kvp.Value.PropertyType == typeof(int)) {
                    int value = BitConverter.int(theBytes,index);
                    kvp.Value.SetValue(this,value,null);
                }
                else if (kvp.Value.PropertyType == typeof(Guid)) {
                    Int64 value = BitConverter.ToInt64(theBytes, index);
                    byte[] guidBytes = new byte[16];
                    Array.Copy(theBytes, index,guidBytes,0,16);
                    Guid id = new Guid(guidBytes);
                    kvp.Value.SetValue(this,id,null);
                }
                else if (kvp.Value.PropertyType == typeof(DateTime)) {
                    Int64 ticks = BitConverter.ToInt64(theBytes,index);
                    DateTime dt = new DateTime(ticks);
                    kvp.Value.SetValue(this,value,null);
                }
            
            }
            catch(Exception ex){}
        }
        
        public void parseBytes(byte[] theBytes) {
            Parallel.ForEach(m_OrderedProperties, item => pareseBytesForProperty(item, theBytes));
        }
        

        private void appendString(string theValue, ref List<Byte> theByteList){
            try {
                if (theValue == null) {
                    theValue = "";
                    theValue = theValue.PadRight(MAX_STRING_LENGTH);
                }
                else {
                    if (theValue.Length > MAX_STRING_LENGTH) theValue = theValue.Substring(0,MAX_STRING_LENGTH);
                    if (theValue.Length < MAX_STRING_LENGTH) theValue = theValue.PadRight(MAX_STRING_LENGTH);
                }
                byte[] stringBytes = System.Text.Encoding.ASCII.GetBytes(theValue);
                theByteList.AddRange(stringBytes);
            }
            catch(Exception ex) {}
        }
        
        private void appendInt64(Int64 theValue, ref List<Bytes> theByteList) {
            theByteList.AddRange(BitConverter.GetBytes(theValue));
        }
        private void appendInt32(Int32 theValue, ref List<Bytes> theByteList) {
            theByteList.AddRange(BitConverter.GetBytes(theValue));
        }
        private void appendInt16(Int16 theValue, ref List<Bytes> theByteList) {
            theByteList.AddRange(BitConverter.GetBytes(theValue));
        }
        private void appendDouble(Double theValue, ref List<Bytes> theByteList) {
            theByteList.AddRange(BitConverter.GetBytes(theValue));
        }
        private void appendDouble(bool theValue, ref List<Bytes> theByteList) {
            theByteList.AddRange(BitConverter.GetBytes(theValue));
        }
        private void appendDateTime(DateTime theValue, ref List<Bytes> theByteList) {
            appendInt64(theValue.Ticks, ref theByteList);
        }
        private void appendGuid(Guid theValue, ref List<Bytes> theByteList) {
            theByteList.AddRange(theValue.ToByteArray());
        }
        
        public string toCSVString() {
            StringBuilder sBuilder = new StringBuilder();
            foreach (KeyValuePair<string, PropertyInfo> kvp in m_OrderedProperties) {
                PropertyInfo pi = kvp.Value;
                sBuilder.Append(pi.GetValue(this,null).ToString());
                sBuilder.Append(",");")
            }
            return sBuilder.ToString();
        }
        
        public byte[] toByteArray() {
            List<Byte> byteList = new List<Byte>();
            List<Byte> finalByteList = new List<Byte>();
            
            byteList.AddRange(BitConverter.GetBytes((int)PacketType));
            foreach (KeyValuePair<string, PropertyInfo> kvp in m_OrderedProperties) {
                try {
                    PropertyInfo pi = kvp.Value;
                    if (pi.PropertyType == typeof(string)) { appendString((string)pi.GetValue(this,null),ref byteList); }
                    else if (pi.PropertyType == typeof(DateTime)) { appendDateTime((DateTime)pi.GetValue(this,null),ref byteList); }
                    else if (pi.PropertyType == typeof(double)) { appendDouble((double)pi.GetValue(this,null),ref byteList); }
                    else if (pi.PropertyType == typeof(bool)) { appendBool((bool)pi.GetValue(this,null),ref byteList); }
                    else if (pi.PropertyType == typeof(int)) { appendDateTime((int)pi.GetValue(this,null),ref byteList); }
                    else if (pi.PropertyType == typeof(long)) { appendDateTime((long)pi.GetValue(this,null),ref byteList); }
                    else if (pi.PropertyType == typeof(Int16)) { appendDateTime((Int16)pi.GetValue(this,null),ref byteList); }
                    else if (pi.PropertyType == typeof(Guid)) { appendDateTime((Guid)pi.GetValue(this,null),ref byteList); }
                    else if (pi.PropertyType == typeof(PACKETTYPE)) { /* DO NOTHING */ }
                    else {
                        // WARN Property Type Unknown
                    }
                }
                catch(Exception ex){
                    
                }
            }
            finalByteList.AddRange(byteList);
            return finalByteList.ToArray<byte>();
        }
        
        public static Packet fromBytes(byte[] theBytes) {
            Packet newPacket = null;
            Packet.PACKETTYPE pType = Packet.decodePacketType(theBytes);
            switch(pType) {
                case Packet.PACKETTYPE.AIRCRAFTPACKET:
                    newPacket = new AircraftPAcket(theBytes);
                    break;
            }
            return newPacket;
        }
        
        public static PACKETTYPE decodePacketType(byte[] theBytes) {
            PACKETTYPE pType = PACKETTYPE.UNKNOWN;
            pType = (PACKETTYPE)BitConverter.ToInt32(theBytes,0);
            return pType;
        }
        
        
    }
}
