# Conkist.GDK.Core

<p>
  <img src="https://img.shields.io/badge/Unity-6000.3.14f1-black?logo=unity" />
  <img src="https://img.shields.io/badge/C%23-dev-239120?logo=csharp" />
  <img src="https://img.shields.io/badge/version-1.0.0-blue" />
  <img src="https://img.shields.io/badge/license-MIT-green" />
</p>

Foundation layer of the **Conkist Game Development Kit** — a modular Unity architecture framework focused on decoupled systems, testable code and deterministic behavior.

```json
"me.conkist.gdk.core": "https://github.com/Conkist/Conkist.GDK.Core.git"
```

---

## Package Structure

```
me.conkist.gdk.core/
├── Runtime/
│   ├── Core/         → Architectural patterns (Conkist.GDK)
│   ├── Utils/        → Extensions and utilities (Conkist.GDK.Utils)
│   ├── Android/      → Native Android callbacks (platform-gated)
│   └── iOS/          → Native iOS callbacks (platform-gated)
└── Editor/
    └── Tools/        → Editor scripts, menus, attribute drawers (Conkist.GDK.Tools)
```

**Namespaces:**
- `Conkist.GDK` — structural patterns: singletons, events, commands, factories
- `Conkist.GDK.Utils` — extensions (`MyExtensions`, `MyCollections`, `MyString`), enums, constants
- `Conkist.GDK.Tools` — editor-only tooling, asset preset preprocessors

**Dependencies:** VContainer · UniTask · Addressables · Newtonsoft.Json

---

## Features

### 1. Singleton

Two variants for different lifecycle needs.

**`SingletonBehaviour<T>`** — MonoBehaviour-based, scene-persistent:
```csharp
using Conkist.GDK;

public class GameManager : SingletonBehaviour<GameManager>
{
    protected override void Awake()
    {
        base.Awake();
        // safe initialization
    }
}
```

**`PureSingleton<T>`** — pure C#, lazy/thread-safe, no scene dependency:
```csharp
using Conkist.GDK;

public class CryptoManager : PureSingleton<CryptoManager>
{
    public string Encrypt(string data) => "...";
}
```

---

### 2. JSON Serialization

Built on Newtonsoft.Json, tuned for Unity (circular ref safe, `PlayerPrefs`-ready).

**`JsonExtensions`** — quick conversion anywhere:
```csharp
using Conkist.GDK.Serialization;

string json = myObject.ToJson(pretty: true);
MyConfigData config = json.FromJson<MyConfigData>();
```

**`JsonSerializableBase<T>`** — self-managing save data:
```csharp
[System.Serializable]
public class GameSaveData : JsonSerializableBase<GameSaveData>
{
    public string PlayerName;
    public int HighScore;
}

string dataJson = saveState.ToJson();
GameSaveData loaded = GameSaveData.FromJson(dataJson);
```

---

### 3. Event System (Pub/Sub)

Decoupled typed communication. Gameplay logic fires events; UI and other systems subscribe independently — no direct references.

```csharp
using Conkist.GDK;

// 1. Define the event
public struct HealthChangedEvent
{
    public int CurrentHealth;
    public HealthChangedEvent(int health) => CurrentHealth = health;
}

// 2. Listener
public class HealthBarUI : MonoBehaviour, EventListener<HealthChangedEvent>
{
    private void OnEnable() => this.Subscribe<HealthChangedEvent>();
    private void OnDisable() => this.Unsubscribe<HealthChangedEvent>();

    public void OnEventCallback(HealthChangedEvent e) => UpdateUI(e.CurrentHealth);
}

// 3. Publisher
EventManager.TriggerEvent(new HealthChangedEvent(80));
```

---

### 4. Command Pattern

Full Undo/Redo support with async execution. `CommandExecutor` manages command stacks; Redo re-calls `Execute()` — no separate `IRedoCommand` needed.

> **Determinism with randomness:** snapshot `PseudoRandom` state before `Execute()`, restore it on `Undo()` for consistent replay.

```csharp
using Conkist.GDK.Commands;

public class MoveUnitCommand : IUndoableCommand
{
    private Unit _unit;
    private Vector3 _previousPos, _newPos;

    public MoveUnitCommand(Unit unit, Vector3 target)
    {
        _unit = unit; _newPos = target;
    }

    public void Execute()
    {
        _previousPos = _unit.transform.position;
        _unit.transform.position = _newPos;
    }

    public void Undo() => _unit.transform.position = _previousPos;
}

var executor = new CommandExecutor();
executor.Execute(new MoveUnitCommand(myUnit, new Vector3(1, 0, 1)));
executor.Undo();
```

---

### 5. Factory (VContainer-integrated)

Centralized object creation with automatic dependency injection at runtime.

| Type | Use case |
|---|---|
| `IFactory<T>` / `IAsyncFactory<T>` | Sync/async creation contracts |
| `VContainerFactory<T>` | Pure C# objects via constructor injection |
| `PrefabFactory<T>` | Local prefab instantiation with `[Inject]` resolution |
| `AddressablePrefabFactory<T>` | Async Addressables loading + VContainer injection |

**Local prefab (sync):**
```csharp
// Registration
builder.Register<IFactory<Enemy>>(
    c => new PrefabFactory<Enemy>(c, enemyPrefab), Lifetime.Singleton);

// Usage
Enemy enemy = _enemyFactory.Create(); // fully injected
```

**Addressables (async):**
```csharp
// Registration
builder.Register<IAsyncFactory<ShopPanel>>(
    c => new AddressablePrefabFactory<ShopPanel>(c, "Prefabs/UI/ShopPanel"), Lifetime.Singleton);

// Usage
ShopPanel shop = await _shopFactory.CreateAsync(); // loaded, instantiated, injected
```

---

### 6. Deterministic PRNG

Xorshift32-based `PseudoRandom` with isolated seeded state. Unlike `UnityEngine.Random`, state is reproducible — essential for replays, procedural generation and undo/redo determinism.

```csharp
using Conkist.GDK.Utils;

var prng = new PseudoRandom(seed: 12345);
int value = prng.Range(1, 100); // same result on any machine, any run
```

---

### 7. Native Platform Isolation

Android (`.aar`) and iOS (`.mm`) code lives in platform-gated assemblies — compiled only for the matching target. Editor and desktop builds stay clean.

- `Conkist.GDK.Android` — Android only
- `Conkist.GDK.iOS` — iOS only

---

## Install

Add to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "me.conkist.gdk.core": "https://github.com/Conkist/Conkist.GDK.Core.git",
    "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask#2.5.11"
  }
}
```

After cloning the parent repo, run `git submodule update --init --recursive`.

---

## Part of

**[Conkist GDK](https://github.com/Conkist)** — Game Development Kit for Unity · [conkist.me](https://conkist.me)
