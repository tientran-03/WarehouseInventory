using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouseInventory.Domain.Exceptions
{
    public class NotFoundException : BaseException
    {
        public NotFoundException(string resourceName, object identifier)
            : base("RESOURCE_NOT_FOUND", $"Không tìm thấy {resourceName} với mã '{identifier}'.")
        {
        }
    }
}
