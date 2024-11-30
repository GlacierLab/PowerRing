using System.Runtime.InteropServices;

namespace PowercfgInterface
{
    public class Instance
    {
        public string? ActiveProfile = null;

        [DllImport(@"kernel32.dll")]
        extern private static bool GetSystemPowerStatus(out PowerStatus BatteryInfo);


        public Instance()
        {
            
        }



        public async Task Init()
        {
            await GetActiveProfile();
            await EnableHiddenAttr();
        }

        public bool IsReady()
        {
            return ActiveProfile != null;
        }


        // Get and update current active profile
        private async Task<string?> GetActiveProfile()
        {
            string? output = await Pipe.RunAsync("powercfg.exe", ["/GETACTIVESCHEME"]);
            if (output != null)
            {
                output = output.Split(":")[1];
                output = output.Split("(")[0];
                output = output.Trim();
                ActiveProfile = output;
                Console.WriteLine("Active Profile GUID: " + output);
                return output;
            }
            return null;
        }

        public async Task<bool> IsProfileChanged()
        {
            if (ActiveProfile != null)
            {
                return ActiveProfile == (await GetActiveProfile());
            }
            return false;
        }

        private async Task<bool> EnableHiddenAttr()
        {
            if (ActiveProfile != null)
            {
                await Pipe.RunAsync("powercfg.exe", ["-attributes", "SUB_PROCESSOR", "be337238-0d82-4146-a960-4f3749d470c7", "-ATTRIB_HIDE"]);
                string? output = await Pipe.RunAsync("powercfg.exe", ["/QUERY", ActiveProfile, "54533251-82be-4824-96c1-47b60b740d00", "be337238-0d82-4146-a960-4f3749d470c7"]);
                if (output != null)
                {
                    return output.LastIndexOf("be337238-0d82-4146-a960-4f3749d470c7") > 0;
                }
            }
            return false;
        }

        // Return true if fail
        // REF: https://www.cnblogs.com/netlog/p/15914468.html
        private static bool IsAC()
        {
            if (GetSystemPowerStatus(out PowerStatus ps) == true)
            {
                return ps.ACLineStatus == 1;
            }
            return true;
        }


        private async Task ApplyUpdate()
        {
            if (ActiveProfile != null)
            {
                await Pipe.RunAsync("powercfg.exe", ["-S", "SCHEME_CURRENT"]);
            }
        }

        private async Task ChangeBoostMode(bool AC, int mode)
        {
            if (ActiveProfile != null)
            {
                await Pipe.RunAsync("powercfg.exe", [AC ? "/SETACVALUEINDEX" : "/SETDCVALUEINDEX", ActiveProfile, "54533251-82be-4824-96c1-47b60b740d00", "be337238-0d82-4146-a960-4f3749d470c7", mode.ToString()]);
            }
        }

        public async void ChangeBoostModeAndApply(int mode)
        {
            if (ActiveProfile != null)
            {
                await ChangeBoostMode(IsAC(), mode);
                await ApplyUpdate();
            }
        }
    }

    public struct PowerStatus
    {
        public byte ACLineStatus;
        public byte BatteryFlag;
        public byte BatteryLifePercent;
        public byte Reserved;
        public int BatteryLifeTime;
        public int BatteryFullLifeTime;
    }
}
