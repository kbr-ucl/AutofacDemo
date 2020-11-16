using System.Text;
using AutofacDemo.CrossCut;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AutofacDemo.AutoMapperValidationTest
{
    public class AutoMapperConfigurationIsValidTests
    {
        [Fact]
        public void All_AutoMapper_Profiles_Is_Valid()
        {
            // Arrange
            var serviceProvider = AutofacHelper.GetServiceProvider("AutofacDemo.");
            var mapper = serviceProvider.GetService<IMapper>();
            var actual = new StringBuilder();

            // Act
            try
            {
                mapper.ConfigurationProvider.AssertConfigurationIsValid();
            }
            catch (AutoMapperConfigurationException e)
            {
                actual.AppendLine(e.Message);
            }

            // Assert
            Assert.True(actual.Length == 0, actual.ToString());
        }
    }
}

