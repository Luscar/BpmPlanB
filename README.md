# BpmPlanB

BpmPlanB fournit un moteur BPM léger destiné à être emballé en package NuGet et intégré dans différentes solutions clientes. Les définitions de processus sont décrites dans des fichiers JSON lisibles et le moteur délègue toute la logique de persistance via des interfaces de repository afin de laisser chaque client modéliser ses propres tables (Oracle ou autre) avec sa nomenclature.

## Caractéristiques principales

- **Définitions JSON** : chaque processus est décrit par un document JSON simple listant les étapes et l'étape de départ.
- **Étapes prises en charge** :
  - `business` – appel d'un service externe pour la logique d'affaires, étape linéaire.
  - `interactive` – création d'une tâche utilisateur via l'interface cliente, étape linéaire.
  - `decision` – évaluation d'une condition via un service/une requête et sélection d'une route, seule étape permettant plusieurs chemins.
  - `scheduled` – mise en pause jusqu'à une date/heure ou une durée, étape linéaire.
  - `signal` – attente d'un signal externe, étape linéaire.
  - `subProcess` – exécution d'un sous-processus réutilisable, étape linéaire.
- **Handlers typés** : chaque type d'étape possède son interface de handler (`IBusinessStepHandler`, `IInteractiveStepHandler`, etc.).
- **Persistance plug-in** : interfaces `IProcessDefinitionRepository` et `IWorkflowInstanceRepository` pour laisser le client choisir la structure de stockage.
- **Gestion des suspensions** : le moteur sait mettre un workflow en attente d'une date (étape schedulée) ou d'un signal externe.

## Exemple de définition JSON

```json
{
  "id": "onboarding",
  "name": "Processus d'onboarding",
  "version": 1,
  "startStepId": "affaire",
  "steps": {
    "affaire": {
      "type": "business",
      "name": "Préparation du dossier",
      "serviceIdentifier": "affaires/preparer-dossier",
      "nextStepId": "priseInfos"
    },
    "priseInfos": {
      "type": "interactive",
      "name": "Collecte des informations",
      "interfaceIdentifier": "ui/task",
      "role": "agent",
      "nextStepId": "verification"
    },
    "verification": {
      "type": "decision",
      "name": "Vérification de conformité",
      "serviceIdentifier": "verifications/conformite",
      "routes": {
        "valide": "activation",
        "refus": "abandon"
      },
      "defaultNextStepId": "abandon"
    },
    "activation": {
      "type": "scheduled",
      "name": "Activation différée",
      "nextStepId": "notification",
      "delay": "1.00:00:00"
    },
    "notification": {
      "type": "signal",
      "name": "Attente confirmation",
      "nextStepId": "fin",
      "signalKey": "activation-confirmee"
    },
    "fin": {
      "type": "subProcess",
      "name": "Fermeture",
      "subProcessId": "fermeture-dossier"
    },
    "abandon": {
      "type": "business",
      "name": "Refus client",
      "serviceIdentifier": "affaires/notifier-refus"
    }
  }
}
```

> Les propriétés `delay` et `resumeAt` d'une étape `scheduled` utilisent respectivement le format `TimeSpan` .NET (`"J.hh:mm:ss"`) et ISO 8601.

## Persistance personnalisable

Deux interfaces principales permettent d'intégrer la persistance aux systèmes existants :

```csharp
public interface IProcessDefinitionRepository
{
    Task<ProcessDefinition?> GetAsync(string processId, int? version, CancellationToken cancellationToken = default);
    Task SaveAsync(ProcessDefinition definition, CancellationToken cancellationToken = default);
}

public interface IWorkflowInstanceRepository
{
    Task CreateAsync(WorkflowInstance instance, CancellationToken cancellationToken = default);
    Task UpdateAsync(WorkflowInstance instance, CancellationToken cancellationToken = default);
    Task<WorkflowInstance?> GetAsync(Guid instanceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<WorkflowInstance>> GetScheduledAsync(DateTimeOffset scheduledBefore, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<WorkflowInstance>> GetWaitingForSignalAsync(string signalKey, CancellationToken cancellationToken = default);
}
```

Chaque client implémente ces interfaces avec sa propre base de données (Oracle, SQL Server, etc.).

## Enregistrer les handlers

```csharp
var registry = new WorkflowStepHandlerRegistry();
registry.Register<BusinessStepDefinition>(new MonBusinessHandler());
registry.Register<InteractiveStepDefinition>(new MaTacheHandler());
registry.Register<DecisionStepDefinition>(new MaDecisionHandler());
registry.Register<ScheduledStepDefinition>(new MonSchedulerHandler());
registry.Register<SignalStepDefinition>(new MonSignalHandler());
registry.Register<SubProcessStepDefinition>(new MonSousProcessHandler());
```

Un handler implémente l'interface correspondante, par exemple :

```csharp
public sealed class MaDecisionHandler : IDecisionStepHandler
{
    public async Task<WorkflowStepResult> ExecuteAsync(DecisionStepDefinition step, StepExecutionContext context, CancellationToken cancellationToken)
    {
        var resultat = await AppelerServiceDecisionAsync(step.ServiceIdentifier, context.Data, cancellationToken);
        if (step.Routes.TryGetValue(resultat, out var prochain))
        {
            return WorkflowStepResult.Continue(prochain);
        }

        return WorkflowStepResult.Continue(step.DefaultNextStepId);
    }
}
```

## Démarrer un workflow

```csharp
var engine = new WorkflowEngine(processRepo, instanceRepo, registry, serviceProvider);
var instance = await engine.StartAsync("onboarding");
```

Pour reprendre une instance planifiée ou en attente d'un signal :

```csharp
await engine.ResumeAsync(instance.Id);
await engine.PublishSignalAsync("activation-confirmee");
```

## Structure du dépôt

- `BpmPlanB.sln` – solution Visual Studio.
- `src/BpmPlanB.Core` – bibliothèque principale contenant les définitions d'étapes, les interfaces de repository, le moteur d'exécution et les utilitaires JSON.

## Prochaines étapes possibles

- Ajouter une implémentation de stockage en mémoire pour faciliter les tests.
- Fournir des packages NuGet séparés (Core + implémentations optionnelles).
- Intégrer une bibliothèque de logs/audits et un système de traçabilité.
