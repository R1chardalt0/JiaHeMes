using Microsoft.FSharp.Core;

namespace ChargePadLine.Shared
{
    public static class FSharpResultFactory
    {
        public static FSharpResult<TOk, TError> ToOkResult<TOk, TError>(this TOk ok)
        {
            return FSharpResult<TOk, TError>.NewOk(ok);
        }

        public static FSharpResult<TOk, TError> ToErrResult<TOk, TError>(this TError err)
        {
            return FSharpResult<TOk, TError>.NewError(err);
        }
    }
}
