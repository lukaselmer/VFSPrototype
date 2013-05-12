using System;

namespace VFSBrowser.ViewModel
{
    public interface IMainViewModel : IDisposable
    {
        long VersionInput { get; set; }
    }
}