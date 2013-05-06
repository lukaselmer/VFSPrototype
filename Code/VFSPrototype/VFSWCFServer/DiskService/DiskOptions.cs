﻿using System.Runtime.Serialization;
using SQLite;

namespace VFSWCFService.DiskService
{
    /// <summary>
    /// The disk options are used to create the disk on the server and on the client (initial synchronization).
    /// </summary>
    [DataContract]
    public class DiskOptions
    {
        [PrimaryKey]
        [DataMember]
        public string DiskUuid { get; set; }

        [DataMember]
        public int BlockSize { get; set; }

        [DataMember]
        public int MasterBlockSize { get; set; }

        [DataMember]
        public byte[] SerializedFileSystemOptions { get; set; }
    }
}