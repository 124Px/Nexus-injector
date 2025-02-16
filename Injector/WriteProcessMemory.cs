using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DLLInjector.Injectors
{
    public class WriteProcessMemoryInjector
    {
        // Déclarations des API Windows nécessaires
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize,
            IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        // Constantes pour OpenProcess et VirtualAllocEx
        private const uint PROCESS_ALL_ACCESS = 0x001F0FFF;
        private const uint MEM_COMMIT = 0x00001000;
        private const uint MEM_RESERVE = 0x00002000;
        private const uint PAGE_READWRITE = 0x04;

        public static bool Inject(int processID, string dllPath)
        {
            IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, processID);
            if (hProcess == IntPtr.Zero)
            {
                Console.WriteLine("Échec de l'ouverture du processus.");
                return false;
            }

            IntPtr allocatedMemory = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)((dllPath.Length + 1) * sizeof(char)), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            if (allocatedMemory == IntPtr.Zero)
            {
                Console.WriteLine("Impossible d'allouer la mémoire dans le processus cible.");
                CloseHandle(hProcess);
                return false;
            }

            byte[] dllBytes = System.Text.Encoding.ASCII.GetBytes(dllPath);
            if (!WriteProcessMemory(hProcess, allocatedMemory, dllBytes, (uint)dllBytes.Length, out _))
            {
                Console.WriteLine("Échec de l'écriture en mémoire.");
                CloseHandle(hProcess);
                return false;
            }

            IntPtr loadLibraryAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            if (loadLibraryAddr == IntPtr.Zero)
            {
                Console.WriteLine("Impossible de trouver LoadLibraryA.");
                CloseHandle(hProcess);
                return false;
            }

            IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, loadLibraryAddr, allocatedMemory, 0, IntPtr.Zero);
            if (hThread == IntPtr.Zero)
            {
                Console.WriteLine("Échec de la création du thread distant.");
                CloseHandle(hProcess);
                return false;
            }

            CloseHandle(hThread);
            CloseHandle(hProcess);

            Console.WriteLine("Injection réussie via WriteProcessMemory !");
            return true;
        }
    }
}

