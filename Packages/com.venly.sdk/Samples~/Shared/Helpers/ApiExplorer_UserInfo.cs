using Venly.Models.Wallet;

public class ApiExplorer_UserInfo
{
    public const string DATAKEY_PROVIDER_USERINFO = "provider-user-info";

    public VyWalletDto Wallet { get; set; }
    public VyUserDto user { get; set; }
}