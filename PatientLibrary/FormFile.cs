using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientLibrary
{
    public class FormFile
    {
        private string name = "";
        private string id = "";

        public string Name { get => name; set => name = value; }
        public string Id { get => id; set => id = value; }

        public FormFile(string fileName, string fileId)
        {
            Name = fileName;
            Id = fileId;
        }
    }
}
