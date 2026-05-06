using System;
using Xunit;
using Converter;

namespace Converter
{
    public class ConverterTests
    {
        [Fact]
        public void Do_WhenLessThanMinus100_ReturnsMinus2000()
        {
            Assert.Equal(-2000, Converter.Do(-100.0f));
        }

        [Theory]
        [InlineData(-94.5f, -94)]
        [InlineData(-50.1f, -50)]
        public void Do_WhenBetweenMinus100AndMinus50_ReturnsIntegerPart(float x, int expected)
        {
            Assert.Equal(expected, Converter.Do(x));
        }

        [Theory]
        [InlineData(-49.9f)]
        [InlineData(-1.0f)]
        public void Do_WhenBetweenMinus50And0_ThrowsArgumentException(float x)
        {
            Assert.Throws<ArgumentException>(() => Converter.Do(x));
        }

        [Theory]
        [InlineData(0.0f, -5)]
        [InlineData(65.7f, 65)]
        [InlineData(99.9f, 94)]
        public void Do_WhenBetween0And100_ReturnsIntegerPartMinus5(float x, int expected)
        {
            Assert.Equal(expected, Converter.Do(x));
        }

        [Fact]
        public void Do_WhenGreaterOrEqual100_Returns1000()
        {
            Assert.Equal(1000, Converter.Do(100.0f));
        }
    }
}