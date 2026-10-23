// Call this to register your module to main application
var moduleName = 'virtoCommerce.catalogQuality';

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .config(['$stateProvider',
        function ($stateProvider) {
            $stateProvider
                .state('workspace.CatalogQualityState', {
                    url: '/catalog-quality',
                    templateUrl: '$(Platform)/Scripts/common/templates/home.tpl.html',
                    controller: [
                        'platformWebApp.bladeNavigationService',
                        function (bladeNavigationService) {
                            var newBlade = {
                                id: 'qualityTargetList',
                                controller: 'virtoCommerce.catalogQuality.qualityTargetListController',
                                template: 'Modules/$(VirtoCommerce.CatalogQuality)/Scripts/blades/quality-target-list.html',
                                isClosingDisabled: true,
                            };
                            bladeNavigationService.showBlade(newBlade);
                        }
                    ]
                });
        }
    ])
    .run(['platformWebApp.mainMenuService', '$state', 'platformWebApp.widgetService', 'virtoCommerce.catalogQuality.qualityDataTypes',
        function (mainMenuService, $state, widgetService, qualityDataTypes) {
            //Register module in main menu
            var menuItem = {
                path: 'browse/catalog-quality',
                icon: 'fa fa-bullseye',
                title: 'catalog-quality.main-menu.title',
                priority: 100,
                action: function () { $state.go('workspace.CatalogQualityState'); },
                permission: 'catalog-quality:access',
            };
            mainMenuService.addMenuItem(menuItem);

            // Product details: Product data quality score widget
            var productQualityWidget = {
                entityType: 'VirtoCommerce.CatalogModule.Core.Model.CatalogProduct',
                controller: 'virtoCommerce.catalogQuality.qualityDataWidgetController',
                template: 'Modules/$(VirtoCommerce.CatalogQuality)/Scripts/widgets/quality-data-widget.tpl.html',
                permission: 'catalog-quality:read',
            };
            widgetService.registerWidget(productQualityWidget, 'itemDetail');

            // Catalog product quality scoring entity type registration
            qualityDataTypes.addType({
                caption: 'quality-data-entity-types.catalog-product',
                value: 'VirtoCommerce.CatalogModule.Core.Model.CatalogProduct'
            });
        }
    ]);
