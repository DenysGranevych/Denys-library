  [TestClass]
    public class BusinessTypeRepositoryTest
    {
        #region private variables

        private DbConnection _connection;
        private WasteTracContext _databaseContext;
        private BusinessTypeRepository _repository;

        #endregion

        #region Test Initializer

        [TestInitialize]
        public void Initialize()
        {
            _connection = Effort.DbConnectionFactory.CreateTransient();
            _databaseContext = new WasteTracContext(_connection);
            _repository = new BusinessTypeRepository(_databaseContext);

        }

        #endregion

        #region Test Methods

        [TestMethod]
        public void BusinessTypeRepository_GetById_WithNonExistingId_ReturnsNull()
        {
            // Arrange
            Guid nonExistingId = Guid.NewGuid();

            // Act
            var businessType = _repository.GetById(nonExistingId);

            // Assert
            Assert.AreEqual(null, businessType);
        }

        [TestMethod]
        public void BusinessTypeRepository_GetAll()
        {
            _repository.Create(new BusinessType()
            {
                Id = Guid.NewGuid(),
                Name = "BusinessType",
                Code = "0",
                IsActive = true,
                IsDeleted = false,
            });
            _databaseContext.SaveChanges();

            var result = _repository.GetAll().ToList();


            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("BusinessType", result[0].Name);
        }

        #endregion
    }
}