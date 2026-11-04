namespace Siniskalc.Homm3.ViewModels
{
    using Siniskalc.Homm3.Models;

    public static class MapSections
    {
        public const int Merchants = 0;
        public const int BankDetails = 1;
        public const int DragonUtopias = BankDetails;
        public const int CreatureBanks = 2;
        public const int Overview = 3;
        public const int CyclopsStockpiles = 4;
        public const int DwarvenTreasuries = 5;
        public const int ImpCaches = 6;
        public const int MedusaStores = 7;
        public const int NagaBanks = 8;

        public static CreatureBankKind? BankKind(int section) => section switch
        {
            DragonUtopias => CreatureBankKind.DragonUtopia,
            CyclopsStockpiles => CreatureBankKind.CyclopsStockpile,
            DwarvenTreasuries => CreatureBankKind.DwarvenTreasury,
            ImpCaches => CreatureBankKind.ImpCache,
            MedusaStores => CreatureBankKind.MedusaStores,
            NagaBanks => CreatureBankKind.NagaBank,
            _ => null,
        };

        public static int ForBank(CreatureBankKind kind) => kind switch
        {
            CreatureBankKind.DragonUtopia => DragonUtopias,
            CreatureBankKind.CyclopsStockpile => CyclopsStockpiles,
            CreatureBankKind.DwarvenTreasury => DwarvenTreasuries,
            CreatureBankKind.ImpCache => ImpCaches,
            CreatureBankKind.MedusaStores => MedusaStores,
            _ => NagaBanks,
        };
    }
}
