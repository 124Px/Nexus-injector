using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace DLLInjector.Injectors
{
    public class ManualMapInjector
    {
        [DllImport("kernel32.dll", SetLastError = true)]
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

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtUnmapViewOfSection(IntPtr hProcess, IntPtr baseAddress);

        private const uint PROCESS_ALL_ACCESS = 0x001F0FFF;
        private const uint MEM_COMMIT = 0x00001000;
        private const uint MEM_RESERVE = 0x00002000;
        private const uint PAGE_EXECUTE_READWRITE = 0x40;

        public static bool Inject(int processID, string dllPath)
        {
            if (!File.Exists(dllPath))
            {
                Console.WriteLine("Fichier DLL introuvable.");
                return false;
            }

            byte[] dllBytes = File.ReadAllBytes(dllPath);
            IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, processID);
            if (hProcess == IntPtr.Zero)
            {
                Console.WriteLine("Échec de l'ouverture du processus.");
                return false;
            }

            IntPtr allocatedMemory = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)dllBytes.Length, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
            if (allocatedMemory == IntPtr.Zero)
            {
                Console.WriteLine("Échec de l'allocation mémoire.");
                CloseHandle(hProcess);
                return false;
            }

            if (!WriteProcessMemory(hProcess, allocatedMemory, dllBytes, (uint)dllBytes.Length, out _))
            {
                Console.WriteLine("Échec de l'écriture en mémoire.");
                CloseHandle(hProcess);
                return false;
            }

            IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, allocatedMemory, IntPtr.Zero, 0, IntPtr.Zero);
            if (hThread == IntPtr.Zero)
            {
                Console.WriteLine("Échec de la création du thread distant.");
                CloseHandle(hProcess);
                return false;
            }

            CloseHandle(hThread);
            CloseHandle(hProcess);

            Console.WriteLine("Injection réussie via Manual Map.");
            return true;
        }
    }
}

