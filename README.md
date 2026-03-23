# dotnet-aws-lambda-keep-awake

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![GitHub Actions](https://github.com/osisdie/dotnet-aws-lambda-keep-awake/actions/workflows/dotnet.yml/badge.svg)](https://github.com/osisdie/dotnet-aws-lambda-keep-awake/actions)
[![GitHub last commit](https://img.shields.io/github/last-commit/osisdie/dotnet-aws-lambda-keep-awake)](https://github.com/osisdie/dotnet-aws-lambda-keep-awake/commits/main)
[![AWS Lambda](https://img.shields.io/badge/AWS-Lambda-orange)](https://aws.amazon.com/lambda/)
[![Awakers](https://img.shields.io/badge/Awakers-4-green)](#built-in-awaker-plugins)

> Keep your AWS Lambda functions warm with creative self-awakening strategies. No more cold starts!

A .NET 10 AWS Lambda Serverless WebAPI template with **4 built-in Awaker plugins** that periodically exercise your Lambda function to prevent cold start delays. Easily extensible with custom awakers.

*Part of the **code_for_fun** series*

---

## Architecture

```
                    +-----------------------+
                    |   API Gateway         |
                    |   /{proxy+}           |
                    +-----------+-----------+
                                |
                    +-----------v-----------+
                    |   Lambda Function     |
                    |   (ASP.NET Core 10)   |
                    +-----------+-----------+
                                |
                    +-----------v-----------+
                    |   SelfAwakeService    |
                    |   (Timer-based)       |
                    +---+---+---+---+-------+
                        |   |   |   |
              +---------+   |   |   +---------+
              |             |   |             |
        +-----v----+  +----v---v-+  +--------v---+
        | S3 Awaker|  |SQS Awaker|  | CPU Awaker |
        +----------+  +----------+  +------------+
                                    | MEM Awaker |
                                    +------------+
```

The `SelfAwakeService` runs a background timer that periodically invokes all enabled Awaker plugins. Each plugin performs a lightweight operation (S3 list, SQS poll, CPU/MEM burst) to keep the Lambda container active.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [AWS CLI v2](https://docs.aws.amazon.com/cli/latest/userguide/getting-started-install.html)
- [AWS SAM CLI](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html) (for deployment)
- An AWS account with Lambda, S3, and optionally SQS permissions

## Quick Start

### 1. Clone and restore

```bash
git clone https://github.com/osisdie/dotnet-aws-lambda-keep-awake.git
cd dotnet-aws-lambda-keep-awake
dotnet restore
```

### 2. Run locally

```bash
cd AWSServerlessSleepless.Host
dotnet run
# Listening on http://localhost:5001
```

### 3. Test the endpoint

```bash
curl http://localhost:5001/
# Returns JSON status with echo count, generated ID, and awaker stats
```

## Configuration

All awaker settings live in `appsettings.json`:

### Main Service (Orchestrator)

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `Enabled` | bool | `true` | Enable/disable the self-awake service |
| `DelayStartInSecs` | int | `1` | Seconds to wait before first awake cycle |
| `PeriodInSecs` | int | `60` | Interval between awake cycles |

```json
"ServerlessSleepless.Host.Services.SelfAwakeService": {
    "Enabled": true,
    "DelayStartInSecs": 1,
    "PeriodInSecs": 60
}
```

## Built-in Awaker Plugins

| # | Plugin | Strategy | Config Keys |
|---|--------|----------|-------------|
| 1 | **AccessS3** | Lists S3 bucket objects | `BucketName` |
| 2 | **SQSLoop** | Consumes & publishes SQS messages | `QueueUrl`, `Region`, `MaxNumberOfMessages` |
| 3 | **BurstCPU** | Shuffles large arrays to spike CPU | `MaxProcessCount` (default: 1,000,000) |
| 4 | **BurstMEM** | Allocates memory blocks | `MinMemSizeMB`, `MaxMemSizeMB` |

### Plugin Configuration Examples

```json
"ServerlessSleepless.Awaker.AccessS3.SelfAwakeService": {
    "Enabled": true,
    "BucketName": "your-bucket-name"
},
"ServerlessSleepless.Awaker.SQSLoop.SelfAwakeService": {
    "Enabled": false,
    "PublishMessageOnStartup": true,
    "MaxNumberOfMessages": 10,
    "Region": "us-west-2",
    "ServiceUrl": "http://sqs.us-west-2.amazonaws.com",
    "QueueUrl": "https://sqs.us-west-2.amazonaws.com/ACCOUNT_ID/queue-name",
    "DelayStartInSecs": 1,
    "PeriodInSecs": 60
},
"ServerlessSleepless.Awaker.BurstCPU.SelfAwakeService": {
    "Enabled": true,
    "MaxProcessCount": 1000000
},
"ServerlessSleepless.Awaker.BurstMEM.SelfAwakeService": {
    "Enabled": true,
    "MinMemSizeMB": 128,
    "MaxMemSizeMB": 512
}
```

## Create Your Own Awaker

You can easily create a custom awaker plugin:

1. **Create a new class library** targeting `net8.0;net10.0`
2. **Inherit from** `SelfAwakerServiceBase` and implement two abstract methods:
   - `InitializeIfEnabled()` — setup logic when the plugin is enabled
   - `TryAwake()` — the actual keep-warm operation
3. **Add configuration** in `appsettings.json` with the `Enabled` key to toggle it on/off
4. **Register the assembly** in `Startup.cs`:

```csharp
services.AddSingleton<ISelfAwakeService>(new SelfAwakeService(Configuration,
    typeof(Awaker.AccessS3.SelfAwakeService).Assembly,
    typeof(Awaker.SQSLoop.SelfAwakeService).Assembly,
    typeof(Awaker.BurstCPU.SelfAwakeService).Assembly,
    typeof(Awaker.BurstMEM.SelfAwakeService).Assembly,
    typeof(YourCustom.SelfAwakeService).Assembly  // Add your assembly here
));
```

## Deploy to AWS

### Using AWS SAM CLI

```bash
cd AWSServerlessSleepless.Host

# Build the Lambda package
sam build

# Deploy (guided mode for first-time setup)
sam deploy --guided
```

### Using AWS Lambda Tools

```bash
cd AWSServerlessSleepless.Host

# Install the Lambda tools
dotnet tool install -g Amazon.Lambda.Tools

# Deploy the serverless application
dotnet lambda deploy-serverless
```

### CloudFormation Template

The included `serverless.template` configures:
- **Runtime**: `provided.al2023` (self-contained .NET 10)
- **Memory**: 512 MB
- **Timeout**: 30 seconds
- **API Gateway**: Proxy integration for all routes
- **S3 Bucket**: Optional auto-creation via parameters

### EventBridge Keep-Warm Rule (Recommended)

For maximum effectiveness, add a CloudWatch EventBridge rule to invoke your Lambda every 5 minutes:

```bash
aws events put-rule \
    --name "keep-lambda-warm" \
    --schedule-expression "rate(5 minutes)"

aws lambda add-permission \
    --function-name YOUR_FUNCTION_NAME \
    --statement-id keep-warm-event \
    --action lambda:InvokeFunction \
    --principal events.amazonaws.com

aws events put-targets \
    --rule "keep-lambda-warm" \
    --targets "Id=1,Arn=YOUR_FUNCTION_ARN"
```

## Project Structure

```
ServerlessSleepless.sln
├── AWSServerlessSleepless.Host/          # Lambda WebAPI host (net10.0)
│   ├── Controllers/                       # API endpoints
│   ├── Services/                          # SelfAwakeService orchestrator
│   ├── serverless.template                # SAM/CloudFormation template
│   └── appsettings.json                   # All awaker configurations
├── ServerlessSleepless.Awaker.Abstraction/  # Interfaces (IPleaseAwakeMyselfService)
├── ServerlessSleepless.Awaker.Common/       # Base class (SelfAwakerServiceBase)
├── ServerlessSleepless.Awaker.AccessS3/     # S3 awaker plugin
├── ServerlessSleepless.Awaker.SQSLoop/      # SQS awaker plugin
├── ServerlessSleepless.Awaker.BurstCPU/     # CPU burst awaker plugin
├── ServerlessSleepless.Awaker.BurstMEM/     # Memory burst awaker plugin
└── ServerlessSleepless.Logging/             # Log4net + CloudWatch integration
```

## Contributing

Contributions are welcome! See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

Ideas for new awakers:
- **DynamoDB Awaker** — Read/write to a DynamoDB table
- **HTTP Awaker** — Ping an external health endpoint
- **CloudWatch Awaker** — Publish a custom metric

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

Copyright (c) 2019-2026, Kevin Wu
