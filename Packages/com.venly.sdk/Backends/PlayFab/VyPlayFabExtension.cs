using System.Net.Http;
using Venly.Models;
using Venly.Models.Internal;

namespace Venly.Backends.PlayFab
{
    public class VyPlayFabExtension:IBackendExtension
    {
        private readonly IVenlyRequester _requester;

        public VyPlayFabExtension(VyPlayfabRequester requester)
        {
            _requester = requester;
        }

        public VyTask<VyWallet> CreateWalletForUser(VyParam_CreateWallet walletDetails)
        {
            var reqData = RequestData.Get("user_create_wallet", eVyApiEndpoint.Extension).AddJsonContent(walletDetails);
            return _requester.MakeRequest<VyWallet>(reqData);
        }

        public VyTask<VyWallet> GetWalletForUser()
        {
            var reqData = RequestData.Get("user_get_wallet", eVyApiEndpoint.Extension);
            return _requester.MakeRequest<VyWallet>(reqData);
        }
    }
}