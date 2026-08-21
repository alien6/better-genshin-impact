#!/usr/bin/env python3
from __future__ import annotations

import base64
import gzip
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
PT_PATH = ROOT / "BetterGenshinImpact/User/I18n/pt-BR.json"
DELTA_PATH = ROOT / ".github/ptbr-extra.json.gz.b64"
SELF_PATH = Path(__file__).resolve()

def read(path: Path) -> str:
    return path.read_text(encoding="utf-8-sig")

def write(path: Path, text: str) -> None:
    path.write_text(text, encoding="utf-8", newline="\n")

def replace_once(path: Path, old: str, new: str, label: str) -> bool:
    text = read(path)
    if new in text:
        print(f"[skip] {label}: already applied")
        return False
    if old not in text:
        print(f"[warn] {label}: source pattern not found")
        return False
    write(path, text.replace(old, new, 1))
    print(f"[ok] {label}")
    return True

def merge_catalog() -> None:
    current = json.loads(read(PT_PATH))
    compressed = base64.b64decode(DELTA_PATH.read_text(encoding="ascii").strip())
    extra = json.loads(gzip.decompress(compressed).decode("utf-8"))
    before = len(current)
    current.update(extra)
    write(PT_PATH, json.dumps(current, ensure_ascii=False, indent=2) + "\n")
    print(f"[ok] pt-BR catalog: {before} -> {len(current)} keys (+{len(extra)})")

def patch_logger() -> None:
    path = ROOT / "BetterGenshinImpact/App.xaml.cs"
    old = '''                services.AddLogging(c => c.AddSerilog());
                // if ("zh-Hans".Equals(all.OtherConfig.UiCultureInfoName, StringComparison.OrdinalIgnoreCase))
                // {
                //     services.AddLogging(c => c.AddSerilog());
                // }
                // else
                // {
                //     services.AddLogging(logging =>
                //     {
                //         logging.ClearProviders();
                //         logging.SetMinimumLevel(LogLevel.Debug);
                //         logging.AddFilter("Microsoft", LogLevel.Warning);
                //         logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning);
                //         logging.Services.AddSingleton<ILoggerProvider, TranslatingSerilogLoggerProvider>();
                //     });
                // }
'''
    new = '''                if ("zh-Hans".Equals(all.OtherConfig.UiCultureInfoName, StringComparison.OrdinalIgnoreCase))
                {
                    services.AddLogging(c => c.AddSerilog());
                }
                else
                {
                    services.AddLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.SetMinimumLevel(LogLevel.Debug);
                        logging.AddFilter("Microsoft", LogLevel.Warning);
                        logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning);
                        logging.Services.AddSingleton<ILoggerProvider, TranslatingSerilogLoggerProvider>();
                    });
                }
'''
    replace_once(path, old, new, "translate non-Chinese log messages")

