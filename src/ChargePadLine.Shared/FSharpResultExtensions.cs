using Microsoft.FSharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Shared
{
    public static class FSharpResultExtensions
    {
        public static Task<FSharpResult<TOk, TError>> ToTask<TOk, TError>(this FSharpResult<TOk, TError> result)
        {
            return Task.FromResult(result);
        }

        public static TResult Match<TOk, TError, TResult>(this FSharpResult<TOk, TError> result, Func<TError, TResult> handlerError, Func<TOk, TResult> handleOk)
        {
            if (!result.IsError)
            {
                return handleOk(result.ResultValue);
            }

            return handlerError(result.ErrorValue);
        }

        public static void Match<TOk, TError>(this FSharpResult<TOk, TError> result, Action<TError> handlerError, Action<TOk> handleOk)
        {
            if (result.IsError)
            {
                handlerError(result.ErrorValue);
            }
            else
            {
                handleOk(result.ResultValue);
            }
        }

        public static FSharpResult<TOk, TError2> SelectError<TOk, TError1, TError2>(this FSharpResult<TOk, TError1> result, Func<TError1, TError2> mapError)
        {
            Func<TError1, TError2> mapError2 = mapError;
            return result.Match((TError1 err) => mapError2(err).ToErrResult<TOk, TError2>(), (TOk ok) => ok.ToOkResult<TOk, TError2>());
        }

        public static FSharpResult<TOk2, TError> SelectOk<TOk1, TError, TOk2>(this FSharpResult<TOk1, TError> result, Func<TOk1, TOk2> mapOk)
        {
            Func<TOk1, TOk2> mapOk2 = mapOk;
            return result.Match((TError err) => err.ToErrResult<TOk2, TError>(), (TOk1 ok) => mapOk2(ok).ToOkResult<TOk2, TError>());
        }

        public static FSharpResult<TOk2, TError> Select<TOk1, TError, TOk2>(this FSharpResult<TOk1, TError> result, Func<TOk1, TOk2> mapOk)
        {
            return result.SelectOk(mapOk);
        }

        public static FSharpResult<TOk3, TError> SelectMany<TOk1, TError, TOk2, TOk3>(this FSharpResult<TOk1, TError> result, Func<TOk1, FSharpResult<TOk2, TError>> select, Func<TOk1, TOk2, TOk3> proj)
        {
            Func<TOk1, FSharpResult<TOk2, TError>> select2 = select;
            Func<TOk1, TOk2, TOk3> proj2 = proj;
            return result.Match((TError err1) => err1.ToErrResult<TOk3, TError>(), (TOk1 ok1) => select2(ok1).Match((TError err2) => err2.ToErrResult<TOk3, TError>(), (TOk2 ok2) => proj2(ok1, ok2).ToOkResult<TOk3, TError>()));
        }

        public static FSharpResult<TOk2, TError> Bind<TOk1, TOk2, TError>(this FSharpResult<TOk1, TError> result, Func<TOk1, FSharpResult<TOk2, TError>> fn)
        {
            Func<TOk1, FSharpResult<TOk2, TError>> fn2 = fn;
            return result.Match((TError err) => err.ToErrResult<TOk2, TError>(), (TOk1 ok) => fn2(ok));
        }

        public static FSharpResult<TOk, TError> Where<TOk, TError>(this FSharpResult<TOk, TError> result, Func<TOk, FSharpResult<ValueTuple, TError>> predicate)
        {
            Func<TOk, FSharpResult<ValueTuple, TError>> predicate2 = predicate;
            return result.Match((TError err) => err.ToErrResult<TOk, TError>(), (TOk ok) => predicate2(ok).Match((TError err) => err.ToErrResult<TOk, TError>(), (ValueTuple _) => ok.ToOkResult<TOk, TError>()));
        }

        //public static Task<FSharpResult<TOk, TError>> Where<TOk, TError>(this FSharpResult<TOk, TError> result, Func<TOk, Task<FSharpResult<ValueTuple, TError>>> predicate)
        //{
        //    Func<TOk, Task<FSharpResult<ValueTuple, TError>>> predicate2 = predicate;
        //    return result.Match((TError err) => err.ToErrResult<TOk, TError>().ToTask(), (TOk ok) => predicate2(ok).Match((TError err) => err.ToErrResult<TOk, TError>().ToTask(), (ValueTuple _) => ok.ToOkResult<TOk, TError>().ToTask()));
        //}

        public static FSharpResult<TOk, TError> OrElse<TOk, TError>(this FSharpResult<TOk, TError> result, FSharpResult<TOk, TError> elseResult)
        {
            FSharpResult<TOk, TError> elseResult2 = elseResult;
            FSharpResult<TOk, TError> result2 = result;
            return result2.Match((TError err) => elseResult2, (TOk ok) => result2);
        }

        public static FSharpResult<TOk, TError> OrElse<TOk, TError>(this FSharpResult<TOk, TError> result, Func<TError, FSharpResult<TOk, TError>> elseResult)
        {
            Func<TError, FSharpResult<TOk, TError>> elseResult2 = elseResult;
            FSharpResult<TOk, TError> result2 = result;
            return result2.Match((TError err) => elseResult2(err), (TOk ok) => result2);
        }

        public static FSharpResult<IList<TOk>, TError> ExecuteOneByOne<TOk, TError>(this IEnumerable<Func<FSharpResult<TOk, TError>>> funcs)
        {
            List<TOk> list = new List<TOk>();
            foreach (Func<FSharpResult<TOk, TError>> func in funcs)
            {
                FSharpResult<TOk, TError> fSharpResult = func();
                if (fSharpResult.IsError)
                {
                    return fSharpResult.ErrorValue.ToErrResult<IList<TOk>, TError>();
                }

                list.Add(fSharpResult.ResultValue);
            }

            return list.ToOkResult<IList<TOk>, TError>();
        }
    }
}