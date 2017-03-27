using System;
using Microsoft.ProjectOxford.Face.Contract;

namespace Microsoft.ProjectOxford.Face.Model
{
    public class PersonExtended : Person
    {
        public Guid GroupId { get; set; }
    }
}