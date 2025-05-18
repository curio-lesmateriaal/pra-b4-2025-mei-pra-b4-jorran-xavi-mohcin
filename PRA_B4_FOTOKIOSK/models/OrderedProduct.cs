using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRA_B4_FOTOKIOSK.models
{
    public class OrderedProduct
    {
        public string PhotoId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }

        public override string ToString()
        {
            return $"Foto #{PhotoId} - {ProductName} x{Quantity} - €{TotalPrice:F2}";
        }
    }
}
