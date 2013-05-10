using VFSBase.Interfaces;
using VFSBrowser.View;

namespace VFSBrowser.ViewModel
{
    public class SynchronizationViewModel : AbstractViewModel
    {

        public SynchronizationViewModel(ISynchronizationService service)
        {
            _service = service;
        }

        private SynchronizationDialog _dlg;
        private readonly ISynchronizationService _service;

        public void ShowDialog()
        {
            if (_service == null) return;

            _dlg = new SynchronizationDialog(this);
            _dlg.ShowDialog();
        }

        public void Close()
        {
            _dlg.Close();
        }
    }
}
