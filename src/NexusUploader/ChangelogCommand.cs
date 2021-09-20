using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NexusUploader.Services;
using Spectre.Cli;

namespace NexusUploader
{
    public class ChangelogCommand : AsyncCommand<ChangelogCommand.Settings>
    {
        private readonly ManageClient _client;
        private readonly ApiClient _api;
        private readonly ILogger<ChangelogCommand> _logger;

        public ChangelogCommand(ManageClient uploadClient, ApiClient apiClient, ILogger<ChangelogCommand> logger)
        {
            _client = uploadClient;
            _api = apiClient;
            _logger = logger;
        }
        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            var config = settings.MergedConfiguration;
            _logger.LogInformation($"Attempting to retrieve game details for '{config.Game}'");
            var gameId = await _api.GetGameId(config.Game, config.ApiKey);
            _logger.LogDebug($"Game details loaded: {config.Game}/{gameId}");
            var game = new GameRef(config.Game, gameId);
            var success = await _client.AddChangelog(game, config.ModId, settings.ModVersion, settings.ChangelogContent);
            if (success) {
                _logger.LogInformation($"[green]Success![/] Your changelog has been added for version [bold]'{settings.ModVersion}'[/]");
                return 0;
            } else {
                _logger.LogWarning("[bold orange3]Failed![/] There was an unknown error while updating the changelog!");
                _logger.LogWarning("Ensure that you have access to edit the requested mod and that it exists");
                return 1;
            }
        }

        public class Settings : AppSettings {
            private readonly ModConfiguration _config;

            public Settings(ModConfiguration config)
            {
                _config = config;
            }

            public ModConfiguration MergedConfiguration => _config;

            [CommandOption("-k|--api-key [keyValue]")]
            public FlagValue<string> ApiKey {get;set;}

            [CommandArgument(0, "<version>")]
            public string ModVersion {get;set;}

            [CommandOption("-g|--game [game]")]
            public FlagValue<string> Game {get;set;}

            [CommandOption("-m|--mod-id [modId]")]
            public FlagValue<int> ModId {get;set;}

            [CommandArgument(1, "<changelog>")]
            public string ChangelogContent {get;set;} = string.Empty;

            private bool IsSettingsValid() {
                return ChangelogContent.IsSet()
                    && ModVersion.IsSet()
                    && (ApiKey.IsSet || _config.ApiKey.IsSet()) 
                    && (Game.IsSet || _config.Game.IsSet())
                    && (ModId.IsSet || _config.ModId != default(int));
            }

            public override ValidationResult Validate()
            {
                if (!IsSettingsValid()) {
                    return ValidationResult.Error("Not all required settings provided in configuration or command line!");
                } else if (IsSettingsValid()) {
                    _config.ApiKey = ApiKey.IsSet ? ApiKey.Value : _config.ApiKey;
                    _config.ModId = ModId.IsSet ? ModId.Value : _config.ModId;
                    _config.Game = Game.IsSet ? Game.Value : _config.Game;
                    // _config.ModId = ModId;
                }
                return base.Validate();
            }

        }
    }
}