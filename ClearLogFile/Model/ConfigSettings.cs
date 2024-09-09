using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearLogFile.Model
{
    public class ClearLogList 
    {
        public string? service_name { get; set; }
        public List<string?>? path { get; set; }
        public int? keep_last_n_days { get; set; }
        public List<string?>? extention_list { get; set; }
    }
    public class ConfigSettings
    {
        public List<ClearLogList>? clear_log_list { get; set; }
        public long? run_from { get; set; }
        public long? run_to { get; set; }
    }
}
