using System;
using System.Collections.Generic;
using System.Text;

namespace DicomApp
{
    public class DicomAttributeInfo
    {
        public string PatientName { get; set; }
        public string Modality { get; set; }
        public string StudyInstanceUID { get; set; }
        public string SOPClassUID { get; set; }
        public string SeriesDescription { get; set; }
        public string Value { get; internal set; }
        public string Tag { get; internal set; }
    }
}

