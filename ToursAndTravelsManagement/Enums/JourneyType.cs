namespace ToursAndTravelsManagement.Enums
{
    public enum JourneyType
    {
        OneWay = 1,
        Outstation = 2,
        Local = 3,
        Transfer = 4
    }

    public enum CabType
    {
        Hatchback = 1,
        Sedan = 2,
        SUV = 3,
        MUV = 4,
        Luxury = 5,
        TempoTraveller = 6
    }

    public enum CabStatus
    {
        Available = 1,
        Booked = 2,
        Maintenance = 3,
        Unavailable = 4
    }

    public enum CabBookingStatus
    {
        Pending = 1,
        Confirmed = 2,
        InProgress = 3,
        Completed = 4,
        Cancelled = 5
    }

    public enum PaymentMethod
    {
        Cash = 1,
        OnlinePayment = 2,
        CreditCard = 3,
        DebitCard = 4,
        UPI = 5
    }
}
