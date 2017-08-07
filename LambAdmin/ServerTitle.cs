/*
******************************~**********************************
******************+**+**++***+++***++**+**+**+*******************
***************+**+**++**+++ made by +++***++**+*****************
************~*++**+++ Frederica Bernkastel *+++**++*~************
*****************************************************************
************************* nipa~( =^_^= ) ************************
*****************************************************************
*****~*~*~~~ https://github.com/FredericaBernkastel ~~~*~*~******
*****************************************************************
*****************************************************************
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
            private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr buffer, uint size, int lpNumberOfBytesRead);
            [DllImport("kernel32.dll")]
            protected static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, int dwLength);
            [DllImport("kernel32.dll", SetLastError = true)]
            static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);
            [DllImport("kernel32.dll", SetLastError = true)]
            static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize, uint flNewProtect, out uint lpflOldProtect);
            [DllImport("kernel32.dll")]
            protected static extern int OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);
            [DllImport("kernel32.dll")]
            public static extern bool CloseHandle(int hObject);

            private const int PROCESS_VM_WRITE = 0x0020;
            private const int PROCESS_VM_OPERATION = 0x0008;

            private const uint PAGE_EXECUTE = 16;
            private const uint PAGE_EXECUTE_READ = 32;
            private const uint PAGE_EXECUTE_READWRITE = 64;
            private const uint PAGE_EXECUTE_WRITECOPY = 128;
            private const uint PAGE_GUARD = 256;
            private const uint PAGE_NOACCESS = 1;
            private const uint PAGE_NOCACHE = 512;
            private const uint PAGE_READONLY = 2;
            private const uint PAGE_READWRITE = 4;
            private const uint PAGE_WRITECOPY = 8;
            private const uint PROCESS_ALL_ACCESS = 2035711;

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

            private bool MaskCheck(int nOffset, IntPtr data, ref byte[] btPattern)
            {
                // Loop the pattern and compare to the dump
                for (int x = 0; x < btPattern.Length; x++)
                {
                    unsafe {
                        if (btPattern[x] != (*(byte*)(data + nOffset + x)))
                            return false;
                    }
                }

                // The loop was successful so we found the pattern.
                return true;
            }

            private List<IntPtr> _Scan(IntPtr sIn, ref byte[] sFor, int length)
            {
                List<IntPtr> result = new List<IntPtr>();

                for (int x = 0; x < length - sFor.Length; x++)
                {
                    if (MaskCheck(x, sIn, ref sFor))
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
                    if (MemReg[i].RegionSize == 0)
                        continue;

                    unsafe
                    {
                        IntPtr buff = Marshal.AllocHGlobal((int)MemReg[i].RegionSize);

                        ReadProcessMemory(P.Handle, MemReg[i].BaseAddress, buff, MemReg[i].RegionSize, 0);
                        List<IntPtr> Result = _Scan(buff, ref Pattern, (int)MemReg[i].RegionSize);

                        Marshal.FreeHGlobal(buff);

                        if (Result.Count > 0)
                        {
                            Result.ForEach(s =>
                            {
                                result.Add(new IntPtr(MemReg[i].BaseAddress.ToInt32() + s.ToInt32()));
                            });
                        }
                    }
                }
                return result;
            }

            public void WriteMem(int pOffset, byte[] pBytes)
            {
                int processHandle = OpenProcess(PROCESS_ALL_ACCESS, false, Process.GetCurrentProcess().Id);
                int BytesWritten = 0;
                WriteProcessMemory(processHandle, pOffset, pBytes, pBytes.Length, ref BytesWritten);
                CloseHandle(processHandle);
                
            }
        }

        public bool UTILS_ServerTitle(string MapName, string ModeName)
        {

            Regex rgx = new Regex(@"^gn\\IW5\\gt\\([^\\].*?\\){27}$");

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
                        Match match = rgx.Match(structure);
                        if (match.Success)
                            pass1.Add(addr);
                    }
                });

                List<IntPtr> pass2 = new List<IntPtr>();

                WriteLog.Debug("ServerTitle:: addrs: " + String.Join(", ", pass1.ConvertAll<string>((s) => { return "0x" + (s.ToInt32().ToString("X")); }).ToArray()));

                //step 2.  (black magic)
                int gap_min = 0x700, gap_max = 0x7FF;
                if (pass1.Count < 2)
                    return pass2;
                else
                {
                    int[] _addrs = pass1.ConvertAll(s => { return (int)s; }).ToArray();
                    for(int i = 1; i < _addrs.Length; i++)
                    {
                        if(((_addrs[i] - _addrs[i - 1]) >= gap_min) && ((_addrs[i] - _addrs[i - 1]) <= gap_max))
                        {
                            pass2.Add(addrs[i]);
                            return pass2;
                        }
                    }
                }

                return pass2;
            };

            Action<IntPtr> Write = (addr) =>
            {
                Func<string,string> construct = (_structure) =>
                {

                    // no, Carl, this is not a brainfuck
                    Regex _rgx = new Regex(@"^(gn\\IW5\\gt\\)([^\\].*?)\\(([^\\].*?\\){5})([^\\].*?)\\(([^\\].*?\\){20})$");
                    Match match = _rgx.Match(_structure);

                    /* 
                     * restore default map & mode strings in this case
                     * ConfigValues.mapname == Call<string>("getdvar", mapname);
                     * ConfigValues.g_gametype == Call<string>("getdvar", g_gametype);
                     */
                    ModeName = String.IsNullOrEmpty(ModeName) ? ConfigValues.g_gametype : ModeName;
                    MapName = String.IsNullOrEmpty(MapName) ? ConfigValues.mapname : MapName;

                    _structure = match.Groups[1].Value + ModeName + "\\" + match.Groups[3].Value + MapName + "\\" + match.Groups[6].Value;
                    return _structure;

                };

                if ((int)addr <= 0)
                    return;
                if(MapName.Length > 28)
                {
                    WriteLog.Warning("ServerTitle:: MapName overflow. Max length is 28 chars!");
                    MapName = MapName.Substring(0, 28);
                }
                if (ModeName.Length > 15)
                {
                    WriteLog.Warning("ServerTitle:: ModeName overflow. Max length is 15 chars!");
                    ModeName = ModeName.Substring(0, 15);
                }
                string structure = Mem.ReadString((int)addr, 128);
                if (!rgx.Match(structure).Success)
                    return;

                structure = construct(structure);
                if ((structure.Length >= 128) && (MapName.Length > 20))
                {
                    //maybe it will help...
                    MapName = MapName.Substring(0, 20);
                    structure = construct(structure);
                }

                List<byte> data = Encoding.ASCII.GetBytes(structure).ToList();
                data.Add(0);
                if (data.Count <= 128)
                    (new AobScan()).WriteMem((int)addr, data.ToArray());
                else
                    WriteLog.Warning(String.Format("ServerTitle:: structure overflow 128, but got {0} bytes.", data.Count.ToString()));
            };

            /* Once found, the address wont change in future
             * so we'll store it as a server dvar
             */
            string sv_serverinfo_addr = UTILS_GetDefCDvar("sv_serverinfo_addr");
            if (String.IsNullOrEmpty(sv_serverinfo_addr)) //first start
            {
                // find teh addrs
                FindAddr(new Action<List<IntPtr>>((addrs) => {

                    addrs = Filter(addrs);
                    WriteLog.Debug("ServerTitle:: addrs(filter): " + String.Join(", ", addrs.ConvertAll<string>((s) => { return "0x" + (s.ToInt32().ToString("X")); }).ToArray()));

                    if (addrs.Count == 1)
                    {

                        IntPtr addr = addrs.First();

                        //save found address
                        Call("setdvar", "sv_serverinfo_addr", new Parameter((int)addr.ToInt32()));

                        Write(addr);
                    }
                    else
                    {
                        WriteLog.Warning("ServerTitle:: structure not found");
                        Call("setdvar", "sv_serverinfo_addr", new Parameter((int)0)); //addr no found, skip search in future
                    }
                    WriteLog.Debug("ServerTitle:: done scanning.");
                    
                }));
            }
            else
            {
                Thread _thr = new Thread(() =>
                {
                    Thread.Sleep(1000); // in case of fast restart, default AfterInterval will be ignored

                    //skip search, just load from sdvar
                    int addr = int.Parse(sv_serverinfo_addr);
                    if (addr > 0)
                    {
                        WriteLog.Debug("ServerTitle:: addr: 0x" + addr.ToString("X"));
                        Write(new IntPtr(addr));
                    }
                });

                _thr.Start();
            }

            return false;
        }
    }
}
