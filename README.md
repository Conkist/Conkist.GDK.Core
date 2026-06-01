# Conkist GDK Core

O **Conkist GDK Core** é o pacote fundamental do Conkist Game Development Kit (GDK). Ele fornece uma base arquitetural sólida, modular e de altíssima performance para jogos na Unity, focando no desacoplamento de sistemas, gerenciamento eficiente de estados e na testabilidade do código.

Este documento detalha cada um dos padrões de projeto e ferramentas presentes no pacote, seus casos de uso recomendados e exemplos práticos de aplicação.

---

## 1. Padrão Singleton
Localização: `Runtime/`

O pacote Core separa a inicialização e o ciclo de vida de Singletons em dois utilitários dedicados, de acordo com o contexto necessário.

### A. `SingletonBehaviour<T>`
* **O que é:** Uma classe base genérica para componentes Unity (`MonoBehaviour`) que assegura que apenas uma instância exista na cena e permite persistência entre carregamentos de telas.
* **Quando usar:** Ideal para gerenciadores que dependem do ciclo de vida da Unity ou de componentes físicos nas cenas (ex: `SoundManager`, `InputManager`, `SceneLoader`).
* **Exemplo de Uso:**
  ```csharp
  using Conkist.GDK;
  using UnityEngine;

  public class GameManager : SingletonBehaviour<GameManager>
  {
      protected override void Awake()
      {
          base.Awake();
          // Inicialização segura
      }
  }
  ```

### B. `PureSingleton<T>`
* **O que é:** Uma classe base genérica em C# puro que utiliza inicialização tardia (`System.Lazy<T>`) segura para threads.
* **Quando usar:** Para utilitários ou controladores puramente lógicos que não necessitam de representação física na hierarquia da Unity (ex: `GameConfigRegistry`, `EncryptionUtility`).
* **Exemplo de Uso:**
  ```csharp
  public class CryptoManager : PureSingleton<CryptoManager>
  {
      public string Encrypt(string data) => "...";
  }
  ```

---

## 2. Framework de Serialização JSON (Newtonsoft.Json)
Localização: `Runtime/Serialization/`

Padroniza a forma como modelos de dados e estados de jogo são persistidos e convertidos, configurado nativamente com parâmetros otimizados para Unity (evitando recursões e referências circulares).

### A. Métodos de Extensão `JsonExtensions`
* **Quando usar:** Para converter estruturas rapidamente de/para JSON em qualquer script.
* **Caso de Uso:** Salvar configurações de preferências locais do jogador (`PlayerPrefs`) ou gerar payloads para comunicação HTTP com servidores backend.
* **Exemplo de Uso:**
  ```csharp
  using Conkist.GDK.Serialization;

  // Serialização simples
  string json = myObject.ToJson(pretty: true);

  // Deserialização de string para tipo concreto
  MyConfigData config = json.FromJson<MyConfigData>();
  ```

### B. `JsonSerializableBase<T>`
* **Quando usar:** Classe base ideal para criar modelos de dados e classes de progresso que necessitam de autogestão de carregamento e salvamento.
* **Caso de Uso:** Modelos de perfil de jogador (`PlayerProfile`), inventário, progresso de missões, configurações salvas.
* **Exemplo de Uso:**
  ```csharp
  [System.Serializable]
  public class GameSaveData : JsonSerializableBase<GameSaveData>
  {
      public string PlayerName;
      public int HighScore;
  }

  // Uso:
  string dataJson = saveState.ToJson();
  GameSaveData loadedState = GameSaveData.FromJson(dataJson);
  ```

---

## 3. Sistema Desacoplado de Eventos (Publish/Subscribe)
Localização: `Runtime/Events/`

Um sistema robusto e tipado de comunicação entre classes, eliminando a dependência direta entre sistemas emissores e receptores.

* **Componentes:**
  * `EventManager`: Barramento central estático para adição de ouvintes e despacho de eventos.
  * `GameEvent`: Struct de evento genérica para envio de mensagens com identificadores de string.
  * `EventListener<T>`: Interface que as classes devem assinar para processar eventos específicos.
