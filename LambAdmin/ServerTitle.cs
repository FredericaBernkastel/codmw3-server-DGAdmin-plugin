using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading;
using InfinityScript;
using System.Text.RegularExpressions;

namespace LambAdmin
{
    public partial class DGAdmin
    {
        public class AobScan
        {
            [DllImport("kernel32.dll")]
            private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, int lpNumberOfBytesRead);
            [DllImport("kernel32.dll")]
            protected static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, int dwLength);
            [DllImport("kernel32.dll", SetLastError = true)]
            static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress,
              byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);
            [DllImport("kernel32.dll", SetLastError = true)]
            static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize,
                       uint flNewProtect, out uint lpflOldProtect);

            const int PROCESS_VM_WRITE = 0x0020;
            const int PROCESS_VM_OPERATION = 0x0008;
            const int PAGE_READWRITE = 0x04;

            [StructLayout(LayoutKind.Sequential)]
            protected struct MEMORY_BASIC_INFORMATION
            {
                public IntPtr BaseAddress;
                public IntPtr AllocationBase;
                public uint AllocationProtect;
                public uint RegionSize;
                public uint State;
                public uint Protect;
                public uint Type;
            }
            List<MEMORY_BASIC_INFORMATION> MemReg { get; set; }

            private void MemInfo(IntPtr pHandle)
            {
                IntPtr Addy = new IntPtr();
                while (true)
                {
                    MEMORY_BASIC_INFORMATION MemInfo = new MEMORY_BASIC_INFORMATION();
                    int MemDump = VirtualQueryEx(pHandle, Addy, out MemInfo, Marshal.SizeOf(MemInfo));
                    if (MemDump == 0) break;
                    if ((MemInfo.State & 0x1000) != 0 && (MemInfo.Protect & 0x100) == 0)
                        MemReg.Add(MemInfo);
                    Addy = new IntPtr(MemInfo.BaseAddress.ToInt32() + MemInfo.RegionSize);
                }
            }

            private bool MaskCheck(int nOffset, ref byte[] data, ref byte[] btPattern)
            {
                // Loop the pattern and compare to the mask and dump.
                for (int x = 0; x < btPattern.Length; x++)
                {
                    // If the mask char is not a wildcard, ensure a match is made in the pattern.
                    if (btPattern[x] != data[nOffset + x])
                        return false;
                }

                // The loop was successful so we found the pattern.
                return true;
            }

            private List<IntPtr> _Scan(ref byte[] sIn, ref byte[] sFor)
            {
                List<IntPtr> result = new List<IntPtr>();

                for (int x = 0; x < sIn.Length - sFor.Length; x++)
                {
                    if (MaskCheck(x, ref sIn, ref sFor))
                    {
                        // The pattern was found, return it.
                        result.Add(new IntPtr(x));
                    }
                }

                return result;
            }
            public List<IntPtr> Scan(byte[] Pattern)
            {
                Process P = Process.GetCurrentProcess();
                MemReg = new List<MEMORY_BASIC_INFORMATION>();
                MemInfo(P.Handle);

                List<IntPtr> result = new List<IntPtr>();
                for (int i = 0; i < MemReg.Count; i++)
                {
                    byte[] buff = new byte[MemReg[i].RegionSize];
                    ReadProcessMemory(P.Handle, MemReg[i].BaseAddress, buff, MemReg[i].RegionSize, 0);
                    List<IntPtr> Result = _Scan(ref buff, ref Pattern);
                    if (Result.Count > 0)
                    {
                        Result.ForEach(s =>
                        {
                            result.Add(new IntPtr(MemReg[i].BaseAddress.ToInt32() + s.ToInt32()));
                        });
                    }
                }
                return result;
            }

        }

        public bool UTILS_ServerTitle(string MapName, string ModeName)
        {
            Action<Action<List<IntPtr>>> FindAddr = (callback) => {

                WriteLog.Debug("ServerTitle:: start scanning...");

                // We'll use threading to avoid LAGGGGGS
                Thread _thr = new Thread(() =>
                {
                    callback(new AobScan().Scan(new byte[] { 0x67, 0x6E, 0x5C, 0x49, 0x57, 0x35, 0x5C, 0x67, 0x74, 0x5C }));
                });

                // start the search thread
                _thr.Start();
            };

            //Filter the addrs
            Func<List<IntPtr>, List<IntPtr>> Filter = (addrs) =>
            {
                List<IntPtr> pass1 = new List<IntPtr>();


                // step 1. - lower than 0xC000000, should match pattern
                addrs.ForEach(addr =>
                {
                    if ((int)addr <= 0xC000000)
                    {
                        string structure = Mem.ReadString((int)addr, 128);
                        Regex rgx = new Regex(@"^gn\\IW5\\gt\\([^\\].*?\\){27}$");
                        Match match = rgx.Match(structure);
                        if (match.Success)
                            pass1.Add(addr);
                    }
                });

                List<IntPtr> pass2 = new List<IntPtr>();

                //step 2. - Test for lathness
                unsafe
                {
                    // write test bytes
                    pass1.ForEach(addr => {


                        // >>>> HERE <<<<<
                        *(byte*)(addr + 10) = 0x61;
                        // >>>> HERE <<<<<


                        WriteLog.Debug(String.Format("0x{0}: {1}", (addr + 10).ToString("X"), (*(byte*)(addr + 10)).ToString("X")));
                    });

                    WriteLog.Debug("Delay(1000)");

                    AfterDelay(1000, () =>
                    {
                        // read test bytes
                        pass1.ForEach(addr =>
                        {
                            WriteLog.Debug(String.Format("0x{0}: {1}", (addr + 10).ToString("X"), (*(byte*)(addr + 10)).ToString("X")));
                        });
                    });
                }
                return pass1;
            };

            /* Once found, the address wont change in future
             * so we'll store it as a server dvar
             */
            string sv_serverinfo_addr = UTILS_GetDefCDvar("sv_serverinfo_addr");
            if (String.IsNullOrEmpty(sv_serverinfo_addr)) //first start
            {
                // find teh addrs
                FindAddr(new Action<List<IntPtr>>((addrs) => {
                    if (addrs.Count != 0)
                    {
                        addrs = Filter(addrs);

                        WriteLog.Debug("ServerTitle:: addrs: " + String.Join(", ", addrs.ConvertAll<string>((s) => { return "0x" + (s.ToInt32().ToString("X")); }).ToArray()));

                        //assume its 2nd
                        IntPtr addr = (addrs.Count > 1) ? addrs.ElementAt(1) : addrs.ElementAt(0);

                        //save found address
                        Call("setdvar", "sv_serverinfo_addr", new Parameter((int)addr.ToInt32()));
                    }
                    else
                    {
                        WriteLog.Warning("ServerTitle:: structure not found");
                        Call("setdvar", "sv_serverinfo_addr", new Parameter((int)0)); //just skip dis shit in future
                    }
                    WriteLog.Debug("ServerTitle:: done scanning.");
                }));
            }
            else
            {
                //skip search, just load from sdvar
                int addr = int.Parse(sv_serverinfo_addr);
                if (addr > 0)
                {
                    WriteLog.Debug("ServerTitle:: addr: 0x" + addr.ToString("X"));
                }
            }

            return false;
        }
    }
}
