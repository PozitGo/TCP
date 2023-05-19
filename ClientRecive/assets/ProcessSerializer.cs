using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace ClientRecive
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using System.IO;
    using System.Security.Cryptography.X509Certificates;
    using System.Text.Json;

    public class SerializableProcess
    {
        public int Id { get; set; }
        public string ProcessName { get; set; }
        public DateTime StartTime { get; set; }
        public string FilePath { get; set; }
        public double UsesRAM { get; set; }

        public SerializableProcess(Process process)
        {
            try
            {
                Id = process.Id;
                ProcessName = process.ProcessName;
                StartTime = process.StartTime;
                UsesRAM = Math.Round(process.WorkingSet64 / (double)(1024 * 1024), 0);
                FilePath = process.MainModule.FileName;
            }
            catch (Exception) { }
        }
        public SerializableProcess()
        {

        }
    }

    public class ProcessSerializer
    {
        public static string SerializeProcess(Process process) => JsonSerializer.Serialize(new SerializableProcess(process));

        public static SerializableProcess DeserializeProcess(string jsonString) => JsonSerializer.Deserialize<SerializableProcess>(jsonString);
    }

}
