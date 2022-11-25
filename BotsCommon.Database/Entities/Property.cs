using System;
using System.ComponentModel.DataAnnotations;

namespace BotsCommon.Database.Entities
{
    public sealed class Property
    {
        [Key]
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
