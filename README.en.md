# Jeomseon Unity UI

A UI Toolkit (`UIDocument`/`VisualElement`) screen-stack manager and reusable controls.

## Install via OpenUPM

Register the OpenUPM scoped registry once in your project's `Packages/manifest.json`.

```json
{
  "scopedRegistries": [
    {
      "name": "OpenUPM",
      "url": "https://package.openupm.com",
      "scopes": [
        "com.jeomseon"
      ]
    }
  ],
  "dependencies": {
    "com.jeomseon.unity.ui": "0.7.0"
  }
}
```

## Install via Git URL

Enter the following URL in Unity Package Manager's `Install package from git URL`.

```text
https://github.com/jeomseon0516/Unity.UI.git#v0.7.0
```

## Namespace composition

One package, one asmdef; the sub-layers are split by namespace only. They work independently,
and the only allowed dependency direction is `Jeomseon.Unity.UI` (Core) &larr; `.Transition` /
`.Navigation` (harness `ADR-0009`).

| Namespace | Role |
| --- | --- |
| `Jeomseon.Unity.UI` | Screen-stack core &mdash; `UIStackManager` / `UIStackController` / `UIView` / `UILayer` |
| `Jeomseon.Unity.UI.Channels` | `UIChannel` (requests/notifications) / `UICatalog` (screen list) / `IUIRequester` |
| `Jeomseon.Unity.UI.Components` | Reusable `VisualElement` controls &mdash; `UIScrollView` / `UICarousel` / `UIGrid` |
| `Jeomseon.Unity.UI.Transition` | Screen enter/exit effects &mdash; `ITransition`, `Fade`/`Slide`/`Scale`, `ScreenTransitions` |
| `Jeomseon.Unity.UI.Navigation` | Back-navigation history &mdash; `NavigationStack`, `BackNavigationBinder` |

`.Transition`/`.Navigation` do not enter your code unless you `using` them. When both are used
together, wiring the exit effect is a single consumer line:
`new NavigationStack(channel, v => screenTransitions.Default.PlayExit(v, TransitionContext.Exit()))`.

## Refactoring policy

Code with a Unity-provided equivalent is tracked through Korean TODO comments in the source and
the Unreleased section of the CHANGELOG.

The runtime assembly references only the generic Attributes declarations. UI-specific custom
editors use shared editor helpers, so they reference the Editor Toolkit's editor assembly.
