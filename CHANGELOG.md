# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [2.0.0] - 2026-03-24

### Changed
- Upgraded from .NET Core 2.1 to .NET 10
- Upgraded all AWS SDK packages to latest versions
  - `Amazon.Lambda.AspNetCoreServer` 3.0.2 -> 9.0.1
  - `AWSSDK.S3` 3.5.0 -> 3.7.405.5
  - `AWSSDK.SQS` 3.3.100.60 -> 3.7.400.83
  - `AWSSDK.Extensions.NETCore.Setup` 3.3.6 -> 3.7.400
  - `AWS.Logger.Log4net` 3.0.0 -> 3.4.0
- Changed license from BSD-3-Clause to MIT
- Modernized ASP.NET Core hosting pattern (endpoint routing)
- Updated Lambda runtime from `dotnetcore2.1` to `provided.al2023` (self-contained)
- Replaced deprecated `AWSLambdaFullAccess` policy with `AWSLambdaBasicExecutionRole`
- Increased Lambda memory from 256 MB to 512 MB for self-contained deployment

### Fixed
- Typo: `CoommonLogInfo` -> `CommonLogInfo` in `SelfAwakerServiceBase`

### Added
- GitHub Actions CI pipeline
- Issue templates (bug report, feature request)
- Contributing guide
- This changelog
- README badges and architecture diagram
- AWS deployment documentation (SAM CLI, Lambda Tools, EventBridge)
- Nullable reference types enabled across all projects

## [1.0.0] - 2019

### Added
- Initial release with 4 Awaker plugins (S3, SQS, BurstCPU, BurstMEM)
- Timer-based periodic awakening service
- ASP.NET Core 2.1 Lambda WebAPI
- Reflection-based plugin discovery
- S3 Proxy controller
- Log4net CloudWatch integration
