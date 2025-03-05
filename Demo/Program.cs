using System;
using DevEn.Xrm.Observables;
using Microsoft.Xrm.Sdk;

namespace Demo;

class Program
{
    static void Main()
    {
        var entity = new Entity("account")
        {
            ["name"] = "Test",
            ["int1"] = 10,
            ["int2"] = 20
        };
        var observableAccount = ObservableEntity<Entity>.Create(entity);
        observableAccount.AddOnChange("name", () => DoSomething(observableAccount));

        Console.WriteLine(observableAccount["int3"]);
        observableAccount.SetValue("name", "TestUpdate");
        Console.WriteLine(observableAccount["int3"]);

        observableAccount.InvokeAllOnChange();

        ObservableEntity<Entity> a = "account";
    }

    public static void DoSomething(Entity entity)
    {
        entity["int3"] = entity.GetAttributeValue<int>("int1") * entity.GetAttributeValue<int>("int2");
    }
}
