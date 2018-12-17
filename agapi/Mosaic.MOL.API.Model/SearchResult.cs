using System;

namespace Mosaic.MOL.API.Model
{
    public class SearchResult
    {
        public int TotalRecords { get; set; }
        public object Payload { get; set; }

        private int pageSize;
        public int PageSize
        {
            set
            {
                pageSize = value;
            }
        }

        public int TotalPages
        {
            get
            {
                if (pageSize <= 0)
                {
                    return 0;
                }
                int result = (int)Math.Ceiling((decimal)TotalRecords/pageSize);
                return result;
            }
        }
    }
}
