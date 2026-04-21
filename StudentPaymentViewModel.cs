using System;
using System.Collections.Generic;
using CourseProject.Models;

namespace ProjeAdin.Models
{
    public class PaymentRecord
    {
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
    }

    public class StudentPaymentViewModel
    {
        public decimal TotalFee { get; set; }
        public decimal PaidFee { get; set; }
        public decimal RemainingFee => TotalFee - PaidFee;
        public int Installments { get; set; }
       

        public List<PaymentRecord> PaymentHistory { get; set; } = new List<PaymentRecord>();
    }
    namespace CourseProject.Models
    {
        public class StudentPaymentViewModel
        {
            public Student Student { get; set; }
            public List<Payment> Payments { get; set; }
        }
    }
}
