using CloudX.Shared;

namespace AccountDownloaderLibrary
{
    public static class CloudXInterfaceExtensions
    {
        public static Task<CloudResult<List<CloudVariable>>> GetAllVariables(this CloudXInterface i, string ownerId)
        {
            return i.GET<List<CloudVariable>>($"api/{GetOwnerPath(ownerId)}/{ownerId}/vars");
        }
        static string GetOwnerPath(string ownerId)
        {
            return IdUtil.GetOwnerType(ownerId) switch
            {
                OwnerType.Group => "groups",
                OwnerType.User => "users",
                _ => throw new Exception("Invalid owner type: " + ownerId),
            };
        }

        public static Task<CloudResult<List<CloudVariableDefinition>>> ListVariableDefinitions(this CloudXInterface i, string ownerId)
        {
            return i.GET<List<CloudVariableDefinition>>($"api/{GetOwnerPath(ownerId)}/{ownerId}/vardefs");
        }

        public static Task<CloudResult<Storage>> GetStorage(this CloudXInterface i, string ownerId)
        {
            return i.GET<Storage>($"api/{GetOwnerPath(ownerId)}/{ownerId}/storage");
        }

        public static Task<CloudResult<Storage>> GetMemberStorage(this CloudXInterface i, string ownerId, string userId)
        {
            return i.GET<Storage>($"api/groups/{ownerId}/members/{userId}/storage");
        }
    }
}
