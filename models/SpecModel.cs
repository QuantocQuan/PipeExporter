using System;
using System.Collections.Generic;

namespace ExporterPipe.models
{
    public class SpecModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SpecType { get; set; }
        public string Material { get; set; }
        public string Manufacture { get; set; }
        public string Description { get; set; }
        public string Discipline { get; set; }
        public string Finish { get; set; }
        public string InstallType { get; set; }
        public string ItemGroup { get; set; }
        public string Specification { get; set; }
        public string Range { get; set; }
        public string Origin { get; set; }
        public string Model { get; set; }
        public string Item { get; set; }
        public string ItemMajor { get; set; }
        public List<object> SpecSizes { get; set; }
        public DateTime ModifyTime { get; set; }
        public string ItemNumber { get; set; }
        public string ItemMinor { get; set; }
        public string LibraryCode { get; set; }
        public string Code { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }
        public int Status { get; set; }
        public int Type { get; set; }
        public int LibraryId { get; set; }
        public string DisciplineId { get; set; }
        public int ManufactureId { get; set; }
        public int MaterialId { get; set; }
        public int FinishId { get; set; }
        public string ItemId { get; set; }
        public string ItemGroupId { get; set; }
        public int ItemMajorId { get; set; }
        public int SpecificationId { get; set; }
        public int InstallTypeId { get; set; }
        public int RangeId { get; set; }
        public int OriginId { get; set; }
        public string DisciplineCode { get; set; }
        public string ManufactureCode { get; set; }
        public string MaterialCode { get; set; }
        public string FinishCode { get; set; }
        public string ItemCode { get; set; }
        public string ItemGroupCode { get; set; }
        public string ItemMajorCode { get; set; }
        public string SpecificationCode { get; set; }
        public string InstallTypeCode { get; set; }
        public string RangeCode { get; set; }
        public string OriginCode { get; set; }

        public override string ToString()
        {
            return $"[{Id}] {Name} | Type: {SpecType} | Material: {Material} | Manufacture: {Manufacture} | " +
                   $"Description: {Description} | Discipline: {Discipline} | Finish: {Finish} | Specification: {Specification}";
        }
    }
}
