using NativeFileDialogSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK
{
    public static class NativeDialogs
    {
        /*
         * 
         * This whole section needs replacing
         * 
         */

        public class OpenFileResult
        {
            public string? Path { get; private set; } = null;
            public string[]? Paths { get; private set; } = null;
            public bool IsOk { get; private set; } = true;
            public bool IsError { get; private set; } = false;
            public bool Cancelled { get; private set; } = false;
            public bool MultiplePaths { get; private set; } = false;
            public string? ErrorMessage { get; private set; } = null;

            public OpenFileResult
            (
                string? path,
                string[]? paths,
                bool isOk,
                bool isError,
                bool cancelled,
                string? errorMessage
            )
            {
                Path = path;
                Paths = paths;
                if (Paths != null && Paths?.Length > 0) MultiplePaths = true;
                IsOk = isOk;
                IsError = isError;
                Cancelled = cancelled;
                ErrorMessage = errorMessage;
            }
        }

        public static bool OpenFileDialogResultHasPath(OpenFileResult result)
        {
            return result.IsOk && !result.IsError && !result.Cancelled && result.Path != null;
        }

        public static OpenFileResult OpenFileDialog(string? filter = null, string? defaultPath = null)
        {
            DialogResult res = Dialog.FileOpen(filter, defaultPath);
            return new(res.Path, res.Paths == null ? null : [.. res.Paths], res.IsOk, res.IsError, res.IsCancelled, res.ErrorMessage);
        }

        public class OpenDirectoryResult
        {
            public string? Path { get; private set; } = null;
            public string[]? Paths { get; private set; } = null;
            public bool IsOk { get; private set; } = true;
            public bool IsError { get; private set; } = false;
            public bool Cancelled { get; private set; } = false;
            public bool MultiplePaths { get; private set; } = false;
            public string? ErrorMessage { get; private set; } = null;

            public OpenDirectoryResult
            (
                string? path,
                string[]? paths,
                bool isOk,
                bool isError,
                bool cancelled,
                string? errorMessage
            )
            {
                Path = path;
                Paths = paths;
                if (Paths != null && Paths?.Length > 0) MultiplePaths = true;
                IsOk = isOk;
                IsError = isError;
                Cancelled = cancelled;
                ErrorMessage = errorMessage;
            }
        }

        public static bool OpenDirectoryDialogResultHasPath(OpenFileResult result)
        {
            return result.IsOk && !result.IsError && !result.Cancelled && result.Path != null;
        }

        public static OpenFileResult OpenDirectoryDialog(string? defaultPath = null)
        {
            DialogResult res = Dialog.FolderPicker(defaultPath);
            return new(res.Path, res.Paths == null ? null : [.. res.Paths], res.IsOk, res.IsError, res.IsCancelled, res.ErrorMessage);
        }
    }
}
