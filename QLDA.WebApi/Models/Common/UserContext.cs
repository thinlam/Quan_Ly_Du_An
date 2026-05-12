using QLDA.WebApi.Models.Common.Interfaces;

namespace QLDA.WebApi.Models.Common;

public class UserContext : IUserContext {
    public long UserId { get; set; }
}
