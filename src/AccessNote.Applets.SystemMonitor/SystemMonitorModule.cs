using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace AccessNote;

internal sealed class SystemMonitorModule
{
    private TextBlock? _cpuText;
    private TextBlock? _memoryText;
    private ItemsControl? _diskList;
    private DispatcherTimer? _timer;
    private readonly ObservableCollection<string> _diskEntries = new();
    private TimeSpan _lastCpuTime;
    private DateTime _lastCheckTime;
    private int _focusedSection;
    private Action<string>? _announce;

    public void Enter(TextBlock cpuText, TextBlock memoryText, ItemsControl diskList, Action<string> announce)
    {
        _cpuText = cpuText;
        _memoryText = memoryText;
        _diskList = diskList;
        _diskList.ItemsSource = _diskEntries;
        _focusedSection = 0;
        _announce = announce;

        var process = Process.GetCurrentProcess();
        _lastCpuTime = process.TotalProcessorTime;
        _lastCheckTime = DateTime.UtcNow;

        UpdateDisplay();

        _announce($"System Monitor. {_cpuText.Text}. {_memoryText.Text}.");

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    public void RestoreFocus()
    {
    }

    public bool HandleInput(Key key, ModifierKeys modifiers)
    {
        if (key == Key.F5)
        {
            UpdateDisplay();
            AnnounceFocusedSection();
            return true;
        }

        if (key == Key.Up)
        {
            if (_focusedSection > 0) _focusedSection--;
            AnnounceFocusedSection();
            return true;
        }

        if (key == Key.Down)
        {
            if (_focusedSection < 2) _focusedSection++;
            AnnounceFocusedSection();
            return true;
        }

        return false;
    }

    private void AnnounceFocusedSection()
    {
        var text = _focusedSection switch
        {
            0 => _cpuText?.Text ?? "CPU: unavailable",
            1 => _memoryText?.Text ?? "Memory: unavailable",
            2 => _diskEntries.Count > 0 ? string.Join(". ", _diskEntries) : "No disks available",
            _ => string.Empty
        };
        _announce?.Invoke(text);
    }

    public bool CanLeave()
    {
        return true;
    }

    public void Stop()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Tick -= OnTimerTick;
            _timer = null;
        }
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        UpdateCpu();
        UpdateMemory();
        UpdateDisks();
    }

    private void UpdateCpu()
    {
        if (_cpuText == null) return;

        try
        {
            var process = Process.GetCurrentProcess();
            var currentCpuTime = process.TotalProcessorTime;
            var currentTime = DateTime.UtcNow;
            var elapsed = currentTime - _lastCheckTime;

            double cpuPercent = 0;
            if (elapsed.TotalMilliseconds > 0)
            {
                var cpuUsed = (currentCpuTime - _lastCpuTime).TotalMilliseconds;
                cpuPercent = cpuUsed / (elapsed.TotalMilliseconds * Environment.ProcessorCount) * 100;
                cpuPercent = Math.Min(cpuPercent, 100);
            }

            _lastCpuTime = currentCpuTime;
            _lastCheckTime = currentTime;

            _cpuText.Text = $"CPU: {cpuPercent:F1}% (process, {Environment.ProcessorCount} cores)";
        }
        catch
        {
            _cpuText.Text = "CPU: unavailable";
        }
    }

    private void UpdateMemory()
    {
        if (_memoryText == null) return;

        try
        {
            var process = Process.GetCurrentProcess();
            double usedGb = process.WorkingSet64 / (1024.0 * 1024 * 1024);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var memStatus = new MEMORYSTATUSEX { dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>() };
                if (GlobalMemoryStatusEx(ref memStatus))
                {
                    double totalGb = memStatus.ullTotalPhys / (1024.0 * 1024 * 1024);
                    double systemUsedGb = (memStatus.ullTotalPhys - memStatus.ullAvailPhys) / (1024.0 * 1024 * 1024);
                    double percent = (double)(memStatus.ullTotalPhys - memStatus.ullAvailPhys) / memStatus.ullTotalPhys * 100;
                    _memoryText.Text = $"Memory: {systemUsedGb:F1} / {totalGb:F1} GB ({percent:F0}%)";
                    return;
                }
            }

            var gcInfo = GC.GetGCMemoryInfo();
            double totalGcGb = gcInfo.TotalAvailableMemoryBytes / (1024.0 * 1024 * 1024);
            double percentGc = totalGcGb > 0 ? usedGb / totalGcGb * 100 : 0;
            _memoryText.Text = $"Memory: {usedGb:F1} / {totalGcGb:F1} GB ({percentGc:F0}%)";
        }
        catch
        {
            _memoryText.Text = "Memory: unavailable";
        }
    }

    private void UpdateDisks()
    {
        if (_diskList == null) return;

        try
        {
            _diskEntries.Clear();
            var drives = DriveInfo.GetDrives();
            foreach (var drive in drives)
            {
                if (drive.DriveType == DriveType.Fixed && drive.IsReady)
                {
                    double totalGb = drive.TotalSize / (1024.0 * 1024 * 1024);
                    double usedGb = (drive.TotalSize - drive.TotalFreeSpace) / (1024.0 * 1024 * 1024);
                    double percent = (double)(drive.TotalSize - drive.TotalFreeSpace) / drive.TotalSize * 100;
                    var name = drive.Name.TrimEnd('\\');
                    _diskEntries.Add($"{name} {usedGb:F1} / {totalGb:F1} GB ({percent:F0}%)");
                }
            }
        }
        catch
        {
            _diskEntries.Clear();
            _diskEntries.Add("Disk information unavailable");
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

    [StructLayout(LayoutKind.Sequential)]
    private struct MEMORYSTATUSEX
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
    }
}
