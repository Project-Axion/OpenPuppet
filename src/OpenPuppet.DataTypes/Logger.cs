using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK
{
    public static class Logger
    {
        internal static string? LocalPath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenPuppet");
            }
        }

        public static string? LogPath
        {
            get
            {
                return LocalPath != null ? Path.Combine(LocalPath, "Logs") : null;
            }
        }

        public static class LogManager
        {
            public static string RequestPluginLogFile(string pluginName)
            {
                if(!Directory.Exists(Path.Combine(LogPath!, Plugins.PluginsPath.SafePluginName(pluginName)))) {
                    Directory.CreateDirectory(Path.Combine(LogPath!, Plugins.PluginsPath.SafePluginName(pluginName)));
                }

                Random r = new Random();

                string path = Path.Combine(
                    LogPath!,
                    Plugins.PluginsPath.SafePluginName(pluginName),
                    DateTime.Now.ToString("dd'-'MM'-'yyyy") + $"-{r.Next()}.txt"
                );

                Console.WriteLine(path);

                File.WriteAllText(path, null);
                return path;
            }

            public static PluginLogger RequestPluginLogger(string pluginName)
            {
                return new PluginLogger(pluginName, RequestPluginLogFile(pluginName));
            }
        }

        public interface ILogger : IDisposable
        {
            //public string LogFile               { get; internal set; }
            //internal FileStream fileStream      { get; set; }
            //internal StreamWriter fileWriter    { get; set; }

            public enum Level
            {
                Log,
                OK,
                Error,
                Warn
            }

            abstract void Write(string message);
            abstract void WriteLine(string message);

            abstract void WriteLine(Level level, string message);
        }

        public class PluginLogger : ILogger
        {
            public string PluginName { get; internal set; } = string.Empty;
            public string LogFile { get; internal set; } = string.Empty;
            internal FileStream FileStream { get; set; }
            internal StreamWriter FileWriter { get; set; }

            public PluginLogger(string name, string path)
            {
                PluginName = name;
                LogFile = path;
                FileStream = File.OpenWrite(path);
                FileWriter = new StreamWriter(FileStream);

                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

                FileWriter.WriteLine("==================== OpenPuppet.SDK Log ====================");
                FileWriter.WriteLine($"\tPlugin name:\t{name}");
                FileWriter.WriteLine($"\tPlugin version:\t{fvi.FileVersion}");

                FileWriter.Flush();
            }

            // TODO: Set up some kind of better flushing system so the file stream doesn't get clogged

            public void Write(string message)
            {
                FileWriter.Write(message);
                Console.Write(message);

                FileWriter.Flush();
            }

            public void WriteLine(string message)
            {
                string msg = $"[{PluginName}] ({DateTime.Now:HH':'mm':'ss dd'-'MM'-'yyyy}) {message}";
                FileWriter.WriteLine(msg);
                Console.WriteLine(msg);

                FileWriter.Flush();
            }

            public void WriteLine(ILogger.Level level, string message)
            {
                WriteLine($"{level}: {message}");
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);

                FileWriter.Close();
                FileStream.Close();

                FileWriter.Dispose();
                FileStream.Dispose();
            }
        }
    }
}