# Hero Limit Break Portfolio Sample

C# / .NET 8로 작성한 게임 서버 기능 예제입니다.

## 포함 내용

- 한계 돌파 가능 여부 및 스펙 검증
- 동일 아이템이 여러 번 정의된 경우 필요 수량 합산
- 재료 사전 검증 후 소모하여 부족한 경우 상태가 변경되지 않도록 처리
- 한계 돌파 단계와 등급 변경
- `SyncHeroCombatPower`에서 현재 영웅 상태를 기준으로 전투력 재계산
- `HeroBattlePowerLogic`에서 전투력 계산 로직 관리
- xUnit 기반 기능 테스트

## 전투력 계산

`HeroLimitBreakService`는 한계 돌파 적용 후 `SyncHeroCombatPower`를 호출합니다. 해당 메서드는 기존 전투력에 보너스를 더하는 대신 `HeroBattlePowerLogic.Calculate`를 호출해 현재 상태로부터 전투력을 다시 계산합니다.

예제 계산에는 영웅 기본 능력치, 레벨, 누적 한계 돌파 보너스, 장비, 스킬, 속성 보너스, 펫 레벨이 반영됩니다. 계산식과 데이터는 포트폴리오 설명을 위한 가상의 예시이며 실제 게임의 산식이나 데이터를 반영하지 않습니다.

실제 서비스에서는 현재 프로젝트의 전투력 계산 로직과 데이터 조회 방식을 사용하고, 재료 소모와 영웅 상태 변경은 DB 트랜잭션 등 일관성 보장 경계 안에서 처리해야 합니다.

## 실행

```bash
dotnet test HeroLimitBreak.sln
```
