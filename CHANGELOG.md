# 변경 기록

## [0.2.1] - 2026-07-29

- Jeomseon Unity Attributes 0.2.0과 EditorToolkit 0.2.1을 사용하도록 의존성을 갱신했습니다.

## [0.2.0] - 2026-07-29

- 런타임 어셈블리의 EditorToolkit 참조를 제거하고 경량 Attributes 패키지를 참조하도록 변경했습니다.
- UI CustomEditor의 공통 Editor Helper 사용을 위해 Editor 어셈블리 참조는 유지했습니다.

## [Unreleased]

- TODO(api): 범용 컨트롤은 UI Toolkit의 Slider·ListView·Pointer 이벤트로 대체 가능한지 확인하고 런타임 uGUI가 필요한 부분만 유지합니다.
- 정적 이벤트와 전역 인스턴스의 Domain Reload 비활성화 호환성을 검토합니다.

## [0.1.0] - 2026-07-29

- JeomseonScriptPack의 관련 모듈을 독립 UPM 패키지로 분리했습니다.
