using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DLLInjector.Injectors
{
    public class SetWindowsHookExInjector
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, IntPtr lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        private const uint PROCESS_ALL_ACCESS = 0x001F0FFF;
        private const uint MEM_COMMIT = 0x00001000;
        private const uint MEM_RESERVE = 0x00002000;
        private const uint PAGE_READWRITE = 0x04;
        private const int WH_GETMESSAGE = 3;

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

            IntPtr hModule = GetModuleHandle("user32.dll");
            if (hModule == IntPtr.Zero)
            {
                Console.WriteLine("Impossible de récupérer le handle de user32.dll.");
                CloseHandle(hProcess);
                return false;
            }

            IntPtr hookProcAddr = GetProcAddress(hModule, "CallNextHookEx");
            if (hookProcAddr == IntPtr.Zero)
            {
                Console.WriteLine("Impossible de récupérer l'adresse de CallNextHookEx.");
                CloseHandle(hProcess);
                return false;
            }

            IntPtr hHook = SetWindowsHookEx(WH_GETMESSAGE, hookProcAddr, hModule, 0);
            if (hHook == IntPtr.Zero)
            {
                Console.WriteLine("Échec de l'installation du hook.");
                CloseHandle(hProcess);
                return false;
            }

            CloseHandle(hProcess);
            Console.WriteLine("Injection réussie via SetWindowsHookEx.");
            return true;
        }
    }
}

