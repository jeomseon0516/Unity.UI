# Jeomseon Unity UI

[한국어](README.md) | English

Provides UI Toolkit view creation/registration/layer stacks and optional Navigation/Transition.

## Quick start

Configure UIView UXML/layers in UICatalog, connect Catalog/Channel to UIStackManager, request open/close through Channel, and consume notifications in Navigation/Transition. Follow samples for subscription/transition lifetimes.

## Contracts and limitations

Core does not depend on concrete Views. Screen/Popup/System are independent layers; UI Toolkit picking/backdrops handle input blocking. Transition and Navigation are namespace layers in the same assembly.

## Requirements

- Unity `6000.6.0f1`
- Package: `com.jeomseon.unity.ui` `0.7.0`

## Installation

Merge this into your project's `Packages/manifest.json`; preserve existing dependencies and registries.

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

Or use **Install package from git URL** in Package Manager:

```text
https://github.com/jeomseon0516/Unity.UI.git#v0.7.0
```

## API usage and details

### Namespace composition

One package, one asmdef; the sub-layers are split by namespace only. They work independently,
and the only allowed dependency direction is `Jeomseon.Unity.UI` (Core) &larr; `.Transition` /
`.Navigation`.

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

the Unreleased section of the CHANGELOG.

The runtime assembly references only the generic Attributes declarations. UI-specific custom
editors use shared editor helpers, so they reference the Editor Toolkit's editor assembly.

## Samples

Import the desired entry from **Package Manager > Samples**. Use the included Scene/Editor tool and complete its Setup step first, if required.

- [Basic Usage](Samples~/BasicUsage/README.md)
  - [UIBasicUsageSample](Samples~/BasicUsage/UIBasicUsageSample.unity)
- [Navigation Transition Usage](Samples~/NavigationTransitionUsage/README.md)
  - [NavigationTransitionSample](Samples~/NavigationTransitionUsage/NavigationTransitionSample.unity)

## Tests

[Test sources](Tests)

Add the package ID to manifest `testables`, then execute its relevant EditMode/PlayMode assemblies in Unity Test Runner.

## Further reading

[Complete source map and package metadata](Documentation~/Overview.en.md)
- [Changelog](CHANGELOG.md)

## License

[License](LICENSE.md)
