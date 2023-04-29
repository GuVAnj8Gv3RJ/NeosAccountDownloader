using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AccountDownloader.Services
{
    public readonly struct OpenFolderResult
    {
        public readonly bool Success = false;
        public readonly string? Error;

        public OpenFolderResult(bool succes)
        {
            Success = succes;
            Error = null;
        }
        public OpenFolderResult(string error)
        {
            Error = error;
            Success = false;
        }
    }
    public class IOService
    {
        public static OpenFolderResult OpenFolderDialog(string folder)
        {
            string? command = GetOpenFolderCommand();
            if (command == null)
                return new OpenFolderResult(string.Format(Res.OpenFolder_UnsupportedPlatform,folder));

            ProcessStartInfo processStartInfo = new(command)
            {
                UseShellExecute = true,
                Arguments = folder
            };
            var process = Process.Start(processStartInfo);
            if (process != null)
            {
                return new OpenFolderResult(true);
            }
            return new OpenFolderResult(Res.OpenFolder_Failed);            
        }

        public static string? GetOpenFolderCommand()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "explorer";

            // Won't work on really weird Linux distros
            // https://askubuntu.com/questions/31069/how-to-open-a-file-manager-of-the-current-directory-in-the-terminal#31071
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "xdg-open";

            return null;
        }
    }
}
