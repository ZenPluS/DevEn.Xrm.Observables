using System;
using DevEn.Xrm.Observables;
using Microsoft.Xrm.Sdk;

namespace Demo;

class Program
{
    static void Main()
    {
        var entity = new Entity("account");
        entity["name"] = "Test";
        entity["int1"] = 10;
        entity["int2"] = 20;
        var observableAccount = ObservableEntity<Entity>.Create(entity);
        observableAccount.Subscribe("name", () =>
        {
            observableAccount["int3"] = observableAccount.GetValue<int>("int1") * observableAccount.GetValue<int>("int2");
        });

        Console.WriteLine(observableAccount["int3"]);
        observableAccount.SetValue("name", "TestUpdate");
        Console.WriteLine(observableAccount["int3"]);
    }
}
