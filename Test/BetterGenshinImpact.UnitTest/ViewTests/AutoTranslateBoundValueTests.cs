using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using BetterGenshinImpact.Service.Interface;
using BetterGenshinImpact.View.Behavior;

namespace BetterGenshinImpact.UnitTest.ViewTests;

public sealed class AutoTranslateBoundValueTests
{
    [Fact]
    public void TranslateBoundCurrentValue_PreservesBindingAndChangesDisplayedValue()
    {
        RunSta(() =>
        {
            var source = new SourceModel { Text = "全部格式" };
            var textBlock = new TextBlock();
            BindingOperations.SetBinding(
                textBlock,
                TextBlock.TextProperty,
                new Binding(nameof(SourceModel.Text)) { Source = source });

            Assert.Equal("全部格式", textBlock.Text);
            Assert.NotNull(BindingOperations.GetBindingExpression(textBlock, TextBlock.TextProperty));

            var method = typeof(AutoTranslateInterceptor).GetMethod(
                "TranslateBoundCurrentValue",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(method);

            var translator = new FakeTranslationService(new Dictionary<string, string>
            {
                ["全部格式"] = "Todos os formatos"
            });

            var result = method!.Invoke(
                null,
                [
                    textBlock,
                    TextBlock.TextProperty,
                    translator,
                    TranslationSourceInfo.From(MissingTextSource.UiDynamicBinding)
                ]);

            Assert.Equal(true, result);
            Assert.Equal("Todos os formatos", textBlock.Text);
            Assert.NotNull(BindingOperations.GetBindingExpression(textBlock, TextBlock.TextProperty));
        });
    }

    [Fact]
    public void TranslateBoundCurrentValue_LeavesUntranslatedBindingUntouched()
    {
        RunSta(() =>
        {
            var source = new SourceModel { Text = "123 FPS" };
            var textBlock = new TextBlock();
            BindingOperations.SetBinding(
                textBlock,
                TextBlock.TextProperty,
                new Binding(nameof(SourceModel.Text)) { Source = source });

            var method = typeof(AutoTranslateInterceptor).GetMethod(
                "TranslateBoundCurrentValue",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(method);

            var translator = new FakeTranslationService(new Dictionary<string, string>());
            var result = method!.Invoke(
                null,
                [
                    textBlock,
                    TextBlock.TextProperty,
                    translator,
                    TranslationSourceInfo.From(MissingTextSource.UiDynamicBinding)
                ]);

            Assert.Equal(false, result);
            Assert.Equal("123 FPS", textBlock.Text);
            Assert.NotNull(BindingOperations.GetBindingExpression(textBlock, TextBlock.TextProperty));
        });
    }

    [Fact]
    public void TranslateBoundCurrentValue_DoesNotTouchTwoWayEditableBinding()
    {
        RunSta(() =>
        {
            var source = new SourceModel { Text = "全部格式" };
            var textBox = new TextBox();
            BindingOperations.SetBinding(
                textBox,
                TextBox.TextProperty,
                new Binding(nameof(SourceModel.Text))
                {
                    Source = source,
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

            var method = typeof(AutoTranslateInterceptor).GetMethod(
                "TranslateBoundCurrentValue",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(method);

            var translator = new FakeTranslationService(new Dictionary<string, string>
            {
                ["全部格式"] = "Todos os formatos"
            });

            var result = method!.Invoke(
                null,
                [
                    textBox,
                    TextBox.TextProperty,
                    translator,
                    TranslationSourceInfo.From(MissingTextSource.UiDynamicBinding)
                ]);

            Assert.Equal(false, result);
            Assert.Equal("全部格式", textBox.Text);
            Assert.Equal("全部格式", source.Text);
            Assert.NotNull(BindingOperations.GetBindingExpression(textBox, TextBox.TextProperty));
        });
    }

    [Fact]
    public void LoadedProgrammaticWindow_EnablesTranslationScopeWhenNotExplicitlyConfigured()
    {
        RunSta(() =>
        {
            var window = new Window();
            Assert.Same(
                DependencyProperty.UnsetValue,
                window.ReadLocalValue(AutoTranslateInterceptor.EnableAutoTranslateProperty));

            InvokeLoadedInterceptor(window);

            Assert.True(AutoTranslateInterceptor.GetEnableAutoTranslate(window));
            Assert.Equal(
                true,
                window.ReadLocalValue(AutoTranslateInterceptor.EnableAutoTranslateProperty));
        });
    }

    [Fact]
    public void LoadedProgrammaticWindow_PreservesExplicitDisabledTranslationScope()
    {
        RunSta(() =>
        {
            var window = new Window();
            AutoTranslateInterceptor.SetEnableAutoTranslate(window, false);

            InvokeLoadedInterceptor(window);

            Assert.False(AutoTranslateInterceptor.GetEnableAutoTranslate(window));
            Assert.Equal(
                false,
                window.ReadLocalValue(AutoTranslateInterceptor.EnableAutoTranslateProperty));
        });
    }

    private static void InvokeLoadedInterceptor(Window window)
    {
        var method = typeof(AutoTranslateInterceptor).GetMethod(
            "OnAnyElementLoaded",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        method!.Invoke(
            null,
            [window, new RoutedEventArgs(FrameworkElement.LoadedEvent, window)]);
    }

    private static void RunSta(Action action)
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (exception != null)
        {
            ExceptionDispatchInfo.Capture(exception).Throw();
        }
    }

    private sealed class SourceModel : INotifyPropertyChanged
    {
        private string _text = string.Empty;
        public string Text
        {
            get => _text;
            set
            {
                if (_text == value)
                {
                    return;
                }

                _text = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    private sealed class FakeTranslationService(IReadOnlyDictionary<string, string> map) : ITranslationService
    {
        public string Translate(string text) => Translate(text, TranslationSourceInfo.From(MissingTextSource.Unknown));

        public string Translate(string text, TranslationSourceInfo sourceInfo)
            => map.TryGetValue(text, out var translated) ? translated : text;

        public CultureInfo GetCurrentCulture() => CultureInfo.GetCultureInfo("pt-BR");

        public void Reload()
        {
        }
    }
}
