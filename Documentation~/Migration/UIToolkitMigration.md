# UI Toolkit Migration

## ResolutionObserver 제거

전역 `ResolutionObserver`와 `ResolutionObserver.Instance`는 제거되었습니다. UI Toolkit 레이아웃은
화면 해상도뿐 아니라 실제 패널·부모 크기의 영향을 받으므로 대상 요소에서 크기 변경을 직접 받습니다.

```csharp
visualElement.RegisterCallback<GeometryChangedEvent>(evt =>
{
    Vector2 size = evt.newRect.size;
});
```

콜백을 별도 객체에 등록했다면 해당 객체의 수명 종료 시 `UnregisterCallback`으로 해제합니다.

## UIStackManager 직접 호출 제거

`UIStackManager.GetUI`/`OpenUI`/`CloseUI`/`CloseAllUI`는 제거되었습니다. 외부 코드는 Manager를
직접 참조하지 않고 `UIChannel.RequestOpen<T>()`, `RequestClose(view)`, `RequestCloseAll()`을
사용합니다. 처리 완료는 같은 채널의 `ScreenOpened(UIView)`, `ScreenClosed(UIView)`,
`AllScreensClosed()`를 구독합니다. `Type`이 아니라 실제 화면 인스턴스가 전달되므로 코드
구독자는 `view.GetType()`이나 `is` 패턴으로 필요한 타입 정보를 얻습니다. Inspector에서
연결하려면 선택적 `UIChannelListener`의 `UnityEvent<UIView>` 필드를 사용합니다(Dynamic
바인딩이라 `UIView`가 `UnityEngine.Object`가 아니어도 정상 동작합니다).

## 화면 카탈로그 타입 문자열 제거

`UIScreenEntry`의 Screen Type 문자열과 Inspector 타입 드롭다운은 제거되었습니다. `UIView` 파생
화면을 `[UxmlElement]` partial 타입으로 선언하고 UXML의 루트 요소로 사용합니다.

```csharp
[UxmlElement]
public sealed partial class InventoryView : UIView
{
}
```

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements" xmlns:game="Game.UI">
    <game:InventoryView />
</ui:UXML>
```

`UIChannel` 카탈로그에는 Layer와 해당 UXML만 등록합니다.
