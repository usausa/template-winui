namespace Template.WindowsApp.Extender;

using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

public sealed class EntranceEffect : IWinUINavigationEffect
{
    public const string Name = "Entrance";

    private const double Offset = 24;

    private static readonly Duration AnimationDuration = new(TimeSpan.FromMilliseconds(250));

    public Task EffectAsync(WinUINavigationEffectContext context)
    {
        if (context.Phase is not (WinUINavigationEffectPhase.Open or WinUINavigationEffectPhase.Activate))
        {
            return Task.CompletedTask;
        }

        var view = context.View;
        var transform = new TranslateTransform { Y = Offset };
        view.RenderTransform = transform;
        view.Opacity = 0;

        var slide = new DoubleAnimation { From = Offset, To = 0, Duration = AnimationDuration, EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
        Storyboard.SetTarget(slide, transform);
        Storyboard.SetTargetProperty(slide, nameof(TranslateTransform.Y));
        var fade = new DoubleAnimation { From = 0, To = 1, Duration = AnimationDuration, EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
        Storyboard.SetTarget(fade, view);
        Storyboard.SetTargetProperty(fade, nameof(UIElement.Opacity));

        var storyboard = new Storyboard();
        storyboard.Children.Add(slide);
        storyboard.Children.Add(fade);

        var completion = new TaskCompletionSource();
        storyboard.Completed += (_, _) =>
        {
            storyboard.Stop();
            view.Opacity = 1;
            view.RenderTransform = null;
            completion.TrySetResult();
        };
        storyboard.Begin();
        return completion.Task;
    }
}
