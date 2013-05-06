﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18033
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace VFSBase.DiskServiceReference {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="User", Namespace="http://schemas.datacontract.org/2004/07/VFSWCFService.UserService")]
    [System.SerializableAttribute()]
    public partial class User : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string HashedPasswordField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string LoginField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string HashedPassword {
            get {
                return this.HashedPasswordField;
            }
            set {
                if ((object.ReferenceEquals(this.HashedPasswordField, value) != true)) {
                    this.HashedPasswordField = value;
                    this.RaisePropertyChanged("HashedPassword");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Login {
            get {
                return this.LoginField;
            }
            set {
                if ((object.ReferenceEquals(this.LoginField, value) != true)) {
                    this.LoginField = value;
                    this.RaisePropertyChanged("Login");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="Disk", Namespace="http://schemas.datacontract.org/2004/07/VFSWCFService.DiskService")]
    [System.SerializableAttribute()]
    public partial class Disk : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private long LastServerVersionField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private long LocalVersionField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private long NewestBlockField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private VFSBase.DiskServiceReference.User UserField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string UuidField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public long LastServerVersion {
            get {
                return this.LastServerVersionField;
            }
            set {
                if ((this.LastServerVersionField.Equals(value) != true)) {
                    this.LastServerVersionField = value;
                    this.RaisePropertyChanged("LastServerVersion");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public long LocalVersion {
            get {
                return this.LocalVersionField;
            }
            set {
                if ((this.LocalVersionField.Equals(value) != true)) {
                    this.LocalVersionField = value;
                    this.RaisePropertyChanged("LocalVersion");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public long NewestBlock {
            get {
                return this.NewestBlockField;
            }
            set {
                if ((this.NewestBlockField.Equals(value) != true)) {
                    this.NewestBlockField = value;
                    this.RaisePropertyChanged("NewestBlock");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public VFSBase.DiskServiceReference.User User {
            get {
                return this.UserField;
            }
            set {
                if ((object.ReferenceEquals(this.UserField, value) != true)) {
                    this.UserField = value;
                    this.RaisePropertyChanged("User");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Uuid {
            get {
                return this.UuidField;
            }
            set {
                if ((object.ReferenceEquals(this.UuidField, value) != true)) {
                    this.UuidField = value;
                    this.RaisePropertyChanged("Uuid");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="DiskOptions", Namespace="http://schemas.datacontract.org/2004/07/VFSWCFService.DiskService")]
    [System.SerializableAttribute()]
    public partial class DiskOptions : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int BlockSizeField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int MasterBlockSizeField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private byte[] SerializedFileSystemOptionsField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int BlockSize {
            get {
                return this.BlockSizeField;
            }
            set {
                if ((this.BlockSizeField.Equals(value) != true)) {
                    this.BlockSizeField = value;
                    this.RaisePropertyChanged("BlockSize");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int MasterBlockSize {
            get {
                return this.MasterBlockSizeField;
            }
            set {
                if ((this.MasterBlockSizeField.Equals(value) != true)) {
                    this.MasterBlockSizeField = value;
                    this.RaisePropertyChanged("MasterBlockSize");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public byte[] SerializedFileSystemOptions {
            get {
                return this.SerializedFileSystemOptionsField;
            }
            set {
                if ((object.ReferenceEquals(this.SerializedFileSystemOptionsField, value) != true)) {
                    this.SerializedFileSystemOptionsField = value;
                    this.RaisePropertyChanged("SerializedFileSystemOptions");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="SynchronizationState", Namespace="http://schemas.datacontract.org/2004/07/VFSWCFService.DiskService")]
    public enum SynchronizationState : int {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        RemoteChanges = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        LocalChanges = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Conflicted = 2,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        UpToDate = 3,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="DiskServiceReference.IDiskService")]
    public interface IDiskService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDiskService/Disks", ReplyAction="http://tempuri.org/IDiskService/DisksResponse")]
        VFSBase.DiskServiceReference.Disk[] Disks(VFSBase.DiskServiceReference.User user);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDiskService/Disks", ReplyAction="http://tempuri.org/IDiskService/DisksResponse")]
        System.Threading.Tasks.Task<VFSBase.DiskServiceReference.Disk[]> DisksAsync(VFSBase.DiskServiceReference.User user);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDiskService/CreateDisk", ReplyAction="http://tempuri.org/IDiskService/CreateDiskResponse")]
        VFSBase.DiskServiceReference.Disk CreateDisk(VFSBase.DiskServiceReference.User user, VFSBase.DiskServiceReference.DiskOptions options);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDiskService/CreateDisk", ReplyAction="http://tempuri.org/IDiskService/CreateDiskResponse")]
        System.Threading.Tasks.Task<VFSBase.DiskServiceReference.Disk> CreateDiskAsync(VFSBase.DiskServiceReference.User user, VFSBase.DiskServiceReference.DiskOptions options);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDiskService/DeleteDisk", ReplyAction="http://tempuri.org/IDiskService/DeleteDiskResponse")]
        bool DeleteDisk(VFSBase.DiskServiceReference.Disk disk);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDiskService/DeleteDisk", ReplyAction="http://tempuri.org/IDiskService/DeleteDiskResponse")]
        System.Threading.Tasks.Task<bool> DeleteDiskAsync(VFSBase.DiskServiceReference.Disk disk);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDiskService/FetchSynchronizationState", ReplyAction="http://tempuri.org/IDiskService/FetchSynchronizationStateResponse")]
        VFSBase.DiskServiceReference.SynchronizationState FetchSynchronizationState(VFSBase.DiskServiceReference.Disk disk);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDiskService/FetchSynchronizationState", ReplyAction="http://tempuri.org/IDiskService/FetchSynchronizationStateResponse")]
        System.Threading.Tasks.Task<VFSBase.DiskServiceReference.SynchronizationState> FetchSynchronizationStateAsync(VFSBase.DiskServiceReference.Disk disk);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDiskService/GetDiskOptions", ReplyAction="http://tempuri.org/IDiskService/GetDiskOptionsResponse")]
        VFSBase.DiskServiceReference.DiskOptions GetDiskOptions(VFSBase.DiskServiceReference.Disk disk);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDiskService/GetDiskOptions", ReplyAction="http://tempuri.org/IDiskService/GetDiskOptionsResponse")]
        System.Threading.Tasks.Task<VFSBase.DiskServiceReference.DiskOptions> GetDiskOptionsAsync(VFSBase.DiskServiceReference.Disk disk);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDiskService/SetDiskOptions", ReplyAction="http://tempuri.org/IDiskService/SetDiskOptionsResponse")]
        void SetDiskOptions(VFSBase.DiskServiceReference.DiskOptions disk);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDiskService/SetDiskOptions", ReplyAction="http://tempuri.org/IDiskService/SetDiskOptionsResponse")]
        System.Threading.Tasks.Task SetDiskOptionsAsync(VFSBase.DiskServiceReference.DiskOptions disk);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDiskService/WriteBlock", ReplyAction="http://tempuri.org/IDiskService/WriteBlockResponse")]
        void WriteBlock(string diskUuid, long blockNr, byte[] content);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDiskService/WriteBlock", ReplyAction="http://tempuri.org/IDiskService/WriteBlockResponse")]
        System.Threading.Tasks.Task WriteBlockAsync(string diskUuid, long blockNr, byte[] content);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDiskService/ReadBlock", ReplyAction="http://tempuri.org/IDiskService/ReadBlockResponse")]
        byte[] ReadBlock(string diskUuid, long blockNr);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDiskService/ReadBlock", ReplyAction="http://tempuri.org/IDiskService/ReadBlockResponse")]
        System.Threading.Tasks.Task<byte[]> ReadBlockAsync(string diskUuid, long blockNr);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IDiskServiceChannel : VFSBase.DiskServiceReference.IDiskService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class DiskServiceClient : System.ServiceModel.ClientBase<VFSBase.DiskServiceReference.IDiskService>, VFSBase.DiskServiceReference.IDiskService {
        
        public DiskServiceClient() {
        }
        
        public DiskServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public DiskServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public DiskServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public DiskServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public VFSBase.DiskServiceReference.Disk[] Disks(VFSBase.DiskServiceReference.User user) {
            return base.Channel.Disks(user);
        }
        
        public System.Threading.Tasks.Task<VFSBase.DiskServiceReference.Disk[]> DisksAsync(VFSBase.DiskServiceReference.User user) {
            return base.Channel.DisksAsync(user);
        }
        
        public VFSBase.DiskServiceReference.Disk CreateDisk(VFSBase.DiskServiceReference.User user, VFSBase.DiskServiceReference.DiskOptions options) {
            return base.Channel.CreateDisk(user, options);
        }
        
        public System.Threading.Tasks.Task<VFSBase.DiskServiceReference.Disk> CreateDiskAsync(VFSBase.DiskServiceReference.User user, VFSBase.DiskServiceReference.DiskOptions options) {
            return base.Channel.CreateDiskAsync(user, options);
        }
        
        public bool DeleteDisk(VFSBase.DiskServiceReference.Disk disk) {
            return base.Channel.DeleteDisk(disk);
        }
        
        public System.Threading.Tasks.Task<bool> DeleteDiskAsync(VFSBase.DiskServiceReference.Disk disk) {
            return base.Channel.DeleteDiskAsync(disk);
        }
        
        public VFSBase.DiskServiceReference.SynchronizationState FetchSynchronizationState(VFSBase.DiskServiceReference.Disk disk) {
            return base.Channel.FetchSynchronizationState(disk);
        }
        
        public System.Threading.Tasks.Task<VFSBase.DiskServiceReference.SynchronizationState> FetchSynchronizationStateAsync(VFSBase.DiskServiceReference.Disk disk) {
            return base.Channel.FetchSynchronizationStateAsync(disk);
        }
        
        public VFSBase.DiskServiceReference.DiskOptions GetDiskOptions(VFSBase.DiskServiceReference.Disk disk) {
            return base.Channel.GetDiskOptions(disk);
        }
        
        public System.Threading.Tasks.Task<VFSBase.DiskServiceReference.DiskOptions> GetDiskOptionsAsync(VFSBase.DiskServiceReference.Disk disk) {
            return base.Channel.GetDiskOptionsAsync(disk);
        }
        
        public void SetDiskOptions(VFSBase.DiskServiceReference.DiskOptions disk) {
            base.Channel.SetDiskOptions(disk);
        }
        
        public System.Threading.Tasks.Task SetDiskOptionsAsync(VFSBase.DiskServiceReference.DiskOptions disk) {
            return base.Channel.SetDiskOptionsAsync(disk);
        }
        
        public void WriteBlock(string diskUuid, long blockNr, byte[] content) {
            base.Channel.WriteBlock(diskUuid, blockNr, content);
        }
        
        public System.Threading.Tasks.Task WriteBlockAsync(string diskUuid, long blockNr, byte[] content) {
            return base.Channel.WriteBlockAsync(diskUuid, blockNr, content);
        }
        
        public byte[] ReadBlock(string diskUuid, long blockNr) {
            return base.Channel.ReadBlock(diskUuid, blockNr);
        }
        
        public System.Threading.Tasks.Task<byte[]> ReadBlockAsync(string diskUuid, long blockNr) {
            return base.Channel.ReadBlockAsync(diskUuid, blockNr);
        }
    }
}
