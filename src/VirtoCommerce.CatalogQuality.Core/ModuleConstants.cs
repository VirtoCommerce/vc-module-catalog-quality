using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CatalogQuality.Core;

public static class ModuleConstants
{
    public static class Security
    {
        //public static class Permissions
        //{
        //    public const string Access = "catalog-quality:access";
        //    public const string Create = "catalog-quality:create";
        //    public const string Read = "catalog-quality:read";
        //    public const string Update = "catalog-quality:update";
        //    public const string Delete = "catalog-quality:delete";

        //    public static string[] AllPermissions { get; } =
        //    [
        //        Access,
        //        Create,
        //        Read,
        //        Update,
        //        Delete,
        //    ];
        //}
    }

    public static class Settings
    {
        public static class General
        {
            public static SettingDescriptor CatalogQualityEnabled { get; } = new()
            {
                Name = "CatalogQuality.Enabled",
                GroupName = "CatalogQuality|General",
                ValueType = SettingValueType.Boolean,
                DefaultValue = false,
            };

            public static IEnumerable<SettingDescriptor> AllGeneralSettings
            {
                get
                {
                    yield return CatalogQualityEnabled;
                }
            }
        }

        public static IEnumerable<SettingDescriptor> AllSettings
        {
            get
            {
                return General.AllGeneralSettings;
            }
        }
    }

    public static class QualityEntityType
    {
        public const string CatalogProduct = "VirtoCommerce.CatalogModule.Core.Model.CatalogProduct";
    }
}
