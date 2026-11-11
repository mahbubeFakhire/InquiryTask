using Extensions;
using Microsoft.Extensions.Configuration;
using System;

namespace WebApi.Server.SqlServerConfigProviders;

public class EntityConfigurationProvider : ConfigurationProvider, IDisposable
{

    public EntityConfigurationSource Source { get; }
    public EntityConfigurationProvider(EntityConfigurationSource source)
    {
        Source = source;
        if (Source.ReloadOnChange)
        {
        }
    }

    private void Load(bool reload)
    {
        try
        {
            OnReload();
        }
        catch (Exception)
        {
            if (Source.Optional.Not())
            {
                throw;
            }
        }

    }

    public override void Load()
    {
        Load(reload: false);
    }

    /// <inheritdoc />
    public void Dispose() => Dispose(true);

    /// <summary>
    /// Dispose the provider.
    /// </summary>
    /// <param name="disposing"><c>true</c> if invoked from <see cref="IDisposable.Dispose"/>.</param>
    protected virtual void Dispose(bool disposing)
    {
    }
    public override string ToString()
            => $"{GetType().Name} for 'Setting' ({(Source.Optional ? "Optional" : "Required")})";
}

public class EntityConfigurationSource : IConfigurationSource
{
    public bool Optional { get; }
    public bool ReloadOnChange { get; }
    public IConfigurationRoot Root { get; }
    public string SubstitutableStartsWith { get; }
    public string SubstitutableEndsWith { get; }

    public EntityConfigurationSource(IConfigurationRoot root, string substitutableStartsWith, string substitutableEndsWith, bool optional, bool reloadOnChange)
    {
        Root = root;
        SubstitutableStartsWith = substitutableStartsWith;
        SubstitutableEndsWith = substitutableEndsWith;
        Optional = optional;
        ReloadOnChange = reloadOnChange;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new EntityConfigurationProvider(this);
}

public static partial class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddEntityConfiguration(
        this IConfigurationBuilder builder, string substitutableStartsWith = "%", string substitutableEndsWith = "%", bool optional = true, bool reloadOnChange = true)
    {
        var root = builder.Build();

        return builder.Add(new EntityConfigurationSource(root, substitutableStartsWith, substitutableEndsWith, optional, reloadOnChange));
    }


}

