namespace Template.WindowsApp.Extender;

using Microsoft.Extensions.DependencyInjection;

using Smart.Mvvm.Resolver;
using Smart.Navigation.Plugins;

public sealed class LeakDetectionPlugin : PluginBase
{
    private static readonly TimeSpan CheckDelay = TimeSpan.FromSeconds(5);

    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(1);

    private ILogger Logger => field ??= ResolveProvider.Default.GetRequiredService<ILogger<LeakDetectionPlugin>>();

    public override void OnClose(IPluginContext pluginContext, object view, object? target)
    {
        var references = new List<KeyValuePair<string, WeakReference>>(2)
        {
            new(view.GetType().Name, new WeakReference(view))
        };
        if (target is not null)
        {
            references.Add(new(target.GetType().Name, new WeakReference(target)));
        }

        _ = CheckAsync(references);
    }

    private async Task CheckAsync(List<KeyValuePair<string, WeakReference>> references)
    {
        await Task.Delay(CheckDelay);
        Collect();

        if (references.Exists(static x => x.Value.IsAlive))
        {
            await Task.Delay(RetryDelay);
            Collect();
        }

        foreach (var (name, reference) in references)
        {
            if (reference.IsAlive)
            {
                Logger.WarnLeakSuspected(name);
            }
            else
            {
                Logger.DebugClosedObjectCollected(name);
            }
        }
    }

    private static void Collect()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}
