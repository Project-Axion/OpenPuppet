using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK.Dialogs
{
    public class ProgressDialog : IUIDialog, IDisposable
    {
        public string Title { get; set; } = "Progress Dialog";
        public ImGuiWindowFlags? Flags { get; set; } = ImGuiWindowFlags.NoTitleBar;
        public Vector2? Size { get; set; } = new(400, 250);

        public string Description = string.Empty;
        public IProgress<float> Progress = new Progress<float>();

        internal CancellationTokenSource? cancellationTokenSource;
        internal float progressValue = 0;

        public void Setup(
            string title,
            Progress<float> progress,
            string? description = default,
            CancellationTokenSource? cancellationTokenSource = null
        )
        {
            Title = title;
            Progress = progress;
            ((Progress<float>)Progress).ProgressChanged += ProgressChanged;
            Description = description ?? string.Empty;
            this.cancellationTokenSource = cancellationTokenSource;
        }

        void ProgressChanged(object? sender, float e)
        {
            progressValue = e;
        }

        public void Cancel()
        {
            if (cancellationTokenSource != null)
                cancellationTokenSource.Cancel();
        }

        public void OnClose()
        {
            Dispose();
        }

        public void OnLoad() { }

        public void OnPreRender() { }

        public void OnRender()
        {
            ImGui.Text(Description);
            ImGui.Spacing();
            ImGui.ProgressBar(progressValue);
            ImGui.Spacing();
            if (cancellationTokenSource != null)
            {
                if (ImGui.Button("Cancel##openpuppet.progressdialog.cancel"))
                {
                    cancellationTokenSource.Cancel();
                    IUIDialog.Close();
                }
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(false);
        }

        public void Dispose(bool everything = false)
        {
            if (everything)
            {
                if (cancellationTokenSource != null)
                    cancellationTokenSource.Dispose();
            }
            else
            {
                cancellationTokenSource = null;
            }
        }

        public static void Open(
            string title,
            Progress<float> progress,
            string? description = default,
            CancellationTokenSource? cancellationTokenSource = null
        )
        {
            IUIDialog.Open("openpuppet.sdk.progressdialog");
            if (IUIDialog.ActiveDialog!.GetType() == typeof(ProgressDialog))
            {
                ((ProgressDialog)IUIDialog.ActiveDialog).Setup(
                    title,
                    progress,
                    description,
                    cancellationTokenSource
                );
            }
        }
    }
}