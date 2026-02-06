using System;
using System.Runtime.InteropServices;

namespace ServiceControllerExtensions;

internal static partial class Interop
{
    internal static class Errors
    {
        internal const int ErrorInsufficientBuffer = 122;
    }
    
    internal static class ServiceConfigOptions
    {
        internal const uint DelayedAutoStartInfo = 3;
    }
    
    internal const uint ServiceNoChange = 0xFFFFFFFF;
    
    [LibraryImport("advapi32.dll", EntryPoint = "QueryServiceConfigW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool QueryServiceConfig(
        SafeHandle hService,
        IntPtr lpBuffer,
        int bufferSize,
        out int bytesNeeded);
    
    [LibraryImport("advapi32.dll", EntryPoint = "QueryServiceConfig2W", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool QueryServiceConfig2(
        SafeHandle hService,
        uint infoLevel,
        IntPtr lpBuffer,
        int bufferSize,
        out int bytesNeeded);
    
    [LibraryImport("advapi32.dll", EntryPoint = "ChangeServiceConfigW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool ChangeServiceConfig(
        SafeHandle hService,
        uint serviceType,
        uint startType,
        uint errorControl,
        string? binaryPathName,
        string? loadOrderGroup,
        IntPtr tagId,
        [In] char[]? dependencies,
        string? serviceStartName,
        string? password,
        string? displayName);


    [LibraryImport("advapi32.dll", EntryPoint = "ChangeServiceConfig2W", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool ChangeServiceConfig2(
        SafeHandle hService,
        uint infoLevel,
        IntPtr lpBuffer);
    
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct ServiceConfigInfo
    {
        internal uint ServiceType;
        internal uint StartType;
        internal uint ErrorControl;
        internal string BinaryPathName;
        internal string LoadOrderGroup;
        internal uint TagId;
        internal string Dependencies;
        internal string ServiceStartName;
        internal string DisplayName;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ServiceDelayedAutoStartInfo
    {
        internal bool DelayedAutostart;
    }
}

