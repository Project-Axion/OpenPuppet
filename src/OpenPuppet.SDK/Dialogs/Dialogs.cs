using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK.Dialogs
{
    public static class Dialogs
    {
        public static void RegisterBuiltInDialogs()
        {
            IUIDialog.Register("openpuppet.sdk.progressdialog", typeof(ProgressDialog));
            IUIDialog.Register("openpuppet.sdk.messagebox", typeof(MessageBox));
        }
    }
}
