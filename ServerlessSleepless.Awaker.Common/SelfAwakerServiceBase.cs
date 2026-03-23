using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServerlessSleepless.Awaker.Abstraction;
using ServerlessSleepless.Awaker.Common.Logging;
using System;
using System.Threading.Tasks;

namespace ServerlessSleepless.Awaker.Common
{
    public abstract class SelfAwakerServiceBase : IPleaseAwakeMyselfService
    {
        public SelfAwakerServiceBase(IConfiguration configuration, ISerializer serializer)
        {
            Logger = CustomLogFactory.CreateLogger(this.GetType());
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));

            Enabled = configuration.GetValue<bool>($"{this.GetType().FullName}:Enabled", false);
            if (Enabled)
            {
                InitializeIfEnabled();
            }
        }

        public abstract void InitializeIfEnabled();
        public abstract Task TryAwake(params object[] args);

        public virtual async Task Awake(params object[] args)
        {
            if (!Enabled) return;

            ++ExecutedCount;

            try
            {
                await TryAwake(args);

                Logger.LogInformation(Serializer.Serialize(new
                {
                    provider = this.GetType().FullName,
                    common = CommonLogInfo,
                    _ts = DateTime.UtcNow
                }));
            }
            catch (Exception ex)
            {
                ++FailedCount;
                LastException = ex;

                Logger.LogWarning(Serializer.Serialize(new
                {
                    provider = this.GetType().FullName,
                    message = ex.Message,
                    exception = ex.ToString(),
                    common = CommonLogInfo,
                    _ts = DateTime.UtcNow,
                }));
            }
        }

        public object CommonLogInfo
        {
            get
            {
                return new
                {
                    @event = "awake",
                    executedCount = ExecutedCount,
                    failedCount = FailedCount,
                    echoCount = EchoCount,
                    generatedId = GeneratedId,
                    generatedTs = GeneratedTs,
                };
            }
        }

        public static object Echo()
        {
            var status = new
            {
                @event = "echo",
                _ts = DateTime.UtcNow,
                echoCount = EchoCount,
                generatedId = GeneratedId,
                generatedTs = GeneratedTs,
            };

            return status;
        }


        public static Guid GeneratedId = Guid.NewGuid();
        public static DateTime GeneratedTs = DateTime.UtcNow;
        public static int EchoCount { get; private set; } = 0;

        public bool Enabled { get; private set; } = false;

        public int ExecutedCount { get; private set; } = 0;
        public int FailedCount { get; private set; } = 0;
        public ILogger Logger { get; private set; }
        public IConfiguration Configuration { get; private set; }
        public ISerializer Serializer { get; private set; }
        public Exception LastException { get; private set; }
    }
}
