using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;


namespace TimeSheet.Models
{
    [DataContract]
    public class NotesDTO
    {
        [DataMember]
        public int DayId { get; set; }

        [DataMember]
        public string Description { get; set; }
    }
}
