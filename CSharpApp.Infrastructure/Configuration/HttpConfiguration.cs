using CSharpApp.Application.Auth;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

namespace CSharpApp.Infrastructure.Configuration;

public static class HttpConfiguration
{
    public static IServiceCollection AddHttpConfiguration(this IServiceCollection services)
    {
        services.AddSingleton<IAuthTokenProvider, AuthTokenProvider>();
        services.AddTransient<AuthenticationDelegatingHandler>();

        // Dedicated client for the login call; must NOT use the auth handler (circular dependency)
        services.AddHttpClient(AuthTokenProvider.AuthClientName, (serviceProvider, client) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<RestApiSettings>>().Value;
            client.BaseAddress = new Uri(settings.BaseUrl!);
        }).AddResiliencePolicies();

        return services;
    }

    public static IHttpClientBuilder AddResiliencePolicies(this IHttpClientBuilder builder)
    {
        // Handler lifetime (minutes) from HttpClientSettings, bound per named client
        builder.Services.AddTransient<IConfigureOptions<HttpClientFactoryOptions>>(serviceProvider =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<HttpClientSettings>>().Value;
            return new ConfigureNamedOptions<HttpClientFactoryOptions>(
                builder.Name,
                options => options.HandlerLifetime = TimeSpan.FromMinutes(settings.LifeTime));
        });

        // Retry on 5xx, 408 and HttpRequestException with exponential backoff
        return builder.AddPolicyHandler((serviceProvider, _) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<HttpClientSettings>>().Value;
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    settings.RetryCount,
                    attempt => TimeSpan.FromMilliseconds(settings.SleepDuration * Math.Pow(2, attempt - 1)));
        });
    }
}