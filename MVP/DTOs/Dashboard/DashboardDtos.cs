namespace MVP.DTOs.Dashboard;

public record DashboardResponse(
    int ActiveProjects,
    int CompletedProjects,
    int OpenJobApplications,
    int WaitingJobApplications,
    int UpcomingCalendarEvents,
    IEnumerable<UpcomingEventResponse> UpcomingEvents,
    IEnumerable<RecentProjectResponse> RecentProjects,
    IEnumerable<RecentJobApplicationResponse> RecentJobApplications
);

public record UpcomingEventResponse(
    int Id,
    string Title,
    DateTime StartDateTime,
    DateTime EndDateTime,
    string EventType
);

public record RecentProjectResponse(
    int Id,
    string Title,
    string Status,
    string Priority,
    DateOnly? EndDate
);

public record RecentJobApplicationResponse(
    int Id,
    string CompanyName,
    string PositionTitle,
    string Status,
    DateOnly? ApplicationDate
);
