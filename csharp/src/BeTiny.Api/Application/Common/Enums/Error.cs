using System.ComponentModel;

namespace BeTiny.Api.Application.Common.Enums
{
    public enum Error
    {
        [Description("Not Found")]
        NotFound,
        [Description("Validation")]
        Validation,
        [Description("Infrastructure")]
        Infrastructure
    }
}
