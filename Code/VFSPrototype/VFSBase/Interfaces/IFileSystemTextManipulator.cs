﻿using System;
using System.Collections.Generic;
using VFSBase.Callbacks;
using VFSBase.DiskServiceReference;
using VFSBase.Implementation;
using VFSBase.Synchronization;

namespace VFSBase.Interfaces
{
    public interface IFileSystemTextManipulator : IDisposable
    {
        IList<string> Search(string keyword, string folder, bool recursive, bool caseSensitive);
        IList<string> Files(string path);
        IList<string> List(string path);
        IList<string> Folders(string path);
        IList<string> Folders(string path, long version);
        bool IsDirectory(string path);
        void CreateFolder(string path);
        void Copy(string source, string dest);
        void Copy(string source, string dest, CallbacksBase copyCallbacks);
        void Import(string source, string dest);
        void Import(string source, string dest, CallbacksBase importCallbacks);
        void Export(string source, string dest);
        void Export(string source, string dest, CallbacksBase exportCallbacks);
        void Export(string source, string dest, CallbacksBase exportCallbacks, long version);
        void Delete(string path);
        void Move(string source, string dest);
        bool Exists(string path);

        IFileSystemOptions FileSystemOptions { get; }

        long Version(string path);
        void SwitchToVersion(long version);
        void SwitchToLatestVersion();
        void RollBackToVersion(long version);
        IEnumerable<long> Versions(string path);
        long LatestVersion { get; }

        ISynchronizationService GenerateSynchronizationService(UserDto user, SynchronizationCallbacks callbacks);

        /// <summary>
        /// Occurs when the file system has changed.
        /// </summary>
        event EventHandler<FileSystemChangedEventArgs> FileSystemChanged;

    }
}
