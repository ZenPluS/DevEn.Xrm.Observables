using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

namespace DevEn.Xrm.Observables.UnitTest
{
    /// <summary>
    /// Integration tests for the <see cref="ObservableEntity{TEntity}"/> class.
    /// </summary>
    [TestClass]
    public class ObservableEntityIntegrationTest
    {
        private class TestEntity : Entity
        {
            public TestEntity() : base("testentity") { }
        }

        /// <summary>
        /// Tests implicit conversion from <see cref="ObservableEntity{TEntity}"/> to <see cref="Entity"/>.
        /// </summary>
        [TestMethod]
        public void ImplicitConversion_ToEntity_ReturnsCorrectEntity()
        {
            var entity = new TestEntity();
            var observableEntity = ObservableEntity<TestEntity>.Create(entity);

            Entity convertedEntity = observableEntity;

            Assert.AreEqual(entity, convertedEntity);
        }

        /// <summary>
        /// Tests implicit conversion from <see cref="Entity"/> to <see cref="ObservableEntity{TEntity}"/>.
        /// </summary>
        [TestMethod]
        public void ImplicitConversion_FromEntity_ReturnsObservableEntity()
        {
            var entity = new TestEntity();
            ObservableEntity<TestEntity> observableEntity = entity;

            Assert.IsNotNull(observableEntity);
            Assert.AreEqual(entity, observableEntity.GetEntity());
        }

        /// <summary>
        /// Tests implicit conversion from a logical name to <see cref="ObservableEntity{TEntity}"/>.
        /// </summary>
        [TestMethod]
        public void ImplicitConversion_FromLogicalName_ReturnsObservableEntity()
        {
            var logicalName = "testentity";
            ObservableEntity<Entity> observableEntity = logicalName;

            Assert.IsNotNull(observableEntity);
            Assert.AreEqual(logicalName, observableEntity.GetEntity().LogicalName);
        }
    }
}
