namespace OzunuInkisaf.Contracts.Common;

// These mirror OzunuInkisaf.Domain.Enums exactly (same underlying int
// values) so the Application layer can cast between them freely, while
// keeping the MAUI client from ever needing to reference the Domain
// project.

public enum UserRoleDto
{
    User = 0,
    Admin = 1
}

public enum BookCategoryDto
{
    General = 0,
    Quran = 1
}

public enum BookStatusDto
{
    Active = 0,
    Archived = 1
}

public enum DuaCategoryDto
{
    Daily = 0,
    Travel = 1,
    Distress = 2,
    Sleep = 3,
    Other = 4
}

public enum KhatimCycleStatusDto
{
    InProgress = 0,
    Completed = 1
}

public enum TallyStatusDto
{
    Draft = 0,
    Published = 1,
    Closed = 2
}

public enum TallyItemFrequencyDto
{
    Daily = 0,
    SpecificDays = 1
}

public enum PointsSourceDto
{
    TallyEntry = 0,
    KhatimJuzCompleted = 1,
    BookCompleted = 2
}
