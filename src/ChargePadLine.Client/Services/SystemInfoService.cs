using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;

namespace ChargePadLine.Client.Services
{
    public interface ISystemInfoService
    {
        double GetCpuUsage();
        double GetMemoryUsage();
        double GetDiskUsage();
    }

    public class SystemInfoService : ISystemInfoService, IDisposable
    {
        private PerformanceCounter? _cpuCounter;
        private PerformanceCounter? _memoryCounter;
        private readonly string _driveLetter;

        public SystemInfoService()
        {
            // 获取系统盘符，通常是C盘
            _driveLetter = Environment.GetFolderPath(Environment.SpecialFolder.System).Substring(0, 2);
            
            try
            {
                // 初始化CPU使用率计数器
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _cpuCounter.NextValue(); // 第一次调用总是返回0，需要调用两次
                
                // 初始化内存使用率计数器
                _memoryCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");
            }
            catch (Exception)
            {
                // 如果PerformanceCounter初始化失败，将计数器设为null
                _cpuCounter?.Dispose();
                _memoryCounter?.Dispose();
                _cpuCounter = null;
                _memoryCounter = null;
            }
        }

        public double GetCpuUsage()
        {
            try
            {
                if (_cpuCounter != null)
                {
                    return Math.Round(_cpuCounter.NextValue(), 1);
                }
                else
                {
                    // 使用WMI作为备选方案
                    return GetCpuUsageViaWMI();
                }
            }
            catch
            {
                return 0.0;
            }
        }

        public double GetMemoryUsage()
        {
            try
            {
                if (_memoryCounter != null)
                {
                    return Math.Round(_memoryCounter.NextValue(), 1);
                }
                else
                {
                    // 使用WMI作为备选方案
                    return GetMemoryUsageViaWMI();
                }
            }
            catch
            {
                return 0.0;
            }
        }

        public double GetDiskUsage()
        {
            try
            {
                DriveInfo drive = new DriveInfo(_driveLetter);
                if (drive.IsReady)
                {
                    long totalSize = drive.TotalSize;
                    long availableSize = drive.AvailableFreeSpace;
                    long usedSize = totalSize - availableSize;
                    double usagePercentage = (double)usedSize / totalSize * 100.0;
                    return Math.Round(usagePercentage, 1);
                }
                return 0.0;
            }
            catch
            {
                return 0.0;
            }
        }

        private double GetCpuUsageViaWMI()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT LoadPercentage FROM Win32_Processor"))
                {
                    var collection = searcher.Get();
                    int cpuCount = 0;
                    int totalUsage = 0;
                    
                    foreach (var obj in collection)
                    {
                        cpuCount++;
                        totalUsage += Convert.ToInt32(obj["LoadPercentage"]);
                    }
                    
                    return cpuCount > 0 ? Math.Round((double)totalUsage / cpuCount, 1) : 0.0;
                }
            }
            catch
            {
                return 0.0;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
            public MEMORYSTATUSEX()
            {
                this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        private double GetMemoryUsageViaWMI()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize, FreePhysicalMemory, TotalVirtualMemorySize, FreeVirtualMemory FROM Win32_OperatingSystem"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        var totalMemory = Convert.ToInt64(obj["TotalVisibleMemorySize"]) * 1024; // KB to bytes
                        var freeMemory = Convert.ToInt64(obj["FreePhysicalMemory"]) * 1024; // KB to bytes
                        var usedMemory = totalMemory - freeMemory;
                        
                        // 计算内存使用率，与任务管理器更接近
                        return Math.Round((double)usedMemory / totalMemory * 100.0, 1);
                    }
                }
            }
            catch
            {
                // 如果WMI也失败，则使用Windows API获取内存信息
                try
                {
                    MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
                    if (GlobalMemoryStatusEx(memStatus))
                    {
                        var totalMemory = memStatus.ullTotalPhys;
                        var availableMemory = memStatus.ullAvailPhys;
                        var usedMemory = totalMemory - availableMemory;
                        return Math.Round((double)usedMemory / totalMemory * 100.0, 1);
                    }
                }
                catch
                {
                    return 0.0;
                }
            }
            
            return 0.0;
        }

        public void Dispose()
        {
            _cpuCounter?.Dispose();
            _memoryCounter?.Dispose();
        }
    }
}