def patch_combo_translation() -> None:
    path = ROOT / "BetterGenshinImpact/View/Behavior/AutoTranslateInterceptor.cs"
    text = read(path)

    field_old = '''            private readonly HashSet<ContextMenu> _trackedContextMenus = new();
            private readonly HashSet<ToolTip> _trackedToolTips = new();
            private readonly HashSet<DependencyObject> _pendingApply = new();
'''
    field_new = '''            private readonly HashSet<ContextMenu> _trackedContextMenus = new();
            private readonly HashSet<ToolTip> _trackedToolTips = new();
            private readonly HashSet<ComboBox> _trackedComboBoxes = new();
            private readonly HashSet<DependencyObject> _pendingApply = new();
'''
    if field_old in text:
        text = text.replace(field_old, field_new, 1)
        print("[ok] track ComboBox popups")
    elif field_new not in text:
        print("[warn] ComboBox tracking field pattern not found")

    request_old = '''                if (IsInComboBoxContext(obj))
                {
                    return;
                }
'''
    request_new = '''                if (IsInComboBoxContext(obj) && obj is not ComboBox && obj is not ComboBoxItem)
                {
                    return;
                }
'''
    if request_old in text:
        text = text.replace(request_old, request_new, 1)
        print("[ok] allow ComboBox/ComboBoxItem translation requests")
    elif request_new not in text:
        print("[warn] ComboBox RequestApply pattern not found")

    apply_old = '''                    if (IsInComboBoxContext(current))
                    {
                        continue;
                    }

                    TranslateKnown(current, translator);
'''
    apply_new = '''                    if (current is ComboBox comboBox)
                    {
                        TranslateToolTip(comboBox, translator);
                        TrackComboBox(comboBox);
                        for (var i = 0; i < comboBox.Items.Count; i++)
                        {
                            if (comboBox.ItemContainerGenerator.ContainerFromIndex(i) is DependencyObject container)
                            {
                                queue.Enqueue(container);
                            }
                            else if (comboBox.Items[i] is DependencyObject dependencyItem)
                            {
                                queue.Enqueue(dependencyItem);
                            }
                        }
                        continue;
                    }

                    if (current is ComboBoxItem comboBoxItem)
                    {
                        if (comboBoxItem.Content is string comboText)
                        {
                            TranslateIfNotBound(
                                comboBoxItem,
                                ContentControl.ContentProperty,
                                comboText,
                                s => comboBoxItem.Content = s,
                                translator);
                        }
                        TranslateToolTip(comboBoxItem, translator);
                        continue;
                    }

                    if (IsInComboBoxContext(current))
                    {
                        continue;
                    }

                    TranslateKnown(current, translator);
'''
    if apply_old in text:
        text = text.replace(apply_old, apply_new, 1)
        print("[ok] translate ComboBox tooltips/items")
    elif apply_new not in text:
        print("[warn] ComboBox Apply pattern not found")

    marker = '''            private static IEnumerable<DependencyObject> EnumerateInlineObjects(FrameworkElement fe)
'''
    method = '''            private void TrackComboBox(ComboBox comboBox)
            {
                if (!_trackedComboBoxes.Add(comboBox))
                {
                    return;
                }

                EventHandler? openedHandler = null;
                openedHandler = (_, _) =>
                {
                    _root.Dispatcher.BeginInvoke(
                        () => Apply(comboBox),
                        DispatcherPriority.Loaded);
                };
                comboBox.DropDownOpened += openedHandler;
                _unsubscribe.Add(() => comboBox.DropDownOpened -= openedHandler);
            }

'''
    if method not in text:
        if marker in text:
            text = text.replace(marker, method + marker, 1)
            print("[ok] refresh translated ComboBox items when dropdown opens")
        else:
            print("[warn] ComboBox method insertion marker not found")

    write(path, text)

def ensure_scope(path: Path) -> bool:
    text = read(path)
    first_gt = text.find(">")
    if first_gt < 0:
        return False
    head = text[:first_gt]
    if "AutoTranslateInterceptor.EnableAutoTranslate=\"True\"" in head:
        return False

    if 'xmlns:behavior="clr-namespace:BetterGenshinImpact.View.Behavior"' not in head:
        m = re.search(r'(\n\s+xmlns:x="http://schemas\.microsoft\.com/winfx/2006/xaml")', head)
        namespace_line = '\n        xmlns:behavior="clr-namespace:BetterGenshinImpact.View.Behavior"'
        if m:
            head = head[:m.end()] + namespace_line + head[m.end():]
        else:
            class_match = re.search(r'(<[\w:]+\s+x:Class="[^"]+")', head)
            if not class_match:
                return False
            head = head[:class_match.end()] + namespace_line + head[class_match.end():]

    head += '\n        behavior:AutoTranslateInterceptor.EnableAutoTranslate="True"'
    text = head + text[first_gt:]
    write(path, text)
    return True

