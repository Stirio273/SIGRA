using SIGRA.Domain.AIsupport;

namespace SIGRA.Services;


public interface IAIResponseParser
{
    AISupportResponse Parse(string rawLlmResponse);
}
