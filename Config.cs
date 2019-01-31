using Sandbox.ModAPI;
using System;


namespace Stollie.NPCCrew
{

    public class NPCCrewConfig
    {
        public float thrustOutputMulitplierControl { get; set; }
        public float gyroOutputMulitplierControl { get; set; }

        public float gasGeneratorOutputMulitplierControl { get; set; }
        public float drillOutputMulitplierControl { get; set;}

        public float powerOutputMulitplierControl { get; set; }
        public float powerConsumptionDivisionControl { get; set; }
        public float drillEfficiencyMulitplierControl { get; set; }

        public bool forceColor { get; set; }
        public bool disableAIModeEnabled { get; set; }

        public NPCCrewConfig()
        {
            thrustOutputMulitplierControl = 3.0f;
            gyroOutputMulitplierControl = 3.0f;

            gasGeneratorOutputMulitplierControl = 3.0f;
            drillOutputMulitplierControl = 2.0f;

            powerOutputMulitplierControl = 3.0f;
            powerConsumptionDivisionControl = 2.0f;
            drillEfficiencyMulitplierControl = 3.0f;

            forceColor = true;
            disableAIModeEnabled = false;
        }

        public static NPCCrewConfig LoadConfigFile()
        {
            if (MyAPIGateway.Utilities.FileExistsInLocalStorage("NPCCrewConfig.xml", typeof(NPCCrewConfig)) == true)
            {
                try
                {
                    NPCCrewConfig config = null;
                    var reader = MyAPIGateway.Utilities.ReadFileInLocalStorage("NPCCrewConfig.xml", typeof(NPCCrewConfig));
                    string configcontents = reader.ReadToEnd();
                    config = MyAPIGateway.Utilities.SerializeFromXML<NPCCrewConfig>(configcontents);
                    //LogEntry("Found Config File");
                    return config;
                }
                catch (Exception exc)
                {
                    Logging.Instance.WriteLine(string.Format("Logging.WriteLine Error: {0}", exc.ToString()));
                }
            }

            NPCCrewConfig defaultconfig = new NPCCrewConfig();
            //LogEntry("Config File Not Found. Using Default Values");
            using (var writer = MyAPIGateway.Utilities.WriteFileInLocalStorage("NPCCrewConfig.xml", typeof(NPCCrewConfig)))
            {
                writer.Write(MyAPIGateway.Utilities.SerializeToXML<NPCCrewConfig>(defaultconfig));
            }
            return defaultconfig;
        }
    }
}
