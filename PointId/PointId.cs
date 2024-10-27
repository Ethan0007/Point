using Microsoft.Win32;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
/*
 * OWNER: JOEVER MONCEDA
 */ 
namespace PointGen
{
    public class PointId
    {
        private static int counter = 0;

        public static string NewPointId()
        {
            byte[] uniqueBytes = new byte[32];

            byte[] machineIdBytes = GetMachineUniqueIdentifier();
            Array.Copy(machineIdBytes, 0, uniqueBytes, 0, 8);

            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            byte[] timestampBytes = BitConverter.GetBytes(timestamp);
            Array.Copy(timestampBytes, 0, uniqueBytes, 8, 8);

            int processId = Environment.ProcessId;
            byte[] processIdBytes = BitConverter.GetBytes(processId);
            Array.Copy(processIdBytes, 0, uniqueBytes, 16, 4);

            byte[] counterBytes = BitConverter.GetBytes(counter++);
            Array.Copy(counterBytes, 0, uniqueBytes, 20, 4);

            string publicIp = GetPublicIPAddressAsync().GetAwaiter().GetResult();
            byte[] ipBytes = ParseIpAddressToBytes(publicIp);
            Array.Copy(ipBytes, 0, uniqueBytes, 24, 4);

            string macIP = GetMacAddress();
            byte[] macBytes = ParseIpAddressToBytes(macIP);
            Array.Copy(macBytes, 0, uniqueBytes, 28, 4);

            string formattedId = FormatIdentifier(uniqueBytes);
            return formattedId;
        }

        private static string FormatIdentifier(byte[] bytes)
        {
            return $"{BitConverter.ToString(bytes, 0, 4).Replace("-", "")}-" +
                   $"{BitConverter.ToString(bytes, 4, 4).Replace("-", "")}-" +
                   $"{BitConverter.ToString(bytes, 8, 4).Replace("-", "")}-" +
                   $"{BitConverter.ToString(bytes, 9, 4).Replace("-", "")}-" +
                   $"{BitConverter.ToString(bytes, 16, 6).Replace("-", "").ToUpper()}";
        }

        private static byte[] GetMachineUniqueIdentifier()
        {
            string uniqueId;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                uniqueId = GetWindowsMachineGuid();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                uniqueId = GetLinuxMachineId();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                uniqueId = GetMacHardwareUUID();
            }
            else
            {
                uniqueId = "default";
            }

            byte[] uniqueIdBytes = Encoding.ASCII.GetBytes(uniqueId);
            Array.Resize(ref uniqueIdBytes, 8);
            return uniqueIdBytes;
        }

        private static string GetWindowsMachineGuid()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography"))
                {
                    return key?.GetValue("MachineGuid")?.ToString() ?? "WINDOW-NA";
                }
            }
            catch
            {
                return "WINDOW-NA";
            }
        }

        private static string GetLinuxMachineId()
        {
            try
            {
                return File.ReadAllText("/etc/machine-id").Trim();
            }
            catch
            {
                return "LINUX-NA";
            }
        }

        private static string GetMacHardwareUUID()
        {
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "ioreg";
                    process.StartInfo.Arguments = "-rd1 -c IOPlatformExpertDevice";
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    const string uuid = "IOPlatformUUID";
                    int uuidIndex = output.IndexOf(uuid);
                    if (uuidIndex >= 0)
                    {
                        int startIndex = uuidIndex + uuid.Length + 4;
                        int endIndex = output.IndexOf('"', startIndex);
                        return output.Substring(startIndex, endIndex - startIndex);
                    }
                }
            }
            catch
            {
                return "PLATFORM-NA";
            }

            return "NA";
        }

        private static async Task<string> GetPublicIPAddressAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    string ip = await client.GetStringAsync("https://api.ipify.org");
                    return ip.Trim();
                }
            }
            catch
            {
                return "0.0.0.0";
            }
        }

        private static string GetMacAddress()
        {
            var macAddress = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up &&
                              (nic.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                               nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel))
                .Select(nic => nic.GetPhysicalAddress())
                .FirstOrDefault();

            return macAddress != null ? macAddress.ToString() : "00-00-00-00-00-00";
        }

        private static byte[] ParseIpAddressToBytes(string ipAddress)
        {
            var ipParts = ipAddress.Split('.');
            if (ipParts.Length != 4)
            {
                return new byte[] { 0, 0, 0, 0 };
            }

            byte[] bytes = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                if (int.TryParse(ipParts[i], out int value) && value >= 0 && value <= 255)
                {
                    bytes[i] = (byte)value; 
                }
                else
                {
                    return new byte[] { 0, 0, 0, 0 };
                }
            }
            return bytes;
        }
    }
}
