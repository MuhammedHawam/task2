namespace PIF.EBP.Core.Authorization.Users
{
    public class User : UserBase<string>
    {
        public string EmailAddress { get; set; }
        public string UserName { get; set; }
    }

    public class UserBase<T>
    {
        public T Id { get; set; }
    }
}
