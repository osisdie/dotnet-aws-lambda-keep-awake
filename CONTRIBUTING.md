# Contributing

Thanks for your interest in contributing!

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [AWS CLI v2](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html) (optional, for testing AWS features)

## Getting Started

```bash
git clone https://github.com/osisdie/dotnet-aws-lambda-keep-awake.git
cd dotnet-aws-lambda-keep-awake
dotnet restore
dotnet build
```

### Run Locally

```bash
cd AWSServerlessSleepless.Host
dotnet run
# Visit http://localhost:5001
```

## Making Changes

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/my-new-awaker`)
3. Make your changes
4. Ensure `dotnet build` passes
5. Commit with a descriptive message
6. Push and open a Pull Request

## Adding a New Awaker Plugin

1. Create a new project: `ServerlessSleepless.Awaker.YourPlugin`
2. Target `net8.0;net10.0`
3. Reference `ServerlessSleepless.Awaker.Common`
4. Inherit from `SelfAwakerServiceBase`
5. Implement `InitializeIfEnabled()` and `TryAwake()`
6. Add configuration section in `appsettings.json`
7. Register the assembly in `Startup.cs`

## Code Style

- Follow existing patterns in the codebase
- Use `CamelCase` for public members
- Keep awaker plugins lightweight and focused

## Questions?

Open an issue and we'll be happy to help!
