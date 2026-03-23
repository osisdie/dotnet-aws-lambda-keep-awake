# ServerlessSleepless.Host

The main Lambda WebAPI host project. See the [root README](../README.md) for full documentation.

## Key Files

- **LambdaEntryPoint.cs** - Lambda function entry point (extends `APIGatewayProxyFunction`)
- **LocalEntryPoint.cs** - Local development entry point (Kestrel on port 5001)
- **Startup.cs** - ASP.NET Core service configuration and awaker registration
- **serverless.template** - AWS SAM/CloudFormation template
- **aws-lambda-tools-defaults.json** - AWS Lambda deployment defaults
- **appsettings.json** - All awaker plugin configurations

## Quick Deploy

```bash
dotnet tool install -g Amazon.Lambda.Tools
dotnet lambda deploy-serverless
```

## Switching to Application Load Balancer

Change the base class of `LambdaEntryPoint` from `APIGatewayProxyFunction` to `ApplicationLoadBalancerFunction`.
