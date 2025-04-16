using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CashFlow.Infrastructure.Settings;

public sealed class AppSettings : IAppSettings, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AppSettings> _logger;
    private IDisposable _changeToken;

    public RabbitMQ RabbitMQSettings { get; private set; }
    public MySqlDB DatabaseSettings { get; private set; }
    public Redis CacheSettings { get; private set; }
    public MongoDB MongoSettings { get; private set; }

    public AppSettings(
        IConfiguration configuration,
        ILogger<AppSettings> logger)
    {
        _configuration = configuration;
        _logger = logger;
        LoadAllSettings();
        SetupChangeTracking();
    }

    private void LoadAllSettings()
    {
        try
        {
            RabbitMQSettings = _configuration.GetSection(RabbitMQ.SectionName).Get<RabbitMQ>();
            DatabaseSettings = _configuration.GetSection(MySqlDB.SectionName).Get<MySqlDB>();
            MongoSettings = _configuration.GetSection(MongoDB.SectionName).Get<MongoDB>();
            CacheSettings = _configuration.GetSection(Redis.SectionName).Get<Redis>();
            
            _logger.LogInformation("Todas as configurações foram carregadas");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Falha ao carregar configurações");
            throw;
        }
    }

    private void SetupChangeTracking()
    {
        _changeToken = _configuration.GetReloadToken().RegisterChangeCallback(_ =>
        {
            _logger.LogInformation("Configurações alteradas - recarregando...");
            LoadAllSettings();
        }, null);
    }

    public Task ReloadAsync()
    {
        LoadAllSettings();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _changeToken?.Dispose();
    }
}