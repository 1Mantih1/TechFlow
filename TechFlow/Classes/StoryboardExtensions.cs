using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

public static class StoryboardExtensions
{
    public static Task BeginAsync(this Storyboard storyboard, FrameworkElement target)
    {
        var tcs = new TaskCompletionSource<bool>();
        EventHandler handler = null;

        handler = (s, e) =>
        {
            storyboard.Completed -= handler;
            tcs.TrySetResult(true);
        };

        storyboard.Completed += handler;
        storyboard.Begin(target);

        return tcs.Task;
    }
}
