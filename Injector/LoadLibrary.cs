using System;
using System.Runtime.InteropServices;

namespace DLLInjector.Injectors
{
    public class LoadLibraryInjector
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        public static bool Inject(int processID, string dllPath)
        {
            IntPtr hProcess = OpenProcess(0x001F0FFF, false, processID); 
            if (hProcess == IntPtr.Zero)
                return false;

            IntPtr allocatedMemory = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)((dllPath.Length + 1) * Marshal.SizeOf(typeof(char))), 0x3000, 0x40); // MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE
            if (allocatedMemory == IntPtr.Zero)
                return false;

            byte[] dllBytes = System.Text.Encoding.ASCII.GetBytes(dllPath);
            WriteProcessMemory(hProcess, allocatedMemory, dllBytes, (uint)dllBytes.Length, out IntPtr bytesWritten);

            IntPtr loadLibraryAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            if (loadLibraryAddr == IntPtr.Zero)
                return false;

            IntPtr remoteThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, loadLibraryAddr, allocatedMemory, 0, IntPtr.Zero);
            if (remoteThread == IntPtr.Zero)
                return false;

            CloseHandle(hProcess);
            return true;
        }
    }
}
