using System;

namespace SharedUtils.DTO
{
    public class EventstoreEvent
    {
        public byte[] Data { get; set; }
        public string Metadata { get; set; }
        public DateTime TimestampUtc { get; set; }
        public string Datatype { get; set; }
    }
}