using System;

namespace EntityCloner.Microsoft.EntityFrameworkCore.Tests.TestModels;
public abstract class Payment : IEntity
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}

public class CreditCardPayment : Payment
{
    public string CardNumber { get; set; }
    public string CardHolder { get; set; }
}

public class BankTransferPayment : Payment
{
    public string Iban { get; set; }
    public string BankName { get; set; }
}