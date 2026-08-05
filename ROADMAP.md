# UI 로드맵

우선순위: `P0` 결함·안전성 → `P1` 핵심 구조 → `P2` API·성능 → `P3` 장기 확장

## 작업 순서

1. **P0-01 — UIManager 수명과 씬 전환 안정화**
   - Domain Reload 비활성화, Additive Scene, 파괴된 UI와 활성 스택을 검증합니다.
2. **P0-02 — UI 키 충돌과 빈 스택 처리**
   - 타입명 기반 키 중복, 중복 Open/Close 및 등록되지 않은 UI 동작을 테스트합니다.
3. **P1-01 — UI 등록 설정 에셋**
   - Canvas, UI prefab, 정렬, 중첩·닫기 정책을 ScriptableObject로 구성할지 검토합니다.
4. **P1-02 — 비동기 UI 로딩**
   - Addressables를 강제하지 않는 loader 계약과 로딩 취소·실패 정책을 설계합니다.
5. **P2-01 — UI Toolkit 대체 범위**
   - RangeSlider와 범용 pointer trigger를 UI Toolkit 기본 기능과 비교합니다.
6. **P2-02 — Trigger 중복 축소**
   - EventTrigger 또는 공통 제네릭 기반 구현과 현재 개별 컴포넌트의 사용성을 비교합니다.
7. **P3-01 — Navigation·Transition 확장**
   - 화면 전환, history, modal, animation은 Core 관리자와 분리된 선택 계층으로 설계합니다.
