namespace ChargePadLine.Application.Trace.Production.TraceInformation
{
    public record QryTraceInfoList(
        int PageIndex,
        int PageSize,

        Guid? Id,
        uint? Vsn,
        string? PIN
    );
}

