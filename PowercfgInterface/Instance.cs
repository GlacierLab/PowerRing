namespace PowercfgInterface
{
    public class Instance
    {
        public string? ActiveProfile=null;

        public Instance() {
            GetActiveProfile();
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
                output=output.Trim();
                ActiveProfile = output;
                Console.WriteLine("Active Profile GUID: "+output);
                return output;
            }
            return null;
        }

    }
}