* **Quando usar:** Comunicação entre sistemas de mecânica (Gameplay) e UI (Interface do Usuário). Por exemplo, quando o jogador recebe dano, a mecânica apenas dispara `PlayerDamagedEvent`, e a UI de barra de vida ou a tela vermelha capturam esse evento de forma independente.
* **Exemplo de Uso:**
  ```csharp
  using Conkist.GDK;

  // 1. Definição do Evento
  public struct HealthChangedEvent
  {
      public int CurrentHealth;
      public HealthChangedEvent(int health) => CurrentHealth = health;
  }

  // 2. Ouvinte (Listener)
  public class HealthBarUI : MonoBehaviour, EventListener<HealthChangedEvent>
  {
      private void OnEnable() => this.Subscribe<HealthChangedEvent>();
      private void OnDisable() => this.Unsubscribe<HealthChangedEvent>();

      public void OnEventCallback(HealthChangedEvent eventData)
      {
          UpdateUI(eventData.CurrentHealth);
      }
  }

  // 3. Emissor (Publisher)
  EventManager.TriggerEvent(new HealthChangedEvent(80));
  ```

---

## 4. Padrão Command
Localização: `Runtime/Commands/`

Desacopla a solicitação de uma ação do objeto que realmente a executa, fornecendo total suporte a ações reversíveis (Undo/Redo) e execução assíncrona.

> [!NOTE]
> No padrão Command, a ação de **Redo** (refazer) é conceitualmente idêntica a reexecutar o comando. Por isso, não é necessária uma interface `IRedoCommand` separada; o `CommandExecutor` simplesmente chama o método `Execute()` (ou `ExecuteAsync()`) do próprio comando novamente quando um Redo é solicitado.

### Determinismo e Aleatoriedade no Undo/Redo
Quando um comando envolve aleatoriedade (ex: rolar um dado ou sortear um efeito), chamar `Execute()` novamente no **Redo** gerará um resultado diferente, quebrando o determinismo do jogo. Para resolver isso no GDK, duas abordagens são recomendadas:
1. **Salvamento de Estado (Abordagem 1):** O comando sorteia o valor apenas na primeira execução e salva esse resultado em uma variável interna. Nas execuções subsequentes (Redo), ele apenas aplica o valor salvo em vez de sortear novamente.
2. **Snapshot de Semente com PseudoRandom (Abordagem 2):** O comando armazena o estado do gerador pseudoaleatório `PseudoRandom` do exato momento anterior à sua execução. Ao realizar o `Undo()`, ele restaura o estado do gerador, garantindo que o fluxo aleatório seja perfeitamente idêntico no re-carregamento.

* **Componentes:**
  * `ICommand` / `IAsyncCommand`: Interfaces para comandos simples.
  * `IUndoableCommand` / `IUndoableAsyncCommand`: Interfaces para comandos reversíveis (contendo o método `Undo()` / `UndoAsync()`).
  * `CommandExecutor`: Histórico e orquestrador de execução de comandos com suporte a pilhas de Undo/Redo.
* **Quando usar:** Sistemas de movimentação em jogos de tabuleiro/estratégia, comandos de input de jogadores, transações de inventário (ex: comprar item com opção de reembolso), e sistemas de edição de fases em tempo de execução.
* **Exemplo de Uso:**
  ```csharp
  using Conkist.GDK.Commands;

  public class MoveUnitCommand : IUndoableCommand
  {
      private Unit _unit;
      private Vector3 _previousPos;
      private Vector3 _newPos;

      public MoveUnitCommand(Unit unit, Vector3 target)
      {
          _unit = unit;
          _newPos = target;
      }

      public void Execute()
      {
          _previousPos = _unit.transform.position;
          _unit.transform.position = _newPos;
      }

      public void Undo()
      {
          _unit.transform.position = _previousPos;
      }
  }

  // Uso através do orquestrador
  CommandExecutor executor = new CommandExecutor();
  executor.Execute(new MoveUnitCommand(myUnit, new Vector3(1, 0, 1)));

  // Desfazer o movimento
  executor.Undo();
  ```

---

## 5. Padrão Factory (Integrado ao VContainer)
Localização: `Runtime/Factories/`

Centraliza a criação de objetos complexos de forma performática. Integra-se nativamente ao **VContainer** para que novos objetos criados tenham todas as suas dependências resolvidas automaticamente em tempo de execução.

