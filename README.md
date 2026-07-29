# Jeomseon Unity UI

Reusable uGUI and TextMesh Pro controls, popups, and interaction components.

## 설치

OpenUPM 등록 전에는 Package Manager의 **Add package from git URL**에서 다음 주소를 사용합니다.

```text
https://github.com/jeomseon0516/Unity.UI.git#v0.2.2
```

## 리팩토링 방침

Unity가 제공하는 동등 기능과 비교해 대체 가능한 코드는 소스의 한글 TODO 주석과 CHANGELOG의 Unreleased 항목에서 추적합니다.

런타임 어셈블리는 범용 Attribute 선언만 참조합니다. UI 전용 CustomEditor는 공통 Editor Helper를 사용하므로 EditorToolkit의 Editor 어셈블리를 참조합니다.
