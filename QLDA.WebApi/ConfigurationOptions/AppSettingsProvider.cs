using Microsoft.Extensions.Options;
using QLDA.Application.Providers;

namespace QLDA.WebApi.ConfigurationOptions;

public class AppSettingsProvider : IAppSettingsProvider {
    private readonly AppSettings _settings;

    public AppSettingsProvider(IOptions<AppSettings> settings) {
        _settings = settings.Value;
    }

    public long PhongKeToanID => _settings.PhongKeToanID;
    public long PhongHCTHID => _settings.PhongHCTHID;
    public long PhongKHTCID => _settings.PhongKHTCID;
}