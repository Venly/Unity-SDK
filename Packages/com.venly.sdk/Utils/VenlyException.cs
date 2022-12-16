using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using Venly.Models;
using Venly.Models.Internal;

public class VenlyException : Exception
{
    public static implicit operator VenlyException(ArgumentException ex) => new (ex);

    public VenlyException(Exception ex) : base(ex.Message, ex)
    {
    }

    public VenlyException(string msg) : base(msg)
    {
    }

    public static void RaiseUnhandled(VenlyException unhandledException)
    {
        UnityEngine.Debug.LogException(new Exception("[Venly Unhandled Exception]", unhandledException));
    }

    public static VenlyException Argument(string message, string paramName, [CallerMemberName] string callerName = "")
    {
        var exception =  new VenlyException($"[Argument Exception] Parameter \'{paramName}\' (Method: {callerName}) is invalid!\nReason: {message}");
        //Todo set StackTrace
        return exception;
    }

    public static VenlyException ParsingError(string message)
    {
        return new VenlyException($"[Parsing Error] {message}");
    }

    public static VenlyException HttpResponseError(HttpResponseMessage msg, RequestData requestData = null)
    {
        if(requestData!=null) 
            return new VenlyException($"[HTTP Response] Code {msg.StatusCode} : {msg.ReasonPhrase}\nRequest Details: [{requestData.Method}] || {requestData.Uri}");

        return new VenlyException($"[HTTP Response] Code {msg.StatusCode} : {msg.ReasonPhrase}\nNo Request Details available.");
    }

    public static VenlyException ApiResponseError(VyResponse apiResponse, HttpResponseMessage responseMsg, RequestData requestData)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"[API Response] The reponse for {requestData.Uri} returned {apiResponse.Errors.Length} error(s). ({responseMsg.ReasonPhrase})");

        int counter = 1;
        foreach (var err in apiResponse.Errors)
        {
            sb.AppendLine($"({counter}) [code={err.Code}, msg={err.Message}, traceCode={err.TraceCode}]");
            ++counter;
        }

        return new VenlyException(sb.ToString());
    }
}