* **Componentes:**
  * `IFactory<T>` / `IFactory<TParam, T>`: Interfaces síncronas padrões de criação.
  * `IAsyncFactory<T>` / `IAsyncFactory<TParam, T>`: Interfaces assíncronas (baseadas em `UniTask`) para carregamento e instanciação dinâmica.
  * `VContainerFactory<T>`: Cria instâncias puras C# resolvendo dependências via construtor.
  * `PrefabFactory<T>`: Instancia Prefabs locais na cena e resolve dependências via injeção (`[Inject]`) em MonoBehaviours clonados.
  * `AddressablePrefabFactory<T>`: Carrega o prefab de forma assíncrona através do `LoadingManager` (aproveitando o sistema de contagem de referências, cache e limpeza do GDK) e o instancia resolvendo dependências via VContainer.
* **Quando usar:** Invocação dinâmica de inimigos durante a partida (Spawn), criação dinâmica de widgets e janelas de UI, instanciação dinâmica de controladores lógicos em C#, e carregamento sob demanda de ativos pesados através do Addressables.

### Exemplo A: Injeção de Prefabs Locais (Síncrono)
```csharp
using VContainer;
using VContainer.Unity;
using Conkist.GDK.Factories;
using UnityEngine;

// 1. Registro no LifetimeScope da cena
public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private Enemy enemyPrefab;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<IFactory<Enemy>>(container => 
            new PrefabFactory<Enemy>(container, enemyPrefab), 
            Lifetime.Singleton);
    }
}

// 2. Uso injetado no Spawner
public class EnemySpawner
{
    private readonly IFactory<Enemy> _enemyFactory;

    public EnemySpawner(IFactory<Enemy> enemyFactory)
    {
        _enemyFactory = enemyFactory;
    }

    public void SpawnEnemy()
    {
        // Cria o clone na cena e injeta automaticamente todos os serviços necessários no Enemy!
        Enemy newEnemy = _enemyFactory.Create();
    }
}
```

### Exemplo B: Integração com `LoadingManager` (Assíncrono via Addressables)
Ideal para carregar prefabs da memória sob demanda (como inimigos raros ou janelas de UI complexas) sem mantê-los carregados na cena o tempo todo.

```csharp
using VContainer;
using VContainer.Unity;
using Cysharp.Threading.Tasks;
using Conkist.GDK.Factories;

// 1. Registro do Factory assíncrono no LifetimeScope
public class MenuLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Registra a fábrica assíncrona passando o endereço do Addressable
        builder.Register<IAsyncFactory<ShopPanel>>(container => 
            new AddressablePrefabFactory<ShopPanel>(container, "Prefabs/UI/ShopPanel"), 
            Lifetime.Singleton);
    }
}

// 2. Uso injetado no controlador de UI
public class MenuController
{
    private readonly IAsyncFactory<ShopPanel> _shopFactory;

    public MenuController(IAsyncFactory<ShopPanel> shopFactory)
    {
        _shopFactory = shopFactory;
    }

    public async UniTask OpenShopAsync()
    {
        // 1. Carrega dinamicamente via LoadingManager (gerenciando cache e memórias do GDK)
        // 2. Instancia o prefab
        // 3. Injeta dependências automáticas via VContainer
        ShopPanel shop = await _shopFactory.CreateAsync();
    }
}
```

---

## 6. Pseudo-Random Determinístico
Localização: `Runtime/Utils/`

Um gerador de números pseudoaleatórios (PRNG) 100% determinístico baseado no algoritmo **Xorshift32**. 

### Por que usar?
Diferente do `UnityEngine.Random` (que utiliza estado global baseado no tempo de sistema), o `PseudoRandom` mantém seu estado isolado em uma semente. Isso permite:
* Criar replays de partidas com comportamento idêntico.
* Geração procedural consistente de mapas e fases a partir de uma única semente.
* Flawless Undo/Redo sincronizando o estado do gerador aleatório em comandos de tabuleiro ou estratégia.

### Exemplo de Uso:
```csharp
using Conkist.GDK;
using UnityEngine;

public class DeterministicSpawner : MonoBehaviour
{
    private PseudoRandom _prng;

    private void Start()
    {
        // Inicializa o gerador com uma semente fixa
        _prng = new PseudoRandom(seed: 12345);

        // Gera 10 números idênticos em qualquer máquina
        for (int i = 0; i < 10; i++)
        {
            int value = _prng.Range(1, 100);
            Debug.Log($"Valor gerado: {value}");
        }
    }
}
```

---

## Como Instalar

Adicione o pacote local no gerenciador de pacotes da Unity (`Packages/manifest.json`) sob as dependências:

```json
"me.conkist.gdk.core": "file:me.conkist.gdk.core"
```