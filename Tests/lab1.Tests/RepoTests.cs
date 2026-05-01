namespace lab1_fieldMask.lab1.Tests
{
    public class RepoTests : IClassFixture<PersonTestFixture>
    {
        private readonly InMemoryPersonRepository _repo;
        private readonly PersonTestFixture _fixture;

        public RepoTests(PersonTestFixture fixture)
        {
            _fixture = fixture;
            _repo = new InMemoryPersonRepository();
            _repo.Add(fixture.P1);
            _repo.Add(fixture.P2);
            _repo.Add(fixture.P3);
        }

        [Fact]
        public void GetAll_ReturnsAllAddedPersons()
        {
            var all = _repo.GetAll();
            Assert.Equal(3, all.Count);
        }

        [Fact]
        public void GetById_ReturnsCorrectPerson()
        {
            var result = _repo.GetById(_fixture.P1.Id);
            Assert.Equal(_fixture.P1.FullName, result.FullName);
        }

        [Fact]
        public void Remove_DeletesPersonFromRepo()
        {
            _repo.Remove(_fixture.P1.Id);
            var all = _repo.GetAll();
            Assert.Equal(2, all.Count);
            Assert.DoesNotContain(all, p => p.Id == _fixture.P1.Id);
        }

        [Fact]
        public void FindByName_ReturnsMatchingPersons()
        {
            var results = _repo.FindByName("test2");
            Assert.Single(results);
            Assert.Equal("test2", results[0].FullName);
        }

        [Fact]
        public void FindByName_ReturnsEmpty_WhenNoMatch()
        {
            var results = _repo.FindByName("nobody");
            Assert.Empty(results);
        }

        [Fact]
        public void GetAll_ReturnsNewList_NotSameReference()
        {
            var list1 = _repo.GetAll();
            list1.Clear();
            var list2 = _repo.GetAll();
            Assert.Equal(3, list2.Count);
        }
    }
}
