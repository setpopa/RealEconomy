using Rocket.Unturned.Player;
using Steamworks;
using HarmonyLib;


namespace RealEconomy.Pathes
{
    [HarmonyPatch]
    internal class UconomyPatches
    {   
        
        public UconomyPatches() { }

        public static decimal OnBalanceChecked(string SteamID)
        {
            return RealEconomy.Instance.CheckBalance(UnturnedPlayer.FromCSteamID(new CSteamID(ulong.Parse(SteamID))));
        }
        public static void BalanceUpdated(string SteamID, decimal amt)
        {
            RealEconomy.Instance.UpdateMoney(UnturnedPlayer.FromCSteamID(new CSteamID(ulong.Parse(SteamID))), (int)amt);
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(fr34kyn01535.Uconomy.DatabaseManager), nameof(fr34kyn01535.Uconomy.DatabaseManager.GetBalance))]
        public static bool GetBalance(ref decimal __result, string id) 
        {
            __result = OnBalanceChecked(id);
            return false;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(fr34kyn01535.Uconomy.DatabaseManager), nameof(fr34kyn01535.Uconomy.DatabaseManager.IncreaseBalance))]
        public static bool IncreaseBalance(ref decimal __result, string id, decimal increaseBy) 
        {
            BalanceUpdated(id, increaseBy);
            __result = OnBalanceChecked(id);
            return false;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(fr34kyn01535.Uconomy.DatabaseManager), "CheckSchema")]
        public static bool CheckSchema()
        {
            return false;
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(fr34kyn01535.Uconomy.DatabaseManager), nameof(fr34kyn01535.Uconomy.DatabaseManager.CheckSetupAccount))]
        public static bool CheckSetupAccount(Steamworks.CSteamID id)
        {
            RealEconomy.Instance.OnPlayerConnected(UnturnedPlayer.FromCSteamID(id));
            return false;
        }                       
    }
}
