using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryApi.Domain.Enums
{
    public enum ReservationStatus
    {
        Reserved = 1,
        Released = 2,
        Fulfilled = 3,
        Rejected = 4
    }
}
