# 🔥 Mindspace Campfire

**MARS 팀 Mindspace (2025 동양미래 EXPO 출품작)의 Unity 3D 캠프파이어 불멍 게임**

모닥불 앞에서 숨쉬기 가이드를 따라 하며 마음을 쉬게 하는 VR 콘텐츠예요.
저는 이 프로젝트에서 **Unity 기반 3D 불멍 게임과 맵**을 만들었어요.

- 🌬 **숨쉬기 가이드:** 들숨·날숨 타이머와 꽃잎 애니메이션 (`BreatheController`)
- 🪵 **모닥불:** 장작 연료에 따라 불 크기·빛·소리가 바뀌어요 (`CampfireController`, `LogFuel`, `FireZone`)
- 🌸 **계절 배경:** 봄·가을·겨울 숲과 하늘을 Addressables로 바꿔요 (`SeasonAddressableLoader`, `SeasonSkyApplier`)
- 🎮 **XR UI:** 컨트롤러 그리드 내비게이션, 햅틱, 입력 모듈 전환 (`GridNavFlexible`, `HapticsUtility`, `InputModuleSwitcher`)

This repository contains the project's own **Assets** content: scripts, the `Fire.unity` scene,
its lighting data, season prefabs and UI sprites.

## Third-party assets (not included)

The following Unity Asset Store packages are used but not included, because their licenses
don't allow redistribution. Import them into `Assets/` from the Asset Store before opening the scene:

- Adventure Nature Vol.4
- AllSkyFree
- Environment Sounds
- Firewood_Pack
- Free Fire VFX URP
- Ian's Fire Pack Universal
- Starfield Skybox
- TriForge Assets (Fantasy Forest Environment)
- Yughues Free Ground Materials

## Setup

1. Create a Unity project with URP.
2. Copy this repository's contents into the project's `Assets/` folder.
3. Import the packages listed above.
4. Open `Fire.unity`.
