# TimeProvider Examples

This repository is an example project that demonstrates how to build and consume a custom time source in .NET using `TimeProvider`.

The example models a small distributed time simulation:

- a backend service owns the simulated time;
- the service can change the current simulated date/time;
- the service can speed up or slow down the flow of time using a scale factor;
- the simulated time is broadcast continuously over UDP;
- a WPF desktop application consumes that time stream and displays it in real time.

## Concept

The core idea of the project is to separate **real system time** from **application time**.

Instead of every component directly using `DateTime.Now`, `DateTime.UtcNow`, or `TimeProvider.System`, the application can depend on a `TimeProvider` abstraction. This makes it possible to control what "now" means for the application.

In this example, the simulated clock can run:

- at normal speed;
- faster than real time;
- slower than real time;
- from a manually selected date and time.

This is useful for scenarios such as:

- simulations;
- testing time-dependent logic;
- replaying historical timelines;
- synchronizing multiple clients to a shared virtual clock;
- demonstrating how `TimeProvider` can replace direct system clock usage.

## Projects

The solution contains the following main projects.

### `TimeProviderExample.Service`

An ASP.NET Core service that owns the simulated time.

It exposes HTTP endpoints for reading and changing the simulated time:

- `GET /time`  
  Returns the current simulated UTC time.

- `POST /time`  
  Sets the simulated start time.

- `GET /time/scale`  
  Returns the current time scale.

- `POST /time/scale`  
  Changes the time scale.

The service also runs a background worker that periodically broadcasts the current simulated time over UDP multicast.

### `TimeProviderExample.Client.Core`

A shared, UI-framework-independent class library consumed by both desktop clients.

It contains the reusable client logic:

- `UdpTimeProvider`, the custom `TimeProvider` fed by the UDP multicast stream;
- `MainViewModel`, the MVVM view-model driving the UI (simulated date/time, real system time, UDP/UI packet rates, scale and date/time control);
- `IMainWindowFactory` and `IUiDispatcher` abstractions so the view-model can spawn windows and marshal to the UI thread without depending on WPF or Avalonia.

### `TimeProviderExample.Wpf`

A WPF client application that consumes the simulated time (Windows only).

### `TimeProviderExample.Avalonia`

An [Avalonia UI](https://avaloniaui.net/) client application that consumes the simulated time. It is a functional port of the WPF client and runs on **Linux, macOS, and Windows**.

The UI (`MainWindow.axaml`) mirrors `MainWindow.xaml`, with the WPF-only constructs replaced by Avalonia equivalents:

- `StatusBar`/`StatusBarItem` replaced by a `Border` + `DockPanel` status strip;
- fixed-size `ToolWindow` replaced by a resizable window with `SizeToContent="WidthAndHeight"` (grows with the sidebar, minimum 600x400);
- `Window.InputBindings` replaced by `Window.KeyBindings`;
- `TextBlock.LayoutTransform` replaced by `LayoutTransformControl`;
- the `Expander` sidebar replaced by a `ToggleButton` strip (chevron + label rotate together) driving an `IsSidebarOpen` view-model property;
- `DatePicker.SelectedDate` bridged to the shared `DateTime` property via `DateTimeToDateTimeOffsetConverter`.

The client receives time updates from the UDP multicast stream and exposes them through a custom `TimeProvider`. The UI displays:

- the simulated date;
- the simulated time;
- the real system time;
- UDP packet rate;
- UI refresh rate;
- the current simulation scale.

The user can also adjust the simulated date, time, and speed from the interface.

### `TimeProviderExample.Service.Tests`

A test project that validates the behavior of the custom time provider service.

The tests cover scenarios such as:

- default initialization;
- custom start time;
- changing the scale factor;
- smooth scale transitions;
- timer behavior under scaled time;
- timestamp consistency.

### `TimeProviderExample.Contracts`

A shared contracts project intended for common types used between the service and clients.

At the moment, the example mainly communicates using primitive values such as `DateTimeOffset` and `double`.

## How It Works

The service maintains an internal time provider that calculates simulated time from:

1. the configured simulation start time;
2. the real elapsed system time;
3. the configured scale factor.

For example:

- with scale `1.0`, simulated time advances at real speed;
- with scale `10.0`, one real second represents ten simulated seconds;
- with scale `0.5`, two real seconds represent one simulated second.

The backend periodically sends the current simulated UTC ticks over UDP multicast on address `224.0.0.1` at port `6000`.

The WPF application listens to the multicast address and updates its local time provider whenever a new packet arrives. UI components can then read the current simulated time through the `TimeProvider` abstraction.

## Running the Example

Start the backend service first.

The service is expected to listen for HTTP requests and broadcast simulated time over UDP multicast.

Then start a client:

- on Windows: the WPF application (`TimeProviderExample.Wpf`);
- on Linux, macOS, or Windows: the Avalonia application (`TimeProviderExample.Avalonia`):
```bash
dotnet run --project TimeProviderExample.Avalonia
```

Once both are running, the client should display the simulated time coming from the service.

## Docker

The repository includes a `compose.yaml` file and a Dockerfile for the service project.

The service can be built as a container image using Docker Compose.
```bash
docker compose up --build
```

> [!NOTE]
> Depending on the environment, UDP multicast traffic may require additional network configuration when running inside containers.

## Main Learning Points

This example demonstrates:

- how to implement a custom `TimeProvider`
- how to model time scaling
- how to avoid hard dependencies on the system clock
- how to drive UI state from an injected time source
- how to synchronize clients with a shared simulated clock
- how to test time-dependent behavior without relying only on real wall-clock time

## Why `TimeProvider` Matters

Using `TimeProvider` makes time an injectable dependency.

That makes applications easier to:

- test
- simulate
- debug
- control
- synchronize

Instead of treating time as a global static resource, the application can treat it as part of its runtime configuration.
