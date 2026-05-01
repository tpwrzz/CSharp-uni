namespace lab1_fieldMask.lab1.Tests
{
    public class CopyFunctionTests : IClassFixture<PersonTestFixture>
    {
        private readonly List<Person> _allPersons;
        private readonly PersonTestFixture _fixt;

        public CopyFunctionTests(PersonTestFixture fixture)
        {
            _fixt = fixture;
            _allPersons = [fixture.P1, fixture.P2, fixture.P3];
        }

        [Fact]
        public void CopySameByMask_FindsCorrectMatches()
        {
            var maskSame = new PersonFieldMaskByte { Status = true };
            var maskCopy = new PersonFieldMaskByte { Age = true };

            var copies = PersonFieldMasksFunc.CopySameByMask(_allPersons, _fixt.P2, maskSame, maskCopy);

            Assert.Equal(2, copies.Count);
        }

        [Fact]
        public void CopySameByMask_CopiesCorrectFields()
        {
            var maskSame = new PersonFieldMaskByte { Status = true };
            var maskCopy = new PersonFieldMaskByte { Age = true, Height = true };

            var copies = PersonFieldMasksFunc.CopySameByMask(_allPersons, _fixt.P2, maskSame, maskCopy);

            foreach (var copy in copies)
            {
                Assert.Equal(_fixt.P2.Age, copy.Age);
                Assert.Equal(_fixt.P2.Height, copy.Height);
            }
        }

        [Fact]
        public void CopySameByMask_DoesNotModifyOriginals()
        {
            var maskSame = new PersonFieldMaskByte { Status = true };
            var maskCopy = new PersonFieldMaskByte { Age = true, FullName = true };

            PersonFieldMasksFunc.CopySameByMask(_allPersons, _fixt.P2, maskSame, maskCopy);

            Assert.Equal(23, _fixt.P3.Age);
            Assert.Equal("test3", _fixt.P3.FullName);
        }

        [Fact]
        public void CopySameByMask_PreservesNonCopiedFields()
        {
            var maskSame = new PersonFieldMaskByte { Status = true };
            var maskCopy = new PersonFieldMaskByte { Age = true };

            var copies = PersonFieldMasksFunc.CopySameByMask(_allPersons, _fixt.P2, maskSame, maskCopy);

            var p3Copy = copies.FirstOrDefault(c => c.FullName == "test3");
            Assert.NotNull(p3Copy);
            Assert.Equal(_fixt.P2.Age, p3Copy.Age);
            Assert.Equal("test3", p3Copy.FullName);
        }

        [Fact]
        public void CopySameByMask_ReturnsEmpty_WhenNoMatch()
        {
            var maskSame = new PersonFieldMaskByte { Status = true };
            var maskCopy = new PersonFieldMaskByte { Age = true };

            var reference = new Person
            {
                Id = Guid.NewGuid(),
                Age = 99,
                FullName = "ghost",
                Height = 100,
                Status = SocialStatus.Childfree
            };

            var copies = PersonFieldMasksFunc.CopySameByMask(_allPersons, reference, maskSame, maskCopy);
            Assert.Empty(copies);
        }
    }
}
