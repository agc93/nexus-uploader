using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexusUploader.Nexus.Services;
using Spectre.Cli;

namespace NexusUploader.Nexus
{
    public class CheckCommand : AsyncCommand<CheckCommand.Settings>
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<CheckCommand> _logger;
        private readonly UploadClient _uploadClient;
        private readonly ModConfiguration _config;
        private readonly ManageClient _manager;

        public CheckCommand(ApiClient apiClient, ILogger<CheckCommand> logger, UploadClient uploadClient, ModConfiguration config, ManageClient manager)
        {
            _apiClient = apiClient;
            _logger = logger;
            _uploadClient = uploadClient;
            _config = config;
            _manager = manager;
        }
        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            var apiValid = !settings.ApiKey.IsSet;
            var ckValid = !settings.RawCookie;
            if (settings.ApiKey.IsSet) {
                apiValid = await _apiClient.CheckValidKey(settings.ApiKey.Value);
                if (apiValid) {
                    _logger.LogInformation("[green]API key successfully validated![/]");
                } else {
                    _logger.LogWarning("[orange3]API key validation [bold]failed![/][/]");
                }
            }
            if (settings.RawCookie) {
                ckValid = await _manager.CheckValidSession();
                if (ckValid) {
                    _logger.LogInformation("[green]Cookies successfully validated![/]");
                } else {
                    _logger.LogWarning("[orange3]Cookie validation [bold]failed![/][/]");
                }
            }
            return ckValid && apiValid ? 0 : 1;
        }

        public class Settings : AppSettings
        {
            private readonly ModConfiguration _config;

            public Settings(ModConfiguration config)
            {
                _config = config;
            }
            [CommandOption("-k|--key [value]")]
            public FlagValue<string> ApiKey { get; set; }

            [CommandOption("-c|--cookie")]
            public bool RawCookie { get; set; }
            public override ValidationResult Validate()
            {
                if (_config.Cookies.IsSet()) {
                    RawCookie = true;
                }
                if (!ApiKey.IsSet && _config.ApiKey.IsSet()) {
                    ApiKey.Value = _config.ApiKey;
                    ApiKey.IsSet = true;
                }
                if (!ApiKey.IsSet && !RawCookie)
                {
                    return ValidationResult.Error("You must specify either --key or --cookie to check API keys or cookies respectively.");
                }
                return base.Validate();
            }
        }
    }
}