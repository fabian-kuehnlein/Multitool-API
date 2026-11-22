using Mapster;
using Multitool.Application.Mappings;

namespace Multitool.Tests.Shared;

/// <summary>
/// Applies the application's Mapster configuration exactly once for the whole test process.
/// Service test constructors used to call GlobalSettings.Apply directly, which races when xUnit
/// creates test classes in parallel ("TypeAdapter.Adapt was already called").
/// </summary>
public static class MappingConfigBootstrapper
{
    private static readonly object SyncRoot = new();
    private static bool _applied;

    public static void EnsureApplied()
    {
        if (_applied)
        {
            return;
        }

        lock (SyncRoot)
        {
            if (_applied)
            {
                return;
            }

            TypeAdapterConfig.GlobalSettings.Apply(new MappingConfig());
            _applied = true;
        }
    }
}
