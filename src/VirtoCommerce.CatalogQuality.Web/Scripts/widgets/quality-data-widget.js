angular.module('virtoCommerce.catalogQuality')
    .controller('virtoCommerce.catalogQuality.qualityDataWidgetController', ['$scope', 'platformWebApp.bladeNavigationService',
        function ($scope, bladeNavigationService) {
            var blade = $scope.widget.blade;
            var widget = $scope.widget;

            $scope.widget.refresh = function () {
                blade.loading = false;
            };

            // Opens the targets list for this product; selecting a target opens the product's quality data for it
            $scope.openBlade = function () {
                if (!blade.isLoading) {
                    var newBlade = {
                        id: 'qualityTargetList',
                        entityId: blade.currentEntityId,
                        entityType: widget.entityType,
                        entityName: blade.currentEntity ? blade.currentEntity.name : undefined,
                        controller: 'virtoCommerce.catalogQuality.qualityTargetListController',
                        template: 'Modules/$(VirtoCommerce.CatalogQuality)/Scripts/blades/quality-target-list.html',
                    };

                    bladeNavigationService.showBlade(newBlade, blade);
                }
            };

            $scope.widget.refresh();
        }]);
