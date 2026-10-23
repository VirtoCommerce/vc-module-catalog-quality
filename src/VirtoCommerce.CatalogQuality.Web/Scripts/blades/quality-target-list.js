angular.module('virtoCommerce.catalogQuality')
    .controller('virtoCommerce.catalogQuality.qualityTargetListController', [
        '$scope', '$translate', 'virtoCommerce.catalogQuality.webApi', 'platformWebApp.bladeUtils', 'platformWebApp.uiGridHelper', 'platformWebApp.dialogService', 'uiGridConstants', 'virtoCommerce.catalogQuality.qualityDataTypes',
        function ($scope, $translate, api, bladeUtils, uiGridHelper, dialogService, uiGridConstants, qualityDataTypes) {
            var blade = $scope.blade;
            var bladeNavigationService = bladeUtils.bladeNavigationService;
            $scope.uiGridConstants = uiGridConstants;

            // Entity context: opened from an entity's quality widget, e.g. on a product (blade.entityId + blade.entityType are set).
            // Selecting a target then opens the entity's quality data for that target instead of the target editor.
            var isEntityContext = !!blade.entityId;

            blade.title = 'catalog-quality.blades.quality-target-list.title';
            blade.subtitle = isEntityContext ? blade.entityName : undefined;
            blade.headIcon = 'fa fa-bullseye';

            blade.refresh = function () {
                blade.isLoading = true;

                var criteria = {
                    sort: uiGridHelper.getSortExpression($scope),
                    skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                    take: $scope.pageSettings.itemsPerPageCount,
                };

                // In entity context show only the targets defined for this entity type
                if (isEntityContext) {
                    criteria.entityType = blade.entityType;
                }

                api.targetSearch(criteria, function (data) {
                    blade.isLoading = false;
                    $scope.pageSettings.totalItems = data.totalCount;
                    blade.currentEntities = data.results;
                    blade.currentEntities.forEach(x => {
                        var typeInfo = qualityDataTypes.getTypeInfo(x.entityType);
                        x.entityTypeCaption = typeInfo ? $translate.instant(typeInfo.caption) : x.entityType;
                    });
                }, function () {
                    blade.isLoading = false;
                });
            };

            function openDetails(newBlade) {
                bladeNavigationService.showBlade(angular.extend({
                    id: 'qualityTargetDetails',
                    controller: 'virtoCommerce.catalogQuality.qualityTargetDetailsController',
                    template: 'Modules/$(VirtoCommerce.CatalogQuality)/Scripts/blades/quality-target-details.html',
                }, newBlade), blade);
            }

            $scope.selectNode = function (node) {
                $scope.selectedNodeId = node.id;

                if (isEntityContext) {
                    bladeNavigationService.showBlade({
                        id: 'qualityDataDetails',
                        entityId: blade.entityId,
                        entityType: blade.entityType,
                        entityName: blade.entityName,
                        targetId: node.id,
                        targetName: node.name,
                        controller: 'virtoCommerce.catalogQuality.qualityDataDetailsController',
                        template: 'Modules/$(VirtoCommerce.CatalogQuality)/Scripts/blades/quality-data-details.html',
                    }, blade);
                } else {
                    openDetails({ currentEntityId: node.id });
                }
            };

            function closeChildrenBlades() {
                angular.forEach(blade.childrenBlades.slice(), function (child) {
                    bladeNavigationService.closeBlade(child);
                });
            }

            function deleteList(selection) {
                dialogService.showConfirmationDialog({
                    id: 'confirmDeleteQualityTargets',
                    title: 'catalog-quality.dialogs.quality-target-delete.title',
                    message: 'catalog-quality.dialogs.quality-target-delete.message',
                    callback: function (remove) {
                        if (remove) {
                            closeChildrenBlades();
                            blade.isLoading = true;

                            api.targetDelete({ ids: _.pluck(selection, 'id') }, function () {
                                blade.refresh();
                            }, function (error) {
                                bladeNavigationService.setError('Error ' + error.status, blade);
                            });
                        }
                    },
                });
            }

            // Targets are managed from the main menu; in entity context the list is for picking only
            blade.toolbarCommands = isEntityContext ? [] : [
                {
                    name: 'platform.commands.add',
                    icon: 'fas fa-plus',
                    executeMethod: function () {
                        $scope.selectedNodeId = undefined;
                        openDetails({ isNew: true });
                    },
                    canExecuteMethod: function () { return true; },
                    permission: 'catalog-quality:update',
                },
                {
                    name: 'platform.commands.delete',
                    icon: 'fas fa-trash-alt',
                    executeMethod: function () {
                        deleteList($scope.gridApi.selection.getSelectedRows());
                    },
                    canExecuteMethod: function () {
                        return $scope.gridApi && _.any($scope.gridApi.selection.getSelectedRows());
                    },
                    permission: 'catalog-quality:delete',
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
