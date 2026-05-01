using Xunit.Abstractions;

namespace lab1_fieldMask.lab1.Tests
{
 
    public class ByteMaskTests : IClassFixture<PersonTestFixture>
    {
        private static PersonFieldMaskByte MaskWith(bool id = false, bool age = false,
            bool fullName = false, bool height = false, bool status = false)
            => new PersonFieldMaskByte { Id = id, Age = age, FullName = fullName, Height = height, Status = status };
        private readonly PersonTestFixture _fixt;
        private readonly ITestOutputHelper _output;

        public ByteMaskTests(PersonTestFixture fixture,ITestOutputHelper output)
        {
            _fixt = fixture;
            _output = output;
        }
        

        [Fact]
        public void Set_And_Get_SingleField_Works()
        {
            var mask = new PersonFieldMaskByte { Age = true };
            Assert.True(mask.Age);
            Assert.False(mask.Id);
            Assert.False(mask.FullName);
        }

        [Fact]
        public void UnionMask_CombinesBothMasks()
        {
            var a = MaskWith(id: true, age: true);
            var b = MaskWith(height: true, status: true);

            var union = PersonFieldMasksFunc.UnionMask(a, b);

            Assert.True(union.Id);
            Assert.True(union.Age);
            Assert.True(union.Height);
            Assert.True(union.Status);
            Assert.False(union.FullName);
        }

        [Fact]
        public void InvertMask_FlipsAllBits()
        {
            var mask = MaskWith(id: true, age: true);
            var inverted = PersonFieldMasksFunc.InvertMask(mask);

            Assert.False(inverted.Id);
            Assert.False(inverted.Age);
            Assert.True(inverted.FullName);
            Assert.True(inverted.Height);
            Assert.True(inverted.Status);
        }

        [Fact]
        public void Contains_ReturnsTrue_WhenAContainsB()
        {
            var a = MaskWith(id: true, age: true, height: true);
            var b = MaskWith(id: true, age: true);

            Assert.True(PersonFieldMasksFunc.Contains(a, b));
        }

        [Fact]
        public void Contains_ReturnsFalse_WhenADoesNotContainB()
        {
            var a = MaskWith(id: true, age: true);
            var b = MaskWith(height: true, status: true);

            Assert.False(PersonFieldMasksFunc.Contains(a, b));
        }

        [Fact]
        public void EmptyMask_AllFieldsFalse()
        {
            var mask = new PersonFieldMaskByte();
            Assert.False(mask.Id);
            Assert.False(mask.Age);
            Assert.False(mask.FullName);
            Assert.False(mask.Height);
            Assert.False(mask.Status);
        }

        [Fact]
        public void BytePrint_WithAgeMask_OutputContainsAge()
        {
            var mask = new PersonFieldMaskByte { Age = true };

            var output = ConsoleCapture.Run(() => PersonFieldMasksFunc.BytePrint(_fixt.P1, mask));

            Assert.Contains("21", output);
            Assert.DoesNotContain("test1", output);
        }

        [Fact]
        public void BytePrint_WithFullNameMask_OutputContainsName()
        {
            var mask = new PersonFieldMaskByte { FullName = true };

            var output = ConsoleCapture.Run(() => PersonFieldMasksFunc.BytePrint(_fixt.P1, mask));

            Assert.Contains("test1", output);
            Assert.DoesNotContain("21", output);
        }

        [Fact]
        public void BytePrint_WithAllFields_OutputContainsEverything()
        {
            var mask = PersonFieldMasksFunc.InvertMask(new PersonFieldMaskByte()); 

            var output = ConsoleCapture.Run(() => PersonFieldMasksFunc.BytePrint(_fixt.P1, mask));
            _output.WriteLine("Captured output:");
            _output.WriteLine(output);
            Assert.Contains("21", output);
            Assert.Contains("test1", output);
            Assert.Contains("161.5", output);
            Assert.Contains("Single", output);
        }

        [Fact]
        public void BytePrint_WithEmptyMask_OutputIsEmpty()
        {
            var mask = new PersonFieldMaskByte(); 

            var output = ConsoleCapture.Run(() => PersonFieldMasksFunc.BytePrint(_fixt.P1, mask));

            Assert.True(string.IsNullOrWhiteSpace(output));
        }

        [Fact]
        public void BytePrint_WithStatusMask_OutputContainsStatus()
        {
            var mask = new PersonFieldMaskByte { Status = true };

            var output = ConsoleCapture.Run(() => PersonFieldMasksFunc.BytePrint(_fixt.P2, mask));

            Assert.Contains("Divorced", output);
            Assert.DoesNotContain("test2", output);
        }
    }
}

