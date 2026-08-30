# Changelog

All notable changes to **Conkist GDK Core** (`me.conkist.gdk.core`) are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - Unreleased

First public release. Consolidates all architecture developed on top of the internal `0.1.0` prototype into a single, stable, documented foundation package.

### Added

- **Singletons** — `SingletonBehaviour<T>` (scene lifecycle + cross-scene persistence) and `PureSingleton<T>` (thread-safe lazy C# singleton).
- **Event system** — decoupled typed pub/sub via the static `EventManager`, `GameEvent` struct and `EventListener<T>` interface.
- **Command pattern** — `ICommand` / `IAsyncCommand`, `IUndoableCommand` / `IUndoableAsyncCommand`, orchestrated by `CommandExecutor` with undo/redo stacks; guidance for keeping randomness deterministic across redo.
- **Factory pattern (VContainer-integrated)** — `IFactory<T>` / `IAsyncFactory<T>`, `VContainerFactory<T>`, `PrefabFactory<T>` and `AddressablePrefabFactory<T>` with automatic dependency resolution.
- **Loading** — `LoadingManager` with async scene transitions and reference-counted / cached Addressables loading.
- **Audio** — `AudioManager`, `AudioTrigger`, `MusicController` with `AudioMixer` integration and object pooling.
- **Game state** — `GameStateManager` with event-driven transitions.
- **UI overlays** — `OverlayManager` and popup / loading overlays (UI Toolkit) driven by connectivity and UI events.
- **Network connectivity monitoring** — status events with a matching UI overlay component.
- **Deterministic PRNG** — `PseudoRandom`, an isolated-state Xorshift32 generator for reproducible replays, procedural generation and undo/redo.
- **JSON serialization** — `JsonExtensions` (`ToJson` / `FromJson`) and `JsonSerializableBase<T>`, built on Newtonsoft.Json and tuned to avoid circular references.
- **Utils** — collection / string / general extension helpers, NaughtyAttributes-based attribute drawers, `ImageStringConverter`, native share helpers.
- **Editor Tools** — editor menus, asset-preset preprocessors, custom drawers, and `ConkistProjectLinker` (automates `manifest.json` wiring for adjacent Conkist projects).
- **Native platform isolation** — `Conkist.GDK.Android` and `Conkist.GDK.iOS` assemblies compiled only for the matching build target.
- **Samples** — Conkist Logo, Audio, Singleton, Game States, Loading, Overlay and Events.
- Assembly-definition version defines / constraints for UniTask and VContainer.

### Changed

- Dependency injection migrated from Extenject to **VContainer**.
- Runtime assemblies consolidated into a single `Conkist.GDK.asmdef` (previous redundant `Utils` / `Android` / `iOS` sub-assemblies removed).
- Share-helper callbacks moved to the `Conkist.GDK.Utils` namespace.

### Fixed

- Track Addressables-loaded audio clips to prevent `Release` errors.
- Stop music when `Play()` is called with an empty clip list.
- Cache `AudioMixer` exposed-parameter checks to stop repeated console warnings.
- Guard overlay scene loading against unconfigured Addressable / build-scene keys.

### Dependencies

- `com.unity.addressables` 2.9.1
- UniTask 2.5.11 (git)
- VContainer 1.18.0 (OpenUPM scoped registry)
- Newtonsoft.Json

[Unreleased]: https://github.com/Conkist/Conkist.GDK.Core/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/Conkist/Conkist.GDK.Core/releases/tag/v1.0.0