def patch_scopes() -> None:
    explicit = {
        "BetterGenshinImpact/View/Pages/HotkeyPage.xaml",
        "BetterGenshinImpact/View/Pages/KeyBindingsSettingsPage.xaml",
        "BetterGenshinImpact/View/Pages/MacroSettingsPage.xaml",
        "BetterGenshinImpact/View/Pages/OneDragonFlowPage.xaml",
        "BetterGenshinImpact/View/Pages/TaskSettingsPage.xaml",
        "BetterGenshinImpact/View/Pages/TriggerSettingsPage.xaml",
        "BetterGenshinImpact/View/Pages/View/HardwareAccelerationView.xaml",
        "BetterGenshinImpact/View/Pages/View/ScriptGroupConfigView.xaml",
    }

    changed = []
    for path in sorted((ROOT / "BetterGenshinImpact/View").rglob("*.xaml")):
        raw = read(path)
        m = re.match(r"\s*<([\w:]+)\b", raw)
        if not m:
            continue
        root_local = m.group(1).split(":")[-1]
        rel = path.relative_to(ROOT).as_posix()
        if root_local in {"Window", "FluentWindow"} or rel in explicit:
            if ensure_scope(path):
                changed.append(rel)
    print(f"[ok] explicit translation scope added to {len(changed)} XAML roots")

def patch_known_bound_text() -> None:
    replacements = [
        (
            ROOT / "BetterGenshinImpact/View/Pages/HotkeyPage.xaml",
            'Content="{Binding FunctionName}"',
            'Content="{Binding FunctionName, Converter={StaticResource TrConverter}}"',
            "hotkey function names",
        ),
        (
            ROOT / "BetterGenshinImpact/View/Pages/HotkeyPage.xaml",
            'Content="{Binding HotKeyTypeName, Mode=OneWay}"',
            'Content="{Binding HotKeyTypeName, Converter={StaticResource TrConverter}, Mode=OneWay}"',
            "hotkey type names",
        ),
        (
            ROOT / "BetterGenshinImpact/View/Pages/KeyBindingsSettingsPage.xaml",
            'Content="{Binding ActionName}"',
            'Content="{Binding ActionName, Converter={StaticResource TrConverter}}"',
            "key-binding action names",
        ),
        (
            ROOT / "BetterGenshinImpact/View/Pages/OneDragonFlowPage.xaml",
            'Text="{Binding Name}" Width="100"',
            'Text="{Binding Name, Converter={StaticResource TrConverter}}" Width="100"',
            "one-dragon task names",
        ),
        (
            ROOT / "BetterGenshinImpact/View/Controls/DomainSelector.xaml",
            'Text="{Binding}" Margin="8,4"',
            'Text="{Binding Converter={StaticResource TrConverter}}" Margin="8,4"',
            "domain selector string items",
        ),
    ]
    for path, old, new, label in replacements:
        if not path.exists():
            print(f"[warn] {label}: file missing")
            continue
        replace_once(path, old, new, label)

def patch_value_display_combos() -> None:
    pattern_self = re.compile(
        r'<ComboBox(?P<attrs>[^<>]*?)\s+DisplayMemberPath="Value"(?P<rest>[^<>]*?)/>',
        re.DOTALL,
    )
    changed_files = 0
    for path in sorted((ROOT / "BetterGenshinImpact/View").rglob("*.xaml")):
        text = read(path)
        original = text

        def repl_self(m: re.Match[str]) -> str:
            start = '<ComboBox' + m.group('attrs') + m.group('rest')
            line_start = text.rfind("\n", 0, m.start()) + 1
            indent = text[line_start:m.start()]
            return (
                start + '>\n'
                f'{indent}    <ComboBox.ItemTemplate>\n'
                f'{indent}        <DataTemplate>\n'
                f'{indent}            <TextBlock Text="{{Binding Value, Converter={{StaticResource TrConverter}}}}" />\n'
                f'{indent}        </DataTemplate>\n'
                f'{indent}    </ComboBox.ItemTemplate>\n'
                f'{indent}</ComboBox>'
            )

        text = pattern_self.sub(repl_self, text)
        if text != original:
            write(path, text)
            changed_files += 1
    print(f"[ok] translated DisplayMemberPath=Value combo displays in {changed_files} files")

def cleanup() -> None:
    for path in (DELTA_PATH, SELF_PATH):
        try:
            path.unlink()
            print(f"[ok] removed temporary {path.relative_to(ROOT)}")
        except FileNotFoundError:
            pass

def main() -> None:
    merge_catalog()
    patch_logger()
    patch_combo_translation()
    patch_scopes()
    patch_known_bound_text()
    patch_value_display_combos()
    cleanup()

if __name__ == "__main__":
    main()
