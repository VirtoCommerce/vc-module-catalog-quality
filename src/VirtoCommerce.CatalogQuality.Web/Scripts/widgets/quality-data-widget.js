angular.module('virtoCommerce.catalogQuality')
    .controller('virtoCommerce.catalogQuality.qualityDataWidgetController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogQuality.webApi',
        function ($scope, bladeNavigationService, catalogQualityApi) {
            var blade = $scope.widget.blade;
            var widget = $scope.widget;

            $scope.widget.refresh = function () {
                blade.loading = true;
                $scope.qualityDataScore = 'N/A';

                catalogQualityApi.getByEntity({ entityId: blade.currentEntityId, entityType: widget.entityType }, function (data) {
                    if (data) {
                        $scope.qualityDataScore = data.readinessScore;
                    }
                    blade.isLoading = false;
                }, function () {
                    blade.isLoading = false;
                });
            }

            $scope.openBlade = function () {
                if (!blade.isLoading) {
                    var newBlade = {
                    id: 'qualityDataDetails',
                    entityId: blade.currentEntityId,
                    entityType: widget.entityType,
                    controller: 'virtoCommerce.catalogQuality.qualityDataDetailsController',
                    template: 'Modules/$(VirtoCommerce.CatalogQuality)/Scripts/blades/quality-data-details.html',
                    };

                    bladeNavigationService.showBlade(newBlade, blade);
                }
            };

            $scope.widget.refresh();

        }]);
