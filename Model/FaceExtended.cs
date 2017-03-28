using System;

namespace Microsoft.ProjectOxford.Face.Model
{
    public class FaceExtended : Microsoft.ProjectOxford.Face.Contract.Face
    {
        public Guid FaceListId { get; set; }
        public Guid PersonId { get; set; }
        public string ImageName { get; set; }
    }
}