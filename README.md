# ServiceController Extensions

Contains extension members for the ServiceController to support the `AutomaticDelayedStart` start mode and modification of account name and password.

## Requirements

You need to add the [`System.ServiceProcess.ServiceController`](https://www.nuget.org/packages/System.ServiceProcess.ServiceController/) package to you project so that you can use the `ServiceController`.

```xml
<ItemGroup>
    <PackageReference Include="System.ServiceProcess.ServiceController" Version="10.0.2" />
</ItemGroup>
```

The uses P/Invoke so you need ot allow unsafe blocks:

```xml
<PropertyGroup>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
</PropertyGroup>
```

## `ServiceController.WaitForStatusAsync()`

Using `Start()` method does not mean the service will be have the `Running` status when it completes. The built-in [`WaitForStatus()`](https://learn.microsoft.com/en-us/dotnet/api/system.serviceprocess.servicecontroller.waitforstatus) is synchronous. These extension methods contain a `WaitForStatusAsync`.

```csharp
using var controller = new ServiceController("mysvc");
controller.Start();
await controller.WaitForStatusAsync(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30), default);
```

## `ServiceController.StartupType`

The `StartupType` property supports `AutomaticDelayedStart` as a start mode.

```csharp
using var controller = new ServiceController("mysvc");
controller.StartupType = WindowsServiceStartMode.AutomaticDelayedStart
```

## `ServiceController.AccountName`

The `AccountName` property gets and sets the Windows account under which the service is run.

```csharp
using var controller = new ServiceController("mysvc");
controller.AccountName = WindowsServiceStartMode.AutomaticDelayedStart
```

## `ServiceController.SetPassword()`

The `SetPassword()` method sets the password of the account name.

