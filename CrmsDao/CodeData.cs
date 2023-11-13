using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace CrmsDao
{
    [DataContract()]
    public class CodeData : ICrmsModel
    {
        [DataMember(Name="Code")]
        public string Code { get; set; }
        [DataMember(Name="Code2")]
        public string Code2 { get; set; }
        [DataMember(Name="Name")]
        public string Name { get; set; }
        [DataMember(Name = "Value")]
        public int? Value { get; set; }  //Add 2019/09/04 yano #4011
    }

    [DataContract()]
    public class CodeDataList : CodeData {
        [DataMember(Name="DataList")]
        public List<CodeData> DataList { get; set; }
    }
}
