using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace Point.Test
{
    public class PointTests
    {
        [Fact]
        public void NewPointId_GeneratesUniqueId()
        {
            string expectedFormat = @"^[A-F0-9]{8}-[A-F0-9]{8}-[A-F0-9]{8}-[A-F0-9]{8}-[A-F0-9]{12}$"; // Updated regex pattern // Adjust the regex based on the expected format

            string result = Point.NewPointId();

            Assert.Matches(new Regex(expectedFormat), result);
        }

        [Fact]
        public void GetMacAddress_ReturnsValidMacAddress()
        {
            var methodInfo = typeof(Point).GetMethod("GetMacAddress", BindingFlags.NonPublic | BindingFlags.Static);

            var result = methodInfo.Invoke(null, null);

            Assert.NotNull(result);  
            Assert.IsType<string>(result); 

            string macAddress = (string)result;
            Assert.Matches(@"^[0-9A-Fa-f]{12}$", macAddress);
        }

        [Fact]
        public async Task GetPublicIPAddressAsync_ReturnsValidIpAddress()
        {
            var expectedIp = await Point.GetPublicIPAddressAsync();

            Assert.NotNull(expectedIp);
        }

        [Theory]
        [InlineData("192.168.1.1", new byte[] { 192, 168, 1, 1 })]
        [InlineData("255.255.255.255", new byte[] { 255, 255, 255, 255 })]
        [InlineData("0.0.0.0", new byte[] { 0, 0, 0, 0 })]
        [InlineData("10.0.0.1", new byte[] { 10, 0, 0, 1 })]
        [InlineData("invalid-ip", new byte[] { 0, 0, 0, 0 })]
        [InlineData("192.168.1", new byte[] { 0, 0, 0, 0 })] 
        [InlineData("192.168.1.256", new byte[] { 0, 0, 0, 0 })] 
        public void ParseIpAddressToBytes_ValidAndInvalidIp_ReturnsExpectedBytes(string ipAddress, byte[] expectedBytes)
        {
            var methodInfo = typeof(Point).GetMethod("ParseIpAddressToBytes", BindingFlags.NonPublic | BindingFlags.Static);

            byte[] result = (byte[])methodInfo.Invoke(null, new object[] { ipAddress });

            Assert.Equal(expectedBytes, result);
        }


        [Fact]
        public void GetMachineUniqueIdentifier_ReturnsExpectedIdentifier()
        {
            var methodInfo = typeof(Point).GetMethod("GetMachineUniqueIdentifier", BindingFlags.NonPublic | BindingFlags.Static);

            var result = methodInfo.Invoke(null, null);

            Assert.NotNull(result);

            Assert.Equal(8, ((byte[])result).Length); 
        }

        [Fact]
        public void GetWindowsMachineGuid_ReturnsExpectedGuid()
        {
            var methodInfo = typeof(Point).GetMethod("GetWindowsMachineGuid", BindingFlags.NonPublic | BindingFlags.Static);

            var result = methodInfo.Invoke(null, null);

            Assert.NotNull(result);

            Assert.IsType<string>(result);
        }

        [Fact]
        public void GetLinuxMachineId_ReturnsExpectedId()
        {
            var methodInfo = typeof(Point).GetMethod("GetLinuxMachineId", BindingFlags.NonPublic | BindingFlags.Static);

            string expectedId = "LinuxTestMachineId"; 

            var result = methodInfo.Invoke(null, null);

            Assert.NotNull(result);
        }

        [Fact]
        public void GetMacHardwareUUID_ReturnsExpectedUUID()
        {
            var methodInfo = typeof(Point).GetMethod("GetMacHardwareUUID", BindingFlags.NonPublic | BindingFlags.Static);

            var result = methodInfo.Invoke(null, null);

            Assert.NotNull(result);

            Assert.IsType<string>(result);
        }


        [Fact]
        public void FormatIdentifier_ValidBytes_ReturnsFormattedString()
        {
            var methodInfo = typeof(Point).GetMethod("FormatIdentifier", BindingFlags.NonPublic | BindingFlags.Static);
            byte[] inputBytes = new byte[32];

            var result = methodInfo.Invoke(null, new object[] { inputBytes });

            Assert.NotNull(result);

            Assert.IsType<string>(result);
        }
    }
}
