using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using TimeoutException = System.TimeoutException;

namespace ServiceControllerExtensions;

[SupportedOSPlatform("windows")]
public static class ServiceControllerExtensions
{
    extension(ServiceController controller)
    {
        public async Task WaitForStatusAsync(
            ServiceControllerStatus desiredStatus, 
            TimeSpan timeout, 
            CancellationToken cancellationToken = default)
        {
            DateTime start = DateTime.UtcNow;
            controller.Refresh();
            while (controller.Status != desiredStatus)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                if (DateTime.UtcNow - start > timeout)
                {
                    throw new TimeoutException("The operation has timed out.");
                }
                
                await Task.Delay(250, cancellationToken);
                controller.Refresh();
            }
        }

        public WindowsServiceStartMode StartupType
        {
            get => controller.GetStartupType();
            set => controller.SetStartupType(value);
        }

        public string AccountName
        {
            get => controller.GetServiceAccount();
            set => controller.SetServiceAccount(value);
        }

        public void SetPassword(string password)
        {
            var success = Interop.ChangeServiceConfig(
                controller.ServiceHandle,
                Interop.ServiceNoChange,
                Interop.ServiceNoChange,
                Interop.ServiceNoChange,
                null,
                null,
                IntPtr.Zero,
                null,
                null,
                password,
                null);
            if (!success)
            {
                throw new Win32Exception();
            }
        }

        private WindowsServiceStartMode GetStartupType()
        {
            var startType = controller.StartType;
            if (startType != ServiceStartMode.Automatic)
            {
                return (WindowsServiceStartMode)startType;
            }

            var success = Interop.QueryServiceConfig2(
                controller.ServiceHandle,
                Interop.ServiceConfigOptions.DelayedAutoStartInfo,
                IntPtr.Zero,
                0,
                out var bytesNeeded);
            var errorCode = Marshal.GetLastWin32Error();
            if (!success && errorCode != Interop.Errors.ErrorInsufficientBuffer)
            {
                throw new Win32Exception();
            }

            var buffer = Marshal.AllocHGlobal(bytesNeeded);
            try
            {
                success = Interop.QueryServiceConfig2(
                    controller.ServiceHandle,
                    Interop.ServiceConfigOptions.DelayedAutoStartInfo,
                    buffer,
                    bytesNeeded,
                    out bytesNeeded);
                if (!success)
                {
                    throw new Win32Exception();
                }
                    
                var info = Marshal.PtrToStructure<Interop.ServiceDelayedAutoStartInfo>(buffer);
                return info.DelayedAutostart 
                    ? WindowsServiceStartMode.AutomaticDelayedStart 
                    : WindowsServiceStartMode.Automatic;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private void SetStartType(ServiceStartMode mode)
        {
            var success = Interop.ChangeServiceConfig(
                controller.ServiceHandle,
                Interop.ServiceNoChange,
                (uint)mode,
                Interop.ServiceNoChange,
                null,
                null,
                IntPtr.Zero,
                null,
                null,
                null,
                null);
            if (!success)
            {
                throw new Win32Exception();
            }
        }

        private void SetStartupType(WindowsServiceStartMode mode)
        {
            var startType = mode;
            if (startType != WindowsServiceStartMode.AutomaticDelayedStart)
            {
                controller.SetStartType((ServiceStartMode)mode);
                return;
            }

            controller.SetStartType(ServiceStartMode.Automatic);

            var info = new Interop.ServiceDelayedAutoStartInfo
            {
                DelayedAutostart = true,
            };
            
            var buffer = Marshal.AllocHGlobal(Marshal.SizeOf<Interop.ServiceDelayedAutoStartInfo>());
            try
            {
                Marshal.StructureToPtr(info, buffer, false);
                var success = Interop.ChangeServiceConfig2(
                    controller.ServiceHandle,
                    Interop.ServiceConfigOptions.DelayedAutoStartInfo,
                    buffer);
                if (!success)
                {
                    throw new Win32Exception();
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private string GetServiceAccount()
        {
            var info = controller.GetServiceConfigInfo();
            return info.ServiceStartName;
        }
        
        private void SetServiceAccount(string accountName)
        {
            var success = Interop.ChangeServiceConfig(
                controller.ServiceHandle,
                Interop.ServiceNoChange,
                Interop.ServiceNoChange,
                Interop.ServiceNoChange,
                null,
                null,
                IntPtr.Zero,
                null,
                accountName,
                null,
                null);
            if (!success)
            {
                throw new Win32Exception();
            }
        }
        
        private Interop.ServiceConfigInfo GetServiceConfigInfo()
        {
            var success = Interop.QueryServiceConfig(
                controller.ServiceHandle,
                IntPtr.Zero,
                0,
                out var bytesNeeded);
            var errorCode = Marshal.GetLastWin32Error();
            if (!success && errorCode != Interop.Errors.ErrorInsufficientBuffer)
            {
                throw new Win32Exception();
            }

            var buffer = Marshal.AllocHGlobal(bytesNeeded);
            try
            {
                success = Interop.QueryServiceConfig(
                    controller.ServiceHandle,
                    buffer,
                    bytesNeeded,
                    out bytesNeeded);
                if (!success)
                {
                    throw new Win32Exception();
                }
                    
                var info = Marshal.PtrToStructure<Interop.ServiceConfigInfo>(buffer);
                return info;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }
}

[SupportedOSPlatform("windows")]
public enum WindowsServiceStartMode
{
    Boot = ServiceStartMode.Boot,
    System = ServiceStartMode.System,
    Automatic = ServiceStartMode.Automatic,
    Manual = ServiceStartMode.Manual,
    Disabled = ServiceStartMode.Disabled,
    AutomaticDelayedStart = 10,
}