namespace OzunuInkisaf.Domain.Enums;

public enum TallyStatus
{
    Draft = 0,
    Published = 1,
    Closed = 2
}

public enum TallyItemFrequency
{
    /// <summary>Expected to be done every day of the tally's week (e.g. daily Qur'an reading).</summary>
    Daily = 0,

    /// <summary>Expected only on specific days of the week (e.g. fasting on Monday &amp; Thursday).</summary>
    SpecificDays = 1
}
