angular.module('virtoCommerce.catalogQuality')
    .controller('virtoCommerce.catalogQuality.qualityDataListController', [
        '$scope', 'virtoCommerce.catalogQuality.webApi', 'platformWebApp.bladeUtils', 'platformWebApp.uiGridHelper', 'uiGridConstants',
        function ($scope, api, bladeUtils, uiGridHelper, uiGridConstants) {
            var blade = $scope.blade;
            var bladeNavigationService = bladeUtils.bladeNavigationService;
            $scope.uiGridConstants = uiGridConstants;

            blade.title = 'catalog-quality.blades.quality-data-list.title';
            blade.headIcon = 'fa fa-check-square-o';

            var filter = $scope.filter = {};
            filter.criteriaChanged = function () {
                if ($scope.pageSettings.currentPage > 1) {
                    $scope.pageSettings.currentPage = 1;
                } else {
                    blade.refresh();
                }
            };

            blade.refresh = function () {
                blade.isLoading = true;

                var criteria = {
                    keyword: filter.keyword,
                    sort: uiGridHelper.getSortExpression($scope),
                    skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                    take: $scope.pageSettings.itemsPerPageCount,
                };

                api.search(criteria, function (data) {
                    blade.isLoading = false;
                    $scope.pageSettings.totalItems = data.totalCount;
                    blade.currentEntities = data.results;
                }, function () {
                    blade.isLoading = false;
                });
            };

            $scope.selectNode = function (node) {
                $scope.selectedNodeId = node.id;

                var newBlade = {
                    id: 'qualityDataDetails',
                    entityId: node.entityId,
                    entityType: node.entityType,
                    controller: 'virtoCommerce.catalogQuality.qualityDataDetailsController',
                    template: 'Modules/$(VirtoCommerce.CatalogQuality)/Scripts/blades/quality-data-details.html',
                };
                bladeNavigationService.showBlade(newBlade, blade);
            };

            blade.toolbarCommands = [
                {
                    name: 'platform.commands.refresh',
                    icon: 'fa fa-refresh',
                    executeMethod: blade.refresh,
                    canExecuteMethod: function () { return true; },
                },
            ];

            // ui-grid
            $scope.setGridOptions = function (gridOptions) {
                uiGridHelper.initialize($scope, gridOptions, function () {
                    uiGridHelper.bindRefreshOnSortChanged($scope);
                });
                // Triggers the initial blade.refresh()
                bladeUtils.initializePagination($scope);
            };
        }]);
