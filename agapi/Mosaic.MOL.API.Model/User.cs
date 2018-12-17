using System;

namespace Mosaic.MOL.API.Model
{
    public class User
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public User Superior { get; set; }

        public string CodeAndName
        {
            get
            {
                return Code + " - " + Name;
            }
        }
    }
}
