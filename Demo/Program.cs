using System;
using DevEn.Xrm.Observables;
using Microsoft.Xrm.Sdk;

namespace Demo;

internal static class Program
{
    //internal static void Main()
    //{
    //    var observableEntity = ObservableEntity.Create("account");
    //    using (observableEntity.Subscribe(
    //               onNext: kvp => Console.WriteLine($"Key: {kvp.Key}, Value: {kvp.Value}"),
    //               onError: ex => Console.WriteLine($"Error: {ex.Message}"),
    //               onCompleted: () => Console.WriteLine("Observation complete."))
    //          )
    //    {
    //        observableEntity.TryGetOrAdd("Apples", 10);
    //        observableEntity.TryAddOrUpdate("Oranges", 5);
    //        observableEntity.TryUpdate("Apples", 15);
    //        observableEntity.TryDelete("Oranges");

    //        Console.WriteLine("JSON representation:");
    //        Console.WriteLine(observableEntity.Json());
    //    }

    //    observableEntity.TryAddOrUpdate("Bananas", 7);
    //    Console.WriteLine("Finished operations without memory leaks.");

    //    Entity entity = observableEntity;
    //    var newObservableEntity = entity.ToObservable();
    //}

    private static void Main()
    {
        var entity = ObservableEntityAttributes.Create("account");

        // Sottoscrizione a "key1"
        var subscription = entity
            .Observe<int>("key1")
            .Subscribe(x => Console.WriteLine($"🔔 Valore aggiornato: {x}"));

        // Modifiche che devono essere intercettate
        entity["key1"] = 42; // Deve notificare "🔔 Valore aggiornato: 42"
        entity.SetAttributeValue("key1", 100); // Deve notificare "🔔 Valore aggiornato: 100"
        entity.Attributes["key1"] = 150; // Deve notificare "🔔 Valore aggiornato: 150"

        // Aspettiamo per osservare i risultati
        System.Threading.Thread.Sleep(1000);

        // Chiudiamo la sottoscrizione
        subscription.Dispose();

    }

    internal static void UpdateKey99(Entity entity, int value)
        => entity.Attributes["key99"] = value * 1.20;
}