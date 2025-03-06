using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.Xrm.Sdk;

namespace DevEn.Xrm.Observables.UnitTest
{
    /// <summary>
    /// Unit tests for the <see cref="ObservableEntity{TEntity}"/> class.
    /// </summary>
    [TestClass]
    public class ObservableEntityUnitTest
    {
        /// <summary>
        /// Test entity class for unit tests.
        /// </summary>
        /// <inheritdoc />
        public class TestEntity : Entity
        {
            public TestEntity() : base("testentity") { }
        }

        /// <summary>
        /// Tests if the indexer correctly retrieves the value of an attribute.
        /// </summary>
        [TestMethod]
        public void Indexer_GetValue_ReturnsCorrectValue()
        {
            var entity = new TestEntity
            {
                ["testAttribute"] = "testValue"
            };
            var observableEntity = ObservableEntity<TestEntity>.Create(entity);

            var value = observableEntity["testAttribute"];

            Assert.AreEqual("testValue", value);
        }

        /// <summary>
        /// Tests if setting a value through the indexer invokes the subscribed delegate.
        /// </summary>
        [TestMethod]
        public void Indexer_SetValue_InvokesDelegate()
        {
            var entity = new TestEntity();
            var observableEntity = ObservableEntity<TestEntity>.Create(entity);
            var delegateInvoked = false;

            observableEntity.AddOnChange("testAttribute", new Action(() => delegateInvoked = true));
            observableEntity["testAttribute"] = "newValue";

            Assert.IsTrue(delegateInvoked);
        }

        /// <summary>
        /// Tests if <see cref="ObservableEntity{TEntity}.GetValue{T}(string)"/> method retrieves the correct value.
        /// </summary>
        [TestMethod]
        public void GetValue_ReturnsCorrectValue()
        {
            var entity = new TestEntity
            {
                ["testAttribute"] = "testValue"
            };
            var observableEntity = ObservableEntity<TestEntity>.Create(entity);

            var value = observableEntity.GetValue<string>("testAttribute");

            Assert.AreEqual("testValue", value);
        }

        /// <summary>
        /// Tests if <see cref="ObservableEntity{TEntity}.SetValue{T}(string, T)"/> method sets the correct value.
        /// </summary>
        [TestMethod]
        public void SetValue_SetsCorrectValue()
        {
            var entity = new TestEntity();
            var observableEntity = ObservableEntity<TestEntity>.Create(entity);

            observableEntity.SetValue("testAttribute", "testValue");

            Assert.AreEqual("testValue", entity["testAttribute"]);
        }

        /// <summary>
        /// Tests if <see cref="ObservableEntity{TEntity}.SetValue{T}(string, T)"/> method invokes the subscribed delegate.
        /// </summary>
        [TestMethod]
        public void SetValue_InvokesDelegate()
        {
            var entity = new TestEntity();
            var observableEntity = ObservableEntity<TestEntity>.Create(entity);
            var delegateInvoked = false;

            observableEntity.AddOnChange("testAttribute", new Action(() => delegateInvoked = true));
            observableEntity.SetValue("testAttribute", "newValue");

            Assert.IsTrue(delegateInvoked);
        }

        /// <summary>
        /// Tests if <see cref="ObservableEntity{TEntity}.AddOnChange(string, Delegate[])"/> method adds a delegate that gets invoked on attribute change.
        /// </summary>
        [TestMethod]
        public void AddOnChange_AddsDelegate()
        {
            var entity = new TestEntity();
            var observableEntity = ObservableEntity<TestEntity>.Create(entity);
            var delegateInvoked = false;

            observableEntity.AddOnChange("testAttribute", new Action(() => delegateInvoked = true));
            observableEntity["testAttribute"] = "newValue";

            Assert.IsTrue(delegateInvoked);
        }

        /// <summary>
        /// Tests if <see cref="ObservableEntity{TEntity}.RemoveOnChange(string)"/> method removes the delegate.
        /// </summary>
        [TestMethod]
        public void RemoveOnChange_RemovesDelegate()
        {
            var entity = new TestEntity();
            var observableEntity = ObservableEntity<TestEntity>.Create(entity);
            var delegateInvoked = false;
            Action action = () => delegateInvoked = true;

            observableEntity.AddOnChange("testAttribute", action);
            observableEntity.RemoveOnChange("testAttribute");
            observableEntity["testAttribute"] = "newValue";

            Assert.IsFalse(delegateInvoked);
        }

        /// <summary>
        /// Tests if <see cref="ObservableEntity{TEntity}.InvokeAllOnChange"/> method invokes all subscribed delegates.
        /// </summary>
        [TestMethod]
        public void InvokeAllOnChange_InvokesAllDelegates()
        {
            var entity = new TestEntity();
            var observableEntity = ObservableEntity<TestEntity>.Create(entity);
            var delegateInvoked1 = false;
            var delegateInvoked2 = false;

            observableEntity.AddOnChange("testAttribute1", new Action(() => delegateInvoked1 = true));
            observableEntity.AddOnChange("testAttribute2", new Action(() => delegateInvoked2 = true));
            observableEntity.InvokeAllOnChange();

            Assert.IsFalse(delegateInvoked1);
            Assert.IsFalse(delegateInvoked2);
        }

        /// <summary>
        /// Tests if <see cref="ObservableEntity{TEntity}.InvokeOnChange(string)"/> method invokes the specific delegate for a given attribute.
        /// </summary>
        [TestMethod]
        public void InvokeOnChange_InvokesSpecificDelegate()
        {
            var entity = new TestEntity();
            var observableEntity = ObservableEntity<TestEntity>.Create(entity);
            var delegateInvoked = false;

            observableEntity.AddOnChange("testAttribute", new Action(() => delegateInvoked = true));
            observableEntity.InvokeOnChange("testAttribute");

            Assert.IsTrue(delegateInvoked);
        }
    }
}
