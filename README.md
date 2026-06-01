# Conkist GDK Core

O **Conkist GDK Core** é o pacote fundamental do Conkist Game Development Kit (GDK). Ele reúne um conjunto de utilitários, padrões de projeto (design patterns) e ferramentas de editor otimizadas para acelerar o desenvolvimento de jogos na Unity.

## Recursos Principais

O pacote é composto pelos seguintes módulos principais:

*   **Padrão Singleton (`Runtime/Singleton.cs`)**: Implementação genérica e performática do padrão Singleton para `MonoBehaviour` e classes C# puras, garantindo instâncias persistentes e de acesso global seguro nas cenas.
*   **Loading Manager (`Runtime/Loading`)**: Gerenciador de carregamento assíncrono de cenas e recursos, integrado ao pipeline do Addressables e UniTask.
*   **EventManager (`Runtime/Managers`)**: Sistema desacoplado de eventos do tipo Publish/Subscribe para comunicação eficiente entre componentes do jogo.
*   **Build Pipelines (`Editor/BuildPipelines`)**: Automações e scripts utilitários do editor para configurar, otimizar e exportar compilações de forma padronizada.

## Requisitos e Dependências

Este pacote foi desenvolvido para a **Unity 6000.3** ou superior e requer os seguintes pacotes externos:

1.  **UniTask** (`com.cysharp.unitask`): Biblioteca para alocação eficiente de tarefas assíncronas (async/await) na Unity.
2.  **Unity Addressables** (`com.unity.addressables`): Gerenciador avançado de carregamento de assets dinâmicos.

As dependências são resolvidas automaticamente através do manifest do pacote (`package.json`).

## Instalação

Para adicionar o Conkist GDK Core ao seu projeto Unity, adicione a seguinte linha ao bloco `dependencies` no arquivo `Packages/manifest.json` do seu projeto:

```json
"me.conkist.gdk.core": "https://github.com/Conkist/Conkist.GDK.Core.git"
```

## Como Usar

### Exemplo de Singleton

```csharp
using Conkist.GDK;
using UnityEngine;

public class GameManager : SingletonBehaviour<GameManager>
{
    protected override void Awake()
    {
        base.Awake();
        // Lógica de inicialização aqui
    }

    public void IniciarJogo()
    {
        Debug.Log("Jogo Iniciado.");
    }
}
```

### Exemplo de EventManager

```csharp
using Conkist.GDK;

// Se inscrever em um evento
EventManager.StartListening("JogadorMorreu", OnJogadorMorreu);

// Disparar um evento
EventManager.TriggerEvent("JogadorMorreu");
```