using System;

namespace Sale.Model.Base
{
    public class BaseModel<TKey>
    {
        public TKey Id { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset? LastModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTimeOffset? DeletedDate { get; set; }
        public string DeletedBy { get; set; }
    }
}